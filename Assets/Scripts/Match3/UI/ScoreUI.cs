using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Match3
{
    public class ScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private float updateDuration = 1f;
        
        private int currentScore;
        private Coroutine scoreRoutine;

        private void Start()
        {
            scoreText.text = currentScore.ToString();
        }

        private void OnEnable()
        {
            ScoreManager.OnScoreChanged += OnOnScoreChanged;
        }

        private void OnDisable()
        {
            ScoreManager.OnScoreChanged -= OnOnScoreChanged;
        }

        private void OnOnScoreChanged(int oldScore, int newScore)
        {
            if (scoreRoutine != null) StopCoroutine(scoreRoutine);
            
            scoreRoutine = StartCoroutine(AnimateScore(currentScore, newScore));
        }
        
        private IEnumerator AnimateScore(int startScore, int endScore)
        {
            float elapsedTime = 0f;
        
            while (elapsedTime < updateDuration)
            {
                float t = elapsedTime / updateDuration;
                
                int currentDisplayedScore = Mathf.RoundToInt(Mathf.Lerp(startScore, endScore, t));
                currentScore = currentDisplayedScore;
                scoreText.text = currentDisplayedScore.ToString();

                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            currentScore = endScore;
            scoreText.text = currentScore.ToString();
        }
    }
}
