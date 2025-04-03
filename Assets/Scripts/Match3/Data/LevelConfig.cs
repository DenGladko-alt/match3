using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace Match3
{
    /// <summary>
    /// Settings that are most likely to be changed for each level
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Data/Level Settings", order = 0)]
    public class LevelConfig : ScriptableObject
    {
        // Gems that will spawn during game
        public GemSpawnWeight[] GemSpawnWeights;
        
        public int BoardWidth = 7;
        public int BoardHeight = 7;
        
        public GemType GetRandomWeightedGemType(HashSet<GemType> excludedGemTypes = null)
        {
            Random random = new Random();
            int totalWeight = 0;

            // Calculate total weight excluding the excluded gem types
            foreach (GemSpawnWeight item in GemSpawnWeights)
            {
                if (excludedGemTypes == null || !excludedGemTypes.Contains(item.GemType))
                {
                    totalWeight += item.Weight;
                }
            }

            if (totalWeight == 0)
            {
                throw new Exception("No valid GemType found.");
            }

            int randomNumber = random.Next(0, totalWeight);
            int cumulativeWeight = 0;

            // Find a gem type based on the random number and weights
            foreach (GemSpawnWeight item in GemSpawnWeights)
            {
                if (excludedGemTypes == null || !excludedGemTypes.Contains(item.GemType))
                {
                    cumulativeWeight += item.Weight;
                    if (randomNumber < cumulativeWeight)
                    {
                        return item.GemType;
                    }
                }
            }

            throw new Exception("GemType not found");
        }
    }

    [Serializable]
    public class GemSpawnWeight
    {
        public GemType GemType;
        public int Weight;
    }
}