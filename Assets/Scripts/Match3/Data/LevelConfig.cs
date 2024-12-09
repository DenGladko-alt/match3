using UnityEngine;

namespace Match3
{
    /// <summary>
    /// Settings that are most likely to be changed for each level
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Data/Level Settings", order = 0)]
    public class LevelConfig : ScriptableObject
    {
        // Gems that will spawn during game
        public Gem[] Gems;
        
        public float BombSpawnChance = 2f;
        
        public int BoardWidth = 7;
        public int BoardHeight = 7;
    }
}