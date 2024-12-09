using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Utility;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Match3
{
    public class Gem : MonoBehaviour
    {
        #region Variables

            // TODO: Just for debug, Remove later
            [SerializeField] private TextMeshPro textMeshPro;
        
            // Events
            public static event Action<Gem> OnGemDestroyed;

            // Serialized fields
            [SerializeField] private GemType gemType;
            [SerializeField] private GemsConfig gemsConfig;
            [SerializeField] private SpriteRenderer spriteRenderer;
            
            [Header("Current settings")]
            public Vector2Int PosIndex;
            public int MergeGroup = -1;
            public bool MovedByPlayer = false;
            public float DestroyDelay = 0;

            private GameVariablesService gameVariables;
            
            // Properties
            public GemType GemType => gemType;
            public DestroyPattern DestroyPattern => gemsConfig.GetDestroyPattern(gemType);

        #endregion

        #region Editor

            #if UNITY_EDITOR
                    
                public void OnValidate()
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (spriteRenderer != null)
                            spriteRenderer.sprite = gemsConfig.GetSprite(gemType);
                    };
                }
                        
            #endif

        #endregion

        #region MonoBehaviour

        private void Start()
        {
            ServiceManager.Instance.TryGet(out gameVariables);
        }

        private void Update()
        {
            textMeshPro.text = PosIndex.ToString();
        }

        #endregion

        #region Logic
        
        public void SetupGem(Vector2Int position)
        {
            PosIndex = position;
        }

        public void MoveToPositionIndex()
        {
            StartCoroutine(MoveToPositionOverDuration(PosIndex, gameVariables.GameSettings.SwapDuration));
        }

        private IEnumerator MoveToPositionOverDuration(Vector2 end, float duration)
        {
            float elapsedTime = 0f;
            
            Vector3 startPosition = transform.position;

            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startPosition, end, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            transform.position = end;
        }

        public void DestroyGem(bool playEffect = true)
        {
            if (playEffect) PlayDestroyEffect();
            OnGemDestroyed?.Invoke(this);
            Destroy(gameObject);
        }

        private void PlayDestroyEffect()
        {
            Instantiate(gemsConfig.GetDestroyEffect(gemType), transform.position, Quaternion.identity);
        }

        #endregion
    
    }
}