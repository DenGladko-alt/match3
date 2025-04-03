using System;
using UnityEngine;

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
            
            throw new ArgumentException($"Sprite for GemType {gemType} not found.");
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

            throw new ArgumentException($"Destroy pattern for GemType {gemType} not found.");
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

            throw new ArgumentException($"Destroy effect for GemType {gemType} not found.");
        }
    }

    [Serializable]
    public struct GemConfig
    {
        public GemType gemType;
        public Sprite sprite;
        public DestroyPattern destroyPattern;
        public GameObject destroyEffect;
    }
}