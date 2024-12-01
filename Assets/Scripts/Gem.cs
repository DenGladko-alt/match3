using System;
using Match3;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public static event Action<Gem> OnGemDestroyed;
    
    [SerializeField] private DestroyPattern destroyPattern;
    [SerializeField] private GameObject destroyEffect;
    [SerializeField] private GemType gemType;
    
    [HideInInspector] public Vector2Int posIndex;
    
    [HideInInspector] public int DestroyOrder = 0;

    public int scoreValue = 10;
    
    private GameLogic _gameLogic;
    
    public DestroyPattern DestroyPattern { get => destroyPattern; private set => destroyPattern = value; }
    public GemType GemType { get => gemType; private set => gemType = value; }

    void Update()
    {
        if (Vector2.Distance(transform.position, posIndex) > 0.01f)
            transform.position = Vector2.Lerp(transform.position, posIndex, GameVariables.Instance.gemSpeed * Time.deltaTime);
        else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0);
            _gameLogic.SetGem(posIndex.x, posIndex.y, this);
        }
    }

    public void DestroyGem(bool playEffect = true)
    {
        if (playEffect) PlayDestroyEffect();
        OnGemDestroyed?.Invoke(this);
        Destroy(this.gameObject);
    }

    public void PlayDestroyEffect()
    {
        Instantiate(destroyEffect, transform.position, Quaternion.identity);
    }

    public void SetupGem(GameLogic gameLogic, Vector2Int position)
    {
        posIndex = position;
        _gameLogic = gameLogic;
    }
}