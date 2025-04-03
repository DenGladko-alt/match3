using UnityEngine;
using Utility;

namespace Match3
{
    public class GameInstaller : MonoBehaviourSingleton<GameInstaller>
    {
        [SerializeField] private GameLogic gameLogic;
        [SerializeField] private GemSpawnManager gemSpawnManager;
        [SerializeField] private GameVariablesManager gameVariablesManager;
        [SerializeField] private PoolManager poolManager;
        [SerializeField] private VFXManager vfxManager;

        protected override void Awake()
        {
            base.Awake();

            ServiceLocator.Instance.Register(gameLogic);
            ServiceLocator.Instance.Register(gemSpawnManager);
            ServiceLocator.Instance.Register(gameVariablesManager);
            ServiceLocator.Instance.Register(poolManager);
            ServiceLocator.Instance.Register(vfxManager);
        }
    }
}