using UnityEngine;

namespace Match3
{
    /// <summary>
    /// Settings that are mostly static
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Data/Game Settings", order = 0)]
    public class GameConfig : ScriptableObject
    {
        public Gem DefaultGemPrefab;
        public GemsConfig DefaultGemsConfig;
        
        public int ScoreForDestroyedGem = 10;
        public int GemDropHeight = 0;
        public GameObject BoardBackgroundTilesPrefab;
        
        [Header("Animations speed and delays")]
        public float SwapDuration = 1;
        
        public float SimpleGemsDestructionDelay = 0.15f;
        public float SpecialGemsDestructionDelay = 0.1f;

        public float FallingGemsDelay = 0.25f;
        public float FallingGemsSpeed = 0.1f;
        
        public float ScoresAnimationSpeed = 0.2f;
        
        [Header("Specials spawn rules")]
        public Gem BombPrefab;
        public int BombSpawnCountRule = 4;
    }
}