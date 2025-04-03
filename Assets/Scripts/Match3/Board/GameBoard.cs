using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utility;

namespace Match3
{
    public class GameBoard : MonoBehaviour
    {
        public event Action OnGameBoardSetup;
        
        #region Variables
        
        [SerializeField] private Transform gemsContainer;
        
        private int currentDestroyGroup;
        private HashSet<Gem> matchedGems = new HashSet<Gem>();
        private Dictionary<int, HashSet<Gem>> groupedMatches;
        private readonly HashSet<Gem> markedForDestruction = new HashSet<Gem>();
        
        private GemSpawnManager gemSpawnManager;
        private GameVariablesManager gameVariables;
        
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Gem[,] AllGems { get; private set; }

        #endregion

        #region MonoBehaviour

        private void Start()
        {
            ServiceLocator.Instance.TryGet(out gemSpawnManager);
            ServiceLocator.Instance.TryGet(out gameVariables);
            
            Width = gameVariables.LevelConfig.BoardWidth;
            Height = gameVariables.LevelConfig.BoardHeight;
            
            AllGems = new Gem[Width, Height];
            
            FillBoard();
            
            OnGameBoardSetup?.Invoke();
        }
        
        #endregion

        #region Events

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
        
        private void OnGemsSwipe(Gem firstGem, Gem secondGem)
        {
            StartCoroutine(SwapGemsCoroutine(firstGem, secondGem));
        }

        #endregion

        #region Gems swapping

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
                if (firstGem?.isActiveAndEnabled == true && secondGem?.isActiveAndEnabled == true)
                {
                    SwapGems(secondGem, firstGem, false);
                }
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
            markedForDestruction.UnionWith(matchedGems);
            
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
                            gemToDestroy.MarkForDestruction(patternPositions[i].z * gameVariables.GameSettings.SimpleGemsDestructionDelay + gem.DestroyDelay);
                            markedForDestruction.Add(gemToDestroy);

                            if (gemToDestroy.GemType.IsSpecial())
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
                            SpawnGemOnBoard(gem.PosIndex, 0, "Gem Bomb", GemType.Bomb);
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
        
        private void DestroyGem(Gem gem)
        {
            if (!gem) return;
            
            Vector2Int gemPos = new Vector2Int(gem.PosIndex.x, gem.PosIndex.y);
            gem.DestroyGem();
            AllGems[gemPos.x, gemPos.y] = null;
        }

        #endregion

        #region Filling

        private void SpawnGemOnBoard(Vector2Int index, int heightOffset, string withName, GemType gemType = GemType.None)
        {
            if (gemType == GemType.None)
            {
                gemType = gameVariables.LevelConfig.GetRandomWeightedGemType();
            }
            
            Gem spawnedGem = gemSpawnManager.GetGemOfType(gemType)
                .WithPositionIndex(index);
            
            spawnedGem.name = withName;
            spawnedGem.transform.SetParent(gemsContainer);
            spawnedGem.transform.localPosition = new Vector2(spawnedGem.PosIndex.x, spawnedGem.PosIndex.y + heightOffset);
            
            AllGems[index.x, index.y] = spawnedGem;
        }
        
        // For initial board spawn
        private void FillBoard()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Gem mostLeftGem = null;
                    Gem leftGem = null;
                    Gem mostBottomGem = null;
                    Gem bottomGem = null;

                    // Generate initial random GemType
                    GemType randomisedGemType = gameVariables.LevelConfig.GetRandomWeightedGemType();

                    // Check horizontal neighbors (boundary check: x > 1)
                    if (x > 1)
                    {
                        mostLeftGem = AllGems[x - 2, y];
                        leftGem = AllGems[x - 1, y];
                    }
                    
                    // Check vertical neighbors (boundary check: y > 1)
                    if (y > 1)
                    {
                        mostBottomGem = AllGems[x, y - 2];
                        bottomGem = AllGems[x, y - 1];
                    }

                    // Check for matches
                    bool isAMatch =
                        (mostBottomGem != null && bottomGem != null &&
                            Match3Utilities.IsMatch(mostBottomGem.GemType, bottomGem.GemType, randomisedGemType)) ||
                        (mostLeftGem != null && leftGem != null &&
                            Match3Utilities.IsMatch(mostLeftGem.GemType, leftGem.GemType, randomisedGemType));

                    // If there's a match, pick a new GemType to avoid matches
                    if (isAMatch)
                    {
                        randomisedGemType = gameVariables.LevelConfig.GetRandomWeightedGemType(
                            new HashSet<GemType>() { randomisedGemType });
                    }
                    
                    SpawnGemOnBoard(new Vector2Int(x, y), 0, $"Gem [{x},{y}]", randomisedGemType);
                }
            }
        }
        
        private IEnumerator DecreaseRowCoroutine()
        {
            yield return new WaitForSeconds(gameVariables.GameSettings.FallingGemsDelay);

            for (int x = 0; x < Width; x++)
            {
                bool encounteredNull = false;
                int nullCounter = 0;

                for (int y = 0; y < Height; y++)
                {
                    Gem curGem = AllGems[x, y];

                    if (curGem is null)
                    {
                        encounteredNull = true;
                        nullCounter++;
                    }
                    else if (encounteredNull)
                    {
                        int newY = y - nullCounter;
                        if (newY >= 0)
                        {
                            curGem.PosIndex.y = newY;
                            curGem.MoveToPositionIndex();
                            AllGems[x, newY] = curGem;
                            AllGems[x, y] = null;
                        }
                    }
                }
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
        
        private void RefillBoard()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Gem _curGem = AllGems[x, y];
                    if (_curGem == null)
                    {
                        GemType gemType = gameVariables.LevelConfig.GetRandomWeightedGemType();
                        SpawnGemOnBoard(new Vector2Int(x, y), 0, $"Refilled Gem [{x},{y}]", gemType);
                    }
                }
            }

            //CheckMisplacedGems();
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
                Debug.Log($"Found misplaced gem at: {g.PosIndex} - Destroying it");
                Destroy(g.gameObject);
            }
        }

        #endregion
    }
}

