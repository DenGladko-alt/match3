using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Utility;
using Random = UnityEngine.Random;

namespace Match3
{
    public class GameBoard : MonoBehaviourService<GameBoard>
    {
        public event Action OnGameBoardSetup;
        
        #region Variables
        
        private int currentDestroyGroup;
        private HashSet<Gem> matchedGems = new HashSet<Gem>();
        private Dictionary<int, HashSet<Gem>> groupedMatches;
        private readonly HashSet<Gem> markedForDestruction = new HashSet<Gem>();
        
        private GemSpawnManager gemSpawnManager;
        private GameVariablesService gameVariables;
        
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Gem[,] AllGems { get; private set; }

        #endregion

        #region MonoBehaviour

        private void Start()
        {
            ServiceManager.Instance.TryGet(out gemSpawnManager);
            ServiceManager.Instance.TryGet(out gameVariables);
            
            Width = gameVariables.LevelConfig.BoardWidth;
            Height = gameVariables.LevelConfig.BoardHeight;
            
            AllGems = new Gem[Width, Height];
            
            OnGameBoardSetup?.Invoke();
            
            gemSpawnManager.Setup(this);
        }

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

        #region Gems swapping

        private void OnGemsSwipe(Gem firstGem, Gem secondGem)
        {
            StartCoroutine(SwapGemsCoroutine(firstGem, secondGem));
        }

        private IEnumerator SwapGemsCoroutine(Gem firstGem, Gem secondGem)
        {
            SwapGems(firstGem, secondGem, true);
            
            yield return new WaitForSeconds(gameVariables.GameSettings.SwapDuration);

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
            firstGem.MoveToPositionIndex();

            AllGems[firstGemPosition.x, firstGemPosition.y] = secondGem;
            secondGem.PosIndex = firstGemPosition;
            secondGem.MovedByPlayer = markAsMovedByPlayer;
            secondGem.MoveToPositionIndex();
        }
        
        #endregion

        #region Matching

        private bool FindMatchingGroups()
        {
            markedForDestruction.Clear();
            
            matchedGems = Match3Utilities.GetMatchingGemsFromBoard(this);
            groupedMatches = Match3Utilities.GetGroupedMatchingGems(matchedGems);
            markedForDestruction.AddRange(matchedGems);
            
            HashSet<Gem> specialGems = Match3Utilities.GetSpecialGemsFromHashSet(matchedGems);
            
            FindGemsDestroyedBySpecialGems(specialGems);
            
            return groupedMatches.Any();
        }

        private void FindGemsDestroyedBySpecialGems(HashSet<Gem> specialGems)
        {
            while (true)
            {
                if (specialGems.Count == 0) return;

                HashSet<Gem> newFoundSpecialGems = new HashSet<Gem>();

                foreach (Gem gem in specialGems)
                {
                    List<Vector3Int> patternPositions = gem.DestroyPattern?.GetPattern(gem.PosIndex);

                    if (patternPositions == null) return;

                    float accumulatedDelay = 0;

                    for (int i = 0; i < patternPositions.Count; i++)
                    {
                        Vector3Int pos = patternPositions[i];

                        if (pos.x < 0 || pos.x >= Width || pos.y < 0 || pos.y >= Height) continue;

                        Gem gemToDestroy = AllGems[pos.x, pos.y];

                        if (!gemToDestroy) continue;

                        if (markedForDestruction.Contains(gemToDestroy))
                        {
                            gemToDestroy.DestroyDelay = Math.Min(gemToDestroy.DestroyDelay, patternPositions[i].z * gameVariables.GameSettings.SimpleGemsDestructionDelay + gem.DestroyDelay);
                        }
                        else
                        {
                            gemToDestroy.DestroyDelay = patternPositions[i].z * gameVariables.GameSettings.SimpleGemsDestructionDelay + gem.DestroyDelay;
                            markedForDestruction.Add(gemToDestroy);

                            if (gemToDestroy.IsSpecial())
                            {
                                newFoundSpecialGems.Add(gemToDestroy);
                            }
                        }

                        accumulatedDelay = Math.Max(accumulatedDelay, gemToDestroy.DestroyDelay);
                    }

                    gem.DestroyDelay = accumulatedDelay + gameVariables.GameSettings.SpecialGemsDestructionDelay;
                }

                specialGems = newFoundSpecialGems;
            }
        }

        #endregion

        #region Destruction

        private void DestroyMatches()
        {
            StartCoroutine(DestroyMarkedGemsCoroutine());
        }

        // Can be remade to return Special GemType by rule
        private bool ShouldSpawnSpecialGem(Gem gem)
        {
            if (groupedMatches.TryGetValue(gem.MergeGroup, out HashSet<Gem> gemsInGroup))
            {
                if (gemsInGroup.Count >= gameVariables.GameSettings.BombSpawnCountRule)
                {
                    return gem.MovedByPlayer;
                }
            }
            return false;
        }
        
        private IEnumerator DestroyMarkedGemsCoroutine()
        {
            HashSet<Gem> gemsToDestroy = new HashSet<Gem>(markedForDestruction);
            HashSet<Gem> destroyedGems = new HashSet<Gem>();
            float elapsedTime = 0f;

            while (gemsToDestroy.Count > 0)
            {
                elapsedTime += Time.deltaTime;
                
                foreach (Gem gem in gemsToDestroy)
                {
                    if (!gem) continue;
                    
                    if (elapsedTime >= gem.DestroyDelay)
                    {
                        bool shouldSpawnGem = ShouldSpawnSpecialGem(gem);
                        DestroyGem(gem);
                        if (shouldSpawnGem)
                        {
                            gemSpawnManager.SpawnGem(GemType.Bomb, gem.PosIndex, this);
                        }
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
            
            StartCoroutine(DecreaseRowCoroutine());
        }
        
        private void DestroyGem(Gem gem)
        {
            if (!gem) return;
            
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
                        int gemToUse = Random.Range(0, gameVariables.LevelConfig.Gems.Length);
                        gemSpawnManager.SpawnGem(new Vector2Int(x, y + gameVariables.GameSettings.GemDropHeight), 
                            gameVariables.LevelConfig.Gems[gemToUse], this);
                    }
                }
            }

            CheckMisplacedGems();
        }
        
        private IEnumerator DecreaseRowCoroutine()
        {
            yield return new WaitForSeconds(gameVariables.GameSettings.FallingGemsDelay);
            
            int nullCounter = 0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Gem curGem = AllGems[x, y];
                    if (!curGem)
                    {
                        nullCounter++;
                    }
                    else if (nullCounter > 0)
                    {
                        curGem.PosIndex.y -= nullCounter;
                        curGem.MoveToPositionIndex();
                        AllGems[x, y - nullCounter] = curGem;
                        AllGems[x, y] = null;
                    }
                }

                nullCounter = 0;
            }

            StartCoroutine(FillBoardCoroutine());
        }

        private IEnumerator FillBoardCoroutine()
        {
            // TODO: Time delay move to config
            yield return new WaitForSeconds(0.5f);
            RefillBoard();
            // TODO: Time delay move to config
            yield return new WaitForSeconds(1f);
            
            // Wait and destroy matches if any
            if (FindMatchingGroups())
            {
                DestroyMatches();
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

