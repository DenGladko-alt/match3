using UnityEngine;

public class GameVariables : MonoBehaviour
{
    public GameObject bgTilePrefabs;
    public Gem bomb;
    public Gem[] gems;
    public float bonusAmount = 0.5f;
    public float bombChance = 2f;
    public int dropHeight = 0;
    public float gemSpeed;
    public float scoreSpeed = 5;
    
    [HideInInspector] public int rowsSize = 7;
    [HideInInspector] public int colsSize = 7;

    #region Singleton

    static GameVariables instance;
    public static GameVariables Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameVariables>();

            return instance;
        }
    }

    #endregion
}
