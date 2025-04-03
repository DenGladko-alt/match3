using System;
using UnityEngine;
using Utility;

namespace Match3
{
    public class ScoreManager : MonoBehaviour
    {
        public static event Action<int, int> OnScoreChanged; // <old score, new score>

        private GameVariablesManager gameVariables;
        
        private int score;

        private void Start()
        {
            ServiceLocator.Instance.TryGet(out gameVariables);
        }

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
            ScoreCheck();
        }

        private void ScoreCheck()
        {
            int oldScore = score;

            score += gameVariables.GameSettings.ScoreForDestroyedGem;

            OnScoreChanged?.Invoke(oldScore, score);
        }
    }
}
