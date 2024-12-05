using System;
using Match3;
using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Gem : MonoBehaviour
{
    public static event Action<Gem> OnGemDestroyed;
    
    [SerializeField] private GemType gemType;
    
    [SerializeField] private GemsConfig gemsConfig;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // TODO: Just for debug, Remove later
    [SerializeField] private TextMeshPro textMeshPro;
    
    //[HideInInspector] 
    public Vector2Int PosIndex;
    //[HideInInspector] 
    public int DestroyOrder = 0;
    [HideInInspector] public int MergeGroup = -1;
    [HideInInspector] public bool MovedByPlayer = false;

    public int scoreValue = 10;

    public GemType GemType => gemType;
    public DestroyPattern DestroyPattern => gemsConfig.GetDestroyPattern(gemType);

    #region Editor

#if UNITY_EDITOR
    public void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            if (spriteRenderer!=null)
                spriteRenderer.sprite = gemsConfig.GetSprite(gemType);
        };
    }
#endif

    #endregion

    void Update()
    {
        if (Vector2.Distance(transform.position, PosIndex) > 0.01f)
            transform.position = Vector2.Lerp(transform.position, PosIndex, GameVariables.Instance.gemSpeed * Time.deltaTime);
        else
        {
            transform.position = new Vector3(PosIndex.x, PosIndex.y, 0);
        }
        
        textMeshPro.text = PosIndex.ToString();
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

    public void SetupGem(Vector2Int position)
    {
        PosIndex = position;
    }
}