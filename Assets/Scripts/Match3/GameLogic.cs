using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class GameLogic : MonoBehaviour
    {
        #region Variables

        [SerializeField, Range(0, 1f)] private float timeSpeed = 1f;
        
        public List<Gem> gems = new List<Gem>();
        
        private int currentlyMovingGemsCount = 0;
        
        public GameState CurrentState { get; private set; } = GameState.WaitingInput;
        
        #endregion
        
        #region MonoBehaviour
        
        private void Update()
        {
            Time.timeScale = timeSpeed;
        }

        #endregion

        #region Events

        private void OnEnable()
        {
            Gem.OnGemMoving += OnGemMoving;
            Gem.OnGemStopped += OnGemStopped;
        }
        private void OnDisable()
        {
            Gem.OnGemMoving -= OnGemMoving;
            Gem.OnGemStopped -= OnGemStopped;
        }
        
        private void OnGemMoving(Gem gem)
        {
            currentlyMovingGemsCount++;
            UpdateGameState();
        }

        private void OnGemStopped(Gem gem)
        {
            currentlyMovingGemsCount--;
            UpdateGameState();
        }

        #endregion

        #region Logic
        
        private void UpdateGameState()
        {
            if (currentlyMovingGemsCount > 0)
            {
                CurrentState = GameState.MovingGems;
            }
            else
            {
                CurrentState = GameState.WaitingInput;
            }
        }

        #endregion
    }
}