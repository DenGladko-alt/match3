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
    public class Gem : MonoBehaviour, IPoolable
    {
        #region Variables

        // Events
        public static event Action<Gem> OnGemDestroyed;
        public static event Action<Gem> OnGemMoving; 
        public static event Action<Gem> OnGemStopped; 
        
        // Debug
        // TODO: Just for debug, Remove later
        [SerializeField] private TextMeshPro textMeshPro;
        
        [SerializeField] private GemsConfig gemsConfig;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [Header("Current settings")]
        public GemType GemType;
        public Vector2Int PosIndex = Vector2Int.zero;
        
        public int MergeGroup = -1;
        public bool MovedByPlayer = false;
        public float DestroyDelay = 0;

        private Collider2D boxCollider;
        
        private GameVariablesManager gameVariables;
        private PoolManager poolManager;
        
        private Coroutine moveCoroutine = null;
        
        // Properties
        public DestroyPattern DestroyPattern { get; private set; }
        public bool IsMarkedForDestruction { get; private set; } = false;
        public PoolType GetPoolType { get; set; } = PoolType.GemBase;

        #endregion

        #region Editor

            #if UNITY_EDITOR
                    
                public void OnValidate()
                {
                    EditorApplication.delayCall += () =>
                    {
                        if (spriteRenderer != null)
                            Build();
                    };
                }
                        
            #endif

        #endregion

        #region MonoBehaviour

        private void Start()
        {
            ServiceLocator.Instance.TryGet(out gameVariables);
            ServiceLocator.Instance.TryGet(out poolManager);
            
            boxCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            textMeshPro.text = PosIndex.ToString();
            name = PosIndex.ToString();
        }

        #endregion

        #region Events
        
        public void OnSpawn() {}

        public void OnDespawn() { ResetGem(); }

        #endregion

        #region Builders

        public Gem WithGemType(GemType type)
        {
            GemType = type;
            return this;
        }
        
        public Gem WithPositionIndex(Vector2Int position)
        {
            PosIndex = position;
            return this;
        }

        public Gem Build()
        {
            spriteRenderer.sprite = gemsConfig.GetSprite(GemType);
            DestroyPattern = gemsConfig.GetDestroyPattern(GemType);            
            
            return this;
        }

        #endregion

        #region Logic

        public void ResetGem()
        {
            PosIndex = new Vector2Int(100, 100);
            MergeGroup = -1;
            MovedByPlayer = false;
            DestroyDelay = 0;
            IsMarkedForDestruction = false;
            boxCollider.enabled = true;
        }

        public void MoveToPositionIndex()
        {
            moveCoroutine = StartCoroutine(MoveToPositionOverDuration(PosIndex, gameVariables.GameSettings.SwapDuration));
        }

        private IEnumerator MoveToPositionOverDuration(Vector2 end, float duration)
        {
            OnGemMoving?.Invoke(this);
         
            // Disable collider for selection
            boxCollider.enabled = false;
            
            float elapsedTime = 0f;
            
            Vector3 startPosition = transform.position;

            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startPosition, end, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            transform.position = end;
            
            // Enable collider for selection
            if (IsMarkedForDestruction == false)
            {
                boxCollider.enabled = true;
            }
            
            OnGemStopped?.Invoke(this);
        }

        public void MarkForDestruction(float delay)
        {
            boxCollider.enabled = false;
            DestroyDelay = delay;
            IsMarkedForDestruction = true;
        }

        public void DestroyGem()
        {
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            OnGemDestroyed?.Invoke(this);
            
            poolManager.Despawn(this);
        }

        #endregion
    }
}