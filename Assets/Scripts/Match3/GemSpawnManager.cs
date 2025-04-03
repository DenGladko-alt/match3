using UnityEngine;
using Utility;

namespace Match3
{
    public class GemSpawnManager : MonoBehaviour
    {
        [SerializeField] private Transform gemsHolder;
        
        private GameVariablesManager gameVariables;
        private PoolManager poolManager;

        private void Start()
        {
            ServiceLocator.Instance.TryGet(out gameVariables);
            ServiceLocator.Instance.TryGet(out poolManager);
            
            poolManager.CreatePool<Gem>(gameVariables.GameSettings.DefaultGemPrefab, 
                gameVariables.GameSettings.DefaultGemPrefab.gameObject, 
                 gameVariables.LevelConfig.BoardWidth * gameVariables.LevelConfig.BoardHeight);
        }

        public Gem GetGemOfType(GemType gemType)
        {
            Gem gemToSpawn = poolManager.Spawn<Gem>(gameVariables.GameSettings.DefaultGemPrefab,
                    new Vector3(0,0,0), Quaternion.identity)
                .WithGemType(gemType)
                .Build();
            
            return gemToSpawn;
        }
    }
}
