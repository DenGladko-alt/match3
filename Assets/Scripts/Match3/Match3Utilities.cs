using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match3
{
    public static class Match3Utilities
    {
        public static HashSet<Gem> GetSpecialGemsFromHashSet(HashSet<Gem> gems)
        {
            HashSet<Gem> specialGems = new HashSet<Gem>();
            foreach (Gem gem in gems)
            {
                if (!gem) continue;
                
                if (gem.IsSpecial()) specialGems.Add(gem);
            }

            return specialGems;
        }

        public static HashSet<Gem> GetMatchingGemsFromBoard(GameBoard board)
        {
            // We can use this later for finding intersections
            HashSet<Gem> horizontalMatches = new HashSet<Gem>();
            HashSet<Gem> verticalMatches = new HashSet<Gem>();

            int currentDestroyGroup = 0;

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Gem currentGem = board.AllGems[x, y];
                    if (currentGem == null) continue;

                    // Check horizontal matches
                    if (x > 0 && x < board.Width - 1)
                        CheckMatchAndAssignGroups(board.AllGems[x - 1, y], currentGem, board.AllGems[x + 1, y], horizontalMatches, ref currentDestroyGroup);

                    //  ... and vertical
                    if (y > 0 && y < board.Height - 1)
                        CheckMatchAndAssignGroups(board.AllGems[x, y - 1], currentGem, board.AllGems[x, y + 1], verticalMatches, ref currentDestroyGroup);
                }
            }

            // Combine matches
            HashSet<Gem> matchedGems = new HashSet<Gem>(horizontalMatches);
            matchedGems.UnionWith(verticalMatches);
            
            return matchedGems;
        }
        
        private static void CheckMatchAndAssignGroups(Gem gem1, Gem gem2, Gem gem3, HashSet<Gem> matches, ref int currentDestroyGroup)
        {
            if (gem1 == null || gem2 == null || gem3 == null || gem1.GemType != gem2.GemType || gem2.GemType != gem3.GemType) return;
            
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
        
        public static Dictionary<int, HashSet<Gem>> GetGroupedMatchingGems(HashSet<Gem> gems)
        {
            return gems.GroupBy(gem => gem.MergeGroup)
                .ToDictionary(group => group.Key, group => group.ToHashSet());
        }
        
        public static bool MatchesAt(Vector2Int positionToCheck, Gem gemToCheck, Gem[,] gems)
        {
            if (positionToCheck.x > 1)
            {
                if (gems[positionToCheck.x - 1, positionToCheck.y].GemType == gemToCheck.GemType &&
                    gems[positionToCheck.x - 2, positionToCheck.y].GemType == gemToCheck.GemType)
                    return true;
            }

            if (positionToCheck.y > 1)
            {
                if (gems[positionToCheck.x, positionToCheck.y - 1].GemType == gemToCheck.GemType &&
                    gems[positionToCheck.x, positionToCheck.y - 2].GemType == gemToCheck.GemType)
                    return true;
            }

            return false;
        }

        public static HashSet<Gem> GetGemsFromPositionsOnBoard(List<Vector2Int> positions, GameBoard board)
        {
            HashSet<Gem> gems = new HashSet<Gem>();
            foreach (Vector2Int pos in positions)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    for (int y = 0; y < board.Height; y++)
                    {
                        if (pos.x < 0 || pos.x >= board.Width || pos.y < 0 || pos.y >= board.Height) continue;
                        
                        Gem currentGem = board.AllGems[pos.x, pos.y];
                        if (currentGem != null)
                        {
                            gems.Add(currentGem);
                        }
                    }
                }
            }

            return gems;
        }
    }
}
