using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace Match3
{
    public class GameBoard : MonoBehaviour
    {
        #region Variables

        [SerializeField] private int width = 7;
        [SerializeField] private int height = 7;
        [SerializeField] private GameLogic gameLogic = null;
        [SerializeField] private GemSpawnManager gemSpawnManager = null;

        public Gem[,] allGems;
        private int currentDestroyGroup;

        public int Width => width;
        public int Height => height;
        
        public HashSet<Gem> MatchedGems { get; private set; } = new HashSet<Gem>();
        public IOrderedEnumerable<IGrouping<int, Gem>> GroupedMatches { get; private set; }
        public HashSet<Gem> MarkedBySpecials { get; private set; } = new HashSet<Gem>();

        #endregion
        
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

        public void Setup()
        {
            allGems = new Gem[Width, Height];
        }

        // TODO: Make universal check in one place, MatchFinder (?)
        public bool MatchesAt(Vector2Int _PositionToCheck, Gem _GemToCheck)
        {
            if (_PositionToCheck.x > 1)
            {
                if (allGems[_PositionToCheck.x - 1, _PositionToCheck.y].GemType == _GemToCheck.GemType &&
                    allGems[_PositionToCheck.x - 2, _PositionToCheck.y].GemType == _GemToCheck.GemType)
                    return true;
            }

            if (_PositionToCheck.y > 1)
            {
                if (allGems[_PositionToCheck.x, _PositionToCheck.y - 1].GemType == _GemToCheck.GemType &&
                    allGems[_PositionToCheck.x, _PositionToCheck.y - 2].GemType == _GemToCheck.GemType)
                    return true;
            }

            return false;
        }

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

            if (FindAllMatches())
            {
                gameLogic.DestroyMatches();
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
            SetGem(secondGem.PosIndex.x, secondGem.PosIndex.y, firstGem);
            firstGem.PosIndex = secondGem.PosIndex;
            firstGem.MovedByPlayer = markAsMovedByPlayer;

            SetGem(firstGemPosition.x, firstGemPosition.y, secondGem);
            secondGem.PosIndex = firstGemPosition;
            secondGem.MovedByPlayer = markAsMovedByPlayer;
        }

        public void SetGem(int _X, int _Y, Gem _Gem)
        {
            allGems[_X, _Y] = _Gem;
        }

        public Gem GetGem(int _X, int _Y)
        {
            return allGems[_X, _Y];
        }
        
        public bool FindAllMatches()
        {
            HashSet<Gem> horizontalMatches = new HashSet<Gem>();
            HashSet<Gem> verticalMatches = new HashSet<Gem>();

            currentDestroyGroup = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gem currentGem = allGems[x, y];
                    if (currentGem == null) continue;

                    // Check horizontal and vertical matches
                    if (x > 0 && x < width - 1)
                        CheckMatchAndAssignGroups(allGems[x - 1, y], currentGem, allGems[x + 1, y], horizontalMatches);

                    if (y > 0 && y < height - 1)
                        CheckMatchAndAssignGroups(allGems[x, y - 1], currentGem, allGems[x, y + 1], verticalMatches);
                }
            }

            // Combine matches
            MatchedGems.Clear();
            MatchedGems = new HashSet<Gem>(horizontalMatches);
            MatchedGems.UnionWith(verticalMatches);
            
            GroupedMatches = MatchedGems
                .GroupBy(gem => gem.MergeGroup)
                .OrderBy(group => group.Key);

            HashSet<Gem> specialGems = new HashSet<Gem>();
            foreach (Gem gem in MatchedGems)
            {
                if (gem == null) continue;
                
                if (gem.GemType.IsSpecial()) specialGems.Add(gem);
            }
            
            MarkedBySpecials.Clear();
            ExecuteSpecialGems(specialGems);
            AddDelaysToSpecialGems(specialGems);
            
            return MatchedGems.Count > 0;
        }
        
        private void CheckSpecialGems(HashSet<Gem> specialGems)
        {
            if (specialGems.Count < 0) return;
            
            foreach (Gem gem in specialGems)
            {
                List<Vector3Int> patternPositions = gem.DestroyPattern?.GetPattern(gem.PosIndex);
                
                if (patternPositions == null) return;
                
                for (int i = 0; i < patternPositions.Count; i++)
                {
                    Vector3Int pos = patternPositions[i];

                    if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) continue;

                    Gem gemToDestroy = allGems[pos.x, pos.y];

                    if (gemToDestroy == null) continue;

                    if (MarkedBySpecials.Add(gemToDestroy))
                    {
                        gemToDestroy.DestroyOrder = patternPositions[i].z;
                    }
                }
            }
        }

        private void ExecuteSpecialGems(HashSet<Gem> specialGems)
        {
            if (specialGems.Count == 0) return;
            
            foreach (Gem gem in specialGems.ToList())
            {
                List<Vector3Int> patternPositions = gem.DestroyPattern?.GetPattern(gem.PosIndex);
                int destroyPatternRadius = gem.DestroyPattern?.Radius ?? 0;
                
                if (patternPositions == null) return;

                int maxDestroyRadius = 0;
                
                for (int i = 0; i < patternPositions.Count; i++)
                {
                    Vector3Int pos = patternPositions[i];

                    if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) continue;

                    Gem gemToDestroy = allGems[pos.x, pos.y];

                    if (gemToDestroy == null) continue;

                    if (MatchedGems.Contains(gemToDestroy)) continue;

                    if (gemToDestroy.DestroyOrder > 0)
                    {
                        gemToDestroy.DestroyOrder = Math.Min(gemToDestroy.DestroyOrder, patternPositions[i].z + gem.DestroyOrder);
                    }
                    else
                    {
                        gemToDestroy.DestroyOrder = patternPositions[i].z + gem.DestroyOrder;
                    }
                 
                    // Destroy special last
                    //gem.DestroyOrder += destroyPatternRadius;
                    
                    if (MarkedBySpecials.Add(gemToDestroy))
                    {
                        maxDestroyRadius = Math.Max(maxDestroyRadius, gemToDestroy.DestroyOrder);
                        if (gemToDestroy.GemType.IsSpecial())
                        {
                            specialGems.Add(gemToDestroy);
                            ExecuteSpecialGems(specialGems);
                        }
                    }
                }
            }
        }

        private void AddDelaysToSpecialGems(HashSet<Gem> specialGems)
        {
            foreach (Gem gem in specialGems)
            {
                gem.DestroyOrder += gem.DestroyPattern.Radius + 1;
            }
        }
        
        private void CheckMatchAndAssignGroups(Gem gem1, Gem gem2, Gem gem3, HashSet<Gem> matches)
        {
            if (gem1 != null && gem2 != null && gem3 != null && gem1.GemType == gem2.GemType && gem2.GemType == gem3.GemType)
            {
                matches.Add(gem1);
                matches.Add(gem2);
                matches.Add(gem3);

                // Assign or unify groups
                if (gem1.MergeGroup == -1 && gem2.MergeGroup == -1 && gem3.MergeGroup == -1)
                {
                    gem1.MergeGroup = gem2.MergeGroup = gem3.MergeGroup = currentDestroyGroup;
                    currentDestroyGroup++;
                }
                else
                {
                    int targetGroup = Math.Max(Math.Max(gem1.MergeGroup, gem2.MergeGroup), gem3.MergeGroup);
                    if (gem1.MergeGroup == -1) gem1.MergeGroup = targetGroup;
                    if (gem2.MergeGroup == -1) gem2.MergeGroup = targetGroup;
                    if (gem3.MergeGroup == -1) gem3.MergeGroup = targetGroup;
                }
            }
        }
        
        // private void CheckForSpecials()
        // {
        //     HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();
        //     
        //     foreach (Gem gem in CurrentMatches)
        //     {
        //         int x = gem.PosIndex.x;
        //         int y = gem.PosIndex.y;
        //
        //         //Check neighbors
        //         CheckAndExecuteSpecialPattern(x, y + 1, checkedPositions); // Above
        //         CheckAndExecuteSpecialPattern(x + 1, y, checkedPositions); // Right
        //         CheckAndExecuteSpecialPattern(x, y - 1, checkedPositions); // Below
        //         CheckAndExecuteSpecialPattern(x - 1, y, checkedPositions); // Left
        //     }
        // }
        //
        // private void CheckAndExecuteSpecialPattern(int x, int y, HashSet<Vector2Int> checkedPositions)
        // {
        //     // Ignore out-of-bounds positions
        //     if (x < 0 || x >= width || y < 0 || y >= height) return;
        //
        //     Vector2Int position = new Vector2Int(x, y);
        //
        //     // Skip already-checked positions
        //     if (!checkedPositions.Add(position)) return;
        //
        //     Gem gem = allGems[x, y];
        //
        //     // Skip null gems or non-special gems
        //     if (gem == null || gem.GemType.IsSpecial() == false) return;
        //
        //     List<Vector3Int> patternPositions = gem.DestroyPattern?.GetPattern(new Vector2Int(x, y));
        //
        //     if (patternPositions == null) return;
        //
        //     for (int i = 0; i < patternPositions.Count; i++)
        //     {
        //         Vector3Int pos = patternPositions[i];
        //
        //         if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) continue;
        //
        //         Gem gemToDestroy = allGems[pos.x, pos.y];
        //
        //         if (gemToDestroy == null) continue;
        //
        //         if (CurrentMatches.Add(gemToDestroy))
        //         {
        //             gemToDestroy.DestroyOrder = patternPositions[i].z;
        //         }
        //     }
        // }
    }
}

