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
                
                if (gem.GemType.IsSpecial()) specialGems.Add(gem);
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

        public static bool IsMatch(GemType gemType1, GemType gemType2, GemType GemType3)
        {
            return gemType1 == gemType2 && gemType2 == GemType3;
        }
        
        public static bool MatchesAt(Vector2Int positionToCheck, Gem[,] gems, GemType gemTypeToCheck)
        {
            // Helper function to safely get a gem at a given position
            Gem GetGem(Vector2Int position)
            {
                if (IsWithinBounds(position, gems))
                {
                    return gems[position.x, position.y];
                }
                return null;
            }

            // Retrieve adjacent gems
            Gem mostLeftGem = GetGem(new Vector2Int(positionToCheck.x - 2, positionToCheck.y));
            Gem leftGem = GetGem(new Vector2Int(positionToCheck.x - 1, positionToCheck.y));
            Gem mostBottomGem = GetGem(new Vector2Int(positionToCheck.x, positionToCheck.y - 2));
            Gem bottomGem = GetGem(new Vector2Int(positionToCheck.x, positionToCheck.y - 1));

            // Check horizontal match
            if (mostLeftGem != null && leftGem != null &&
                mostLeftGem.GemType == gemTypeToCheck &&
                leftGem.GemType == gemTypeToCheck)
            {
                return true;
            }

            // Check vertical match
            if (mostBottomGem != null && bottomGem != null &&
                mostBottomGem.GemType == gemTypeToCheck &&
                bottomGem.GemType == gemTypeToCheck)
            {
                return true;
            }

            return false;
        }

        private static bool IsWithinBounds(Vector2Int position, Gem[,] gems)
        {
            return position.x >= 0 && position.x < gems.GetLength(0) &&
                position.y >= 0 && position.y < gems.GetLength(1);
        }
    }
}
