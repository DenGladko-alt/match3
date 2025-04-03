using System.Collections.Generic;
using Match3;
using UnityEngine;
using Utility;

public class VFXManager : MonoBehaviour
{
    [SerializeField] private int initialPoolsSizePerVFX = 5;
    
    private GameVariablesManager gameVariables;
    private PoolManager poolManager;
    
    private Dictionary<GemType, GameObject> gemTypePrefabDictionary = new Dictionary<GemType, GameObject>();

    private void Start()
    {
        ServiceLocator.Instance.TryGet(out gameVariables);
        ServiceLocator.Instance.TryGet(out poolManager);

        //CreatePoolsForGemsVFX();
    }

    #region Events

    private void OnEnable()
    {
        GemDestroyVFX.OnVFXStopped += OnDestroyedGemVFXStopped;
        Gem.OnGemDestroyed += OnGemDestroyed;
    }

    private void OnDisable()
    {
        GemDestroyVFX.OnVFXStopped -= OnDestroyedGemVFXStopped;
        Gem.OnGemDestroyed -= OnGemDestroyed;
    }

    private void OnDestroyedGemVFXStopped(GemDestroyVFX vfx)
    {
        //poolManager.Despawn(vfx.gameObject, vfx.gameObject);
    }
    
    private void OnGemDestroyed(Gem gem)
    {
        // GetGemDestroyVFXForType(gem.GemType, gem.transform.position, gem.transform.rotation)
        //     .PlayVFX();
    }

    #endregion

    // Create VFX pools for each Gem destroy VFX for each level config so we don`t create redundant pools
    private void CreatePoolsForGemsVFX()
    {
        for (int i = 0; i < gameVariables.LevelConfig.GemSpawnWeights.Length; i++)
        {
            GemType gemType = gameVariables.LevelConfig.GemSpawnWeights[i].GemType;
            GameObject destroyEffect = gameVariables.GameSettings.DefaultGemsConfig.GetDestroyEffect(gemType);
            gemTypePrefabDictionary.Add(gemType, destroyEffect);
            //poolManager.CreatePool<GemDestroyVFX>(destroyEffect, initialPoolsSizePerVFX);
        }
    }

    // public GemDestroyVFX GetGemDestroyVFXForType(GemType gemType, Vector3 position, Quaternion rotation)
    // {
    //     return poolManager.Spawn<GemDestroyVFX>(gemTypePrefabDictionary[gemType], position, rotation);
    // }
}
