using UnityEngine;
using UnityEngine.Serialization;

namespace Match3
{
    [CreateAssetMenu(fileName = "GemsConfig", menuName = "Data/Gems Config", order = 0)]
    public class GemsConfig : ScriptableObject
    {
        // TODO: Make dictionary with display in Inspector
        [SerializeField] public GemConfig[] gemConfigs;
        
        public Sprite GetSprite(GemType gemType)
        {
            for (int i = 0; i < gemConfigs.Length; i++)
            {
                if (gemConfigs[i].gemType == gemType)
                {
                    return gemConfigs[i].sprite;
                }
            }

            Debug.LogError($"Sprite for GemType {gemType} not found.");
            return gemConfigs[0].sprite;
        }

        public DestroyPattern GetDestroyPattern(GemType gemType)
        {
            for (int i = 0; i < gemConfigs.Length; i++)
            {
                if (gemConfigs[i].gemType == gemType)
                {
                    return gemConfigs[i].destroyPattern;
                }
            }

            Debug.LogError($"Destroy pattern for GemType {gemType} not found.");
            return gemConfigs[0].destroyPattern;
        }

        public GameObject GetDestroyEffect(GemType gemType)
        {
            for (int i = 0; i < gemConfigs.Length; i++)
            {
                if (gemConfigs[i].gemType == gemType)
                {
                    return gemConfigs[i].destroyEffect;
                }
            }

            Debug.LogError($"Destroy effect for GemType {gemType} not found.");
            return gemConfigs[0].destroyEffect;
        }
    }

    [System.Serializable]
    public struct GemConfig
    {
        public GemType gemType;
        public DestroyPattern destroyPattern;
        public Sprite sprite;
        public GameObject destroyEffect;
    }
}