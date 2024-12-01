using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static event Action<int, int> OnScoreChanged; // <old score, new score>
    
    public int Score { get; private set; } = 0;

    private void OnEnable()
    {
        Gem.OnGemDestroyed += OnGemDestroyed;
    }

    private void OnDisable()
    {
        Gem.OnGemDestroyed -= OnGemDestroyed;
    }

    private void OnGemDestroyed(Gem gem)
    {
        ScoreCheck(gem);
    }
    
    private void ScoreCheck(Gem gemToCheck)
    {
        int oldScore = Score;
        
        Score += gemToCheck.scoreValue;

        OnScoreChanged?.Invoke(oldScore, Score);
    }
}
