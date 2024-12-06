using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Match3
{
    public class GameBoard : MonoBehaviour
    {
        #region Variables

        [SerializeField] private int width = 7;
        [SerializeField] private int height = 7;
        [SerializeField] private GameLogic gameLogic;
        [SerializeField] private GemSpawnManager gemSpawnManager;

        private int currentDestroyGroup;
        
        private HashSet<Gem> matchedGems = new HashSet<Gem>();
        private IOrderedEnumerable<IGrouping<int, Gem>> groupedMatches;
        private readonly HashSet<Gem> markedForDestruction = new HashSet<Gem>();
        
        public int Width => width;
        public int Height => height;
        public Gem[,] AllGems { get; private set; }

        #endregion

        #region MonoBehaviour

        private void OnEnable()
        {
            // TODO: Remove InputHandler dependency
            InputHandler.OnGemsSwipe += OnGemsSwipe;
        }
        private void OnDisable()
        {
            // TODO: Remove InputHandler dependency
            InputHandler.OnGemsSwipe -= OnGemsSwipe;
        }

        #endregion

        public void Setup()
        {
            AllGems = new Gem[Width, Height];
        }

        #region Gems swapping

        private void OnGemsSwipe(Gem firstGem, Gem secondGem)
        {
            StartCoroutine(SwapGemsCoroutine(firstGem, secondGem));
        }

        private IEnumerator SwapGemsCoroutine(Gem firstGem, Gem secondGem)
        {
            gameLogic.SetState(GameState.Wait);
            
            SwapGems(firstGem, secondGem, true);

            // TODO: Time delay move to config
            yield return new WaitForSeconds(.5f);

            if (FindMatchingGroups())
            {
                DestroyMatches();
            }
            else
            {
                // Match not found, revert
                SwapGems(secondGem, firstGem, false);
            }
        }

        private void SwapGems(Gem firstGem, Gem secondGem, bool markAsMovedByPlayer)
        {
            Vector2Int firstGemPosition = firstGem.PosIndex;
            AllGems[secondGem.PosIndex.x, secondGem.PosIndex.y] = firstGem;
            firstGem.PosIndex = secondGem.PosIndex;
            firstGem.MovedByPlayer = markAsMovedByPlayer;

            AllGems[firstGemPosition.x, firstGemPosition.y] = secondGem;
            secondGem.PosIndex = firstGemPosition;
            secondGem.MovedByPlayer = markAsMovedByPlayer;
        }
        
        #endregion

        #region Matching

        private bool FindMatchingGroups()
        {
            matchedGems = Match3Utilities.GetMatchingGemsFromBoard(this);
            groupedMatches = Match3Utilities.GetGroupedMatchingGems(matchedGems);
            
            HashSet<Gem> specialGems = Match3Utilities.GetSpecialGemsFromHashSet(matchedGems);
            
            markedForDestruction.Clear();
            FindGemsDestroyedBySpecialGems(specialGems);
            
            return groupedMatches.Any();
        }
        
        private void FindGemsDestroyedBySpecialGems(HashSet<Gem> specialGems)
        {
            if (specialGems.Count == 0) return;
            
            foreach (Gem gem in specialGems.ToList())
            {
                List<Vector3Int> patternPositions = gem.DestroyPattern?.GetPattern(gem.PosIndex);
                
                if (patternPositions == null) return;

                int maxDestroyRadius = 0;
                
                for (int i = 0; i < patternPositions.Count; i++)
                {
                    Vector3Int pos = patternPositions[i];

                    if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) continue;

                    Gem gemToDestroy = AllGems[pos.x, pos.y];

                    if (gemToDestroy == null) continue;

                    if (matchedGems.Contains(gemToDestroy)) continue;

                    if (gemToDestroy.DestroyOrder > 0)
                    {
                        gemToDestroy.DestroyOrder = Math.Min(gemToDestroy.DestroyOrder, patternPositions[i].z + gem.DestroyOrder);
                    }
                    else
                    {
                        gemToDestroy.DestroyOrder = patternPositions[i].z + gem.DestroyOrder;
                    }
                    
                    if (markedForDestruction.Add(gemToDestroy))
                    {
                        maxDestroyRadius = Math.Max(maxDestroyRadius, gemToDestroy.DestroyOrder);
                        if (gemToDestroy.GemType.IsSpecial())
                        {
                            specialGems.Add(gemToDestroy);
                            FindGemsDestroyedBySpecialGems(specialGems);
                        }
                    }
                }
            }
            
            foreach (Gem gem in specialGems)
            {
                gem.DestroyOrder += gem.DestroyPattern.Radius;
            }
        }

        private void AddDestructionDelayToGems()
        {
            foreach (Gem gem in markedForDestruction)
            {
                gem.DestroyDelay = gem.DestroyOrder * 0.25f;
            }
        }

        #endregion

        #region Destruction

        private void DestroyMatches()
        {
            StartCoroutine(DestroyMatchedGemsCoroutine());
        }
        
        private IEnumerator DestroyMarkedGemsCoroutine()
        {
            HashSet<Gem> gemsToDestroy = new HashSet<Gem>(markedForDestruction);
            HashSet<Gem> destroyedGems = new HashSet<Gem>();
            float elapsedTime = 0f;

            while (gemsToDestroy.Count > 0)
            {
                elapsedTime += Time.deltaTime;

                // Iterate through a copy to avoid modifying the collection while iterating
                foreach (Gem gem in gemsToDestroy.ToList())
                {
                    if (gem == null) continue;

                    // Check if the gem's destruction delay has elapsed
                    if (elapsedTime >= gem.DestroyDelay)
                    {
                        DestroyGem(gem);
                        destroyedGems.Add(gem);
                    }
                }

                // Remove destroyed gems from the original set
                foreach (Gem destroyedGem in destroyedGems)
                {
                    gemsToDestroy.Remove(destroyedGem);
                }

                destroyedGems.Clear(); // Clear the destroyed set for the next iteration

                yield return null; // Wait until the next frame
            }
        }
        
        private IEnumerator DestroyMatchedGemsCoroutine()
        {
            List<IGrouping<int, Gem>> groupedMatchesList = groupedMatches.ToList();

            for (int i = 0; i < groupedMatchesList.Count; i++)
            {
                IGrouping<int, Gem> group = groupedMatchesList[i];

                // Check if the group has 4 or more matches
                bool isSpecialGroup = group.Count() >= GLOBAL_VARIABLES.SPECIAL_SPAWN_COUNT_RULE;

                var groupArray = group.ToArray(); // Convert the group to an array to avoid repeated enumeration
                for (int j = 0; j < groupArray.Length; j++)
                {
                    Gem gem = groupArray[j];
                    if (gem == null) continue;

                    bool movedByPlayer = gem.MovedByPlayer;
                    Vector2Int posIndex = gem.PosIndex;

                    DestroyGem(gem);

                    // Spawn a special gem only if the group is special and the gem was moved by the player
                    if (isSpecialGroup && movedByPlayer)
                    {
                        gemSpawnManager.SpawnGem(GemType.Bomb, posIndex);
                        isSpecialGroup = false; // Spawn only one special gem per group
                    }
                }
            }

            foreach (var gem in AllGems)
            {
                if (gem == null) continue;
                
                gem.MovedByPlayer = false;
            }
            
            // TODO: Move to config
            yield return new WaitForSeconds(0.25f);

            if (markedForDestruction.Count > 0)
            {
                IOrderedEnumerable<IGrouping<int, Gem>> groupedByDistance = markedForDestruction
                    .GroupBy(gem => gem.DestroyOrder)
                    .OrderBy(group => group.Key);

                foreach (var gem in groupedByDistance)
                {
                    for (int i = 0; i < gem.Count(); i++)
                    {
                        if (matchedGems.Contains(gem.ElementAt(i))) continue;
                        
                        DestroyGem(gem.ElementAt(i));
                    }
                
                    // TODO: Move to config
                    yield return new WaitForSeconds(1f);
                }
            }
            
            StartCoroutine(DecreaseRowCoroutine());
        }
        
        private void DestroyGem(Gem gem)
        {
            if (gem == null) return;
            
            Vector2Int gemPos = new Vector2Int(gem.PosIndex.x, gem.PosIndex.y);
            gem.DestroyGem();
            AllGems[gemPos.x, gemPos.y] = null;
        }

        #endregion

        #region Board manipulation

        private void RefillBoard()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Gem _curGem = AllGems[x, y];
                    if (_curGem == null)
                    {
                        int gemToUse = Random.Range(0, GameVariables.Instance.gems.Length);
                        gemSpawnManager.SpawnGem(new Vector2Int(x, y), GameVariables.Instance.gems[gemToUse]);
                    }
                }
            }

            CheckMisplacedGems();
        }
        
        private IEnumerator DecreaseRowCoroutine()
        {
            // TODO: Time delay move to config
            yield return new WaitForSeconds(0.25f);
            
            int nullCounter = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Gem curGem = AllGems[x, y];
                    if (curGem == null)
                    {
                        nullCounter++;
                    }
                    else if (nullCounter > 0)
                    {
                        curGem.PosIndex.y -= nullCounter;
                        AllGems[x, y - nullCounter] = curGem;
                        AllGems[x, y] = null;
                    }
                }

                nullCounter = 0;
            }

            StartCoroutine(FilledBoardCoroutine());
        }

        private IEnumerator FilledBoardCoroutine()
        {
            // TODO: Time delay move to config
            yield return new WaitForSeconds(0.5f);
            RefillBoard();
            // TODO: Time delay move to config
            yield return new WaitForSeconds(0.5f);
            
            // Wait and destroy matches if any
            if (FindMatchingGroups())
            {
                // TODO: Time delay move to config
                yield return new WaitForSeconds(0.5f);
                DestroyMatches();
            }
            else
            {
                // TODO: Time delay move to config
                yield return new WaitForSeconds(0.5f);
                // TODO: Make event based
                gameLogic.SetState(GameState.Move);
            }
        }
        
        private void CheckMisplacedGems()
        {
            List<Gem> foundGems = new List<Gem>();
            foundGems.AddRange(FindObjectsOfType<Gem>());
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Gem _curGem = AllGems[x, y];
                    if (foundGems.Contains(_curGem))
                        foundGems.Remove(_curGem);
                }
            }

            foreach (Gem g in foundGems)
            {
                Destroy(g.gameObject);
            }
        }

        #endregion
    }
}

