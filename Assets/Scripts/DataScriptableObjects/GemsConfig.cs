using UnityEngine;
using UnityEngine.Serialization;

namespace Match3
{
    [CreateAssetMenu(fileName = "GemsConfig", menuName = "Data/Gems Config", order = 0)]
    public class GemsConfig : ScriptableObject
    {
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

            Debug.LogWarning($"Sprite for GemType {gemType} not found.");
            return null;
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

            Debug.LogWarning($"Destroy pattern for GemType {gemType} not found.");
            return null;
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

            Debug.LogWarning($"Destroy effect for GemType {gemType} not found.");
            return null;
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