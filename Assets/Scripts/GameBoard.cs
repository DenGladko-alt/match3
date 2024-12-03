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

        private Gem[,] allGems;

        public int Width => width;
        public int Height => height;
        
        public HashSet<Gem> CurrentMatches { get; private set; } = new HashSet<Gem>();

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
            
            SwapGems(firstGem, secondGem);

            yield return new WaitForSeconds(.5f);

            FindAllMatches();

            if (CurrentMatches.Count == 0)
            {
                SwapGems(secondGem, firstGem);
            }
            else
            {
                gameLogic.DestroyMatches();
            }
        }

        private void SwapGems(Gem firstGem, Gem secondGem)
        {
            Vector2Int firstGemPosition = firstGem.posIndex;
            SetGem(secondGem.posIndex.x, secondGem.posIndex.y, firstGem);
            firstGem.posIndex = secondGem.posIndex;

            SetGem(firstGemPosition.x, firstGemPosition.y, secondGem);
            secondGem.posIndex = firstGemPosition;
        }

        public void SetGem(int _X, int _Y, Gem _Gem)
        {
            allGems[_X, _Y] = _Gem;
        }

        public Gem GetGem(int _X, int _Y)
        {
            return allGems[_X, _Y];
        }
        
        public void FindAllMatches()
        {
            CurrentMatches.Clear();
            HashSet<Gem> horizontalMatches = new HashSet<Gem>();
            HashSet<Gem> verticalMatches = new HashSet<Gem>();

            int currentDestroyGroup = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gem currentGem = allGems[x, y];
                    if (currentGem == null) continue;

                    // Check horizontal and vertical matches
                    if (x > 0 && x < width - 1)
                        CheckMatchAndAssign(allGems[x - 1, y], currentGem, allGems[x + 1, y], horizontalMatches, ref currentDestroyGroup);

                    if (y > 0 && y < height - 1)
                        CheckMatchAndAssign(allGems[x, y - 1], currentGem, allGems[x, y + 1], verticalMatches, ref currentDestroyGroup);
                }
            }

            // Combine matches
            HashSet<Gem> allMatches = new HashSet<Gem>(horizontalMatches);
            allMatches.UnionWith(verticalMatches);

            // Add all remaining matches to CurrentMatches
            CurrentMatches.AddRange(allMatches);
            
            IOrderedEnumerable<IGrouping<int, Gem>> groupedByDestroyGroup = CurrentMatches
                .GroupBy(gem => gem.DestroyGroup)
                .OrderBy(group => group.Key);

            // int currentDestroyGroupsCount = groupedByDestroyGroup.Count();
            // if (currentDestroyGroupsCount > 0)
            // {
            //     Debug.Log("Destroy groups: " + currentDestroyGroupsCount);
            // }
        }
        
        private void CheckMatchAndAssign(Gem gem1, Gem gem2, Gem gem3, HashSet<Gem> matches, ref int currentDestroyGroup)
        {
            if (gem1 != null && gem2 != null && gem3 != null && gem1.GemType == gem2.GemType && gem2.GemType == gem3.GemType)
            {
                matches.Add(gem1);
                matches.Add(gem2);
                matches.Add(gem3);

                // Assign or unify groups
                if (gem1.DestroyGroup == -1 && gem2.DestroyGroup == -1 && gem3.DestroyGroup == -1)
                {
                    gem1.DestroyGroup = gem2.DestroyGroup = gem3.DestroyGroup = currentDestroyGroup;
                    currentDestroyGroup++;
                }
                else
                {
                    int targetGroup = Math.Max(Math.Max(gem1.DestroyGroup, gem2.DestroyGroup), gem3.DestroyGroup);
                    if (gem1.DestroyGroup == -1) gem1.DestroyGroup = targetGroup;
                    if (gem2.DestroyGroup == -1) gem2.DestroyGroup = targetGroup;
                    if (gem3.DestroyGroup == -1) gem3.DestroyGroup = targetGroup;
                }
            }
        }
        
        private void CheckForSpecials()
        {
            HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();
            
            foreach (Gem gem in CurrentMatches)
            {
                int x = gem.posIndex.x;
                int y = gem.posIndex.y;

                //Check neighbors
                CheckAndExecuteSpecialPattern(x, y + 1, checkedPositions); // Above
                CheckAndExecuteSpecialPattern(x + 1, y, checkedPositions); // Right
                CheckAndExecuteSpecialPattern(x, y - 1, checkedPositions); // Below
                CheckAndExecuteSpecialPattern(x - 1, y, checkedPositions); // Left
            }
        }

        private void CheckAndExecuteSpecialPattern(int x, int y, HashSet<Vector2Int> checkedPositions)
        {
            // Ignore out-of-bounds positions
            if (x < 0 || x >= width || y < 0 || y >= height) return;

            Vector2Int position = new Vector2Int(x, y);

            // Skip already-checked positions
            if (!checkedPositions.Add(position)) return;

            Gem gem = allGems[x, y];

            // Skip null gems or non-special gems
            if (gem == null || gem.GemType.IsSpecial() == false) return;

            List<Vector3Int> patternPositions = gem.DestroyPattern?.GetPattern(new Vector2Int(x, y));

            if (patternPositions == null) return;

            for (int i = 0; i < patternPositions.Count; i++)
            {
                Vector3Int pos = patternPositions[i];

                if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) continue;

                Gem gemToDestroy = allGems[pos.x, pos.y];

                if (gemToDestroy == null) continue;

                if (!CurrentMatches.Contains(gemToDestroy))
                {
                    CurrentMatches.Add(gemToDestroy);
                    gemToDestroy.DestroyOrder = patternPositions[i].z;
                }
            }
        }
    }
}

