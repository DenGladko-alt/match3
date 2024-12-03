using System;
using Match3;
using TMPro;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public static event Action<Gem> OnGemDestroyed;
    
    [SerializeField] private GemType gemType;
    
    [SerializeField] private GemsConfig gemsConfig;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // TODO: Just for debug, Remove later
    [SerializeField] private TextMeshPro textMeshPro;
    
    [HideInInspector] public Vector2Int posIndex;
    [HideInInspector] public int DestroyOrder = 0;
    [HideInInspector] public int DestroyGroup = -1;

    public int scoreValue = 10;
    
    private GameLogic _gameLogic;

    public GemType GemType { get => gemType; private set => gemType = value; }
    public DestroyPattern DestroyPattern => gemsConfig.GetDestroyPattern(gemType);

    void Update()
    {
        if (Vector2.Distance(transform.position, posIndex) > 0.01f)
            transform.position = Vector2.Lerp(transform.position, posIndex, GameVariables.Instance.gemSpeed * Time.deltaTime);
        else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0);
            _gameLogic.SetGem(posIndex.x, posIndex.y, this);
        }
        
        textMeshPro.text = posIndex.ToString();
    }

    public void DestroyGem(bool playEffect = true)
    {
        if (playEffect) PlayDestroyEffect();
        OnGemDestroyed?.Invoke(this);
        Destroy(this.gameObject);
    }

    private void PlayDestroyEffect()
    {
        Instantiate(gemsConfig.GetDestroyEffect(gemType), transform.position, Quaternion.identity);
    }

    public void SetupGem(GameLogic gameLogic, Vector2Int position)
    {
        posIndex = position;
        _gameLogic = gameLogic;
    }
}