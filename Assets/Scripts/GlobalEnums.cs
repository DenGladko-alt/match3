using UnityEngine;

public class GlobalEnums : MonoBehaviour
{
    public enum GemType
    {
        Blue,
        Green,
        Red,
        Yellow,
        Purple,
        Special,
    };

    public enum GameState
    {
        Wait, 
        Move
    }
}
