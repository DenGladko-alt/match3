using UnityEngine;
using Utility;

namespace Match3
{
    public class GameLogic : MonoBehaviourService<GameLogic>
    {
        
        #region Variables

        [SerializeField, Range(0, 1f)] private float timeSpeed = 1f;
        
        public GameState CurrentState { get; private set; } = GameState.Move;

        #endregion
        
        #region MonoBehaviour
        
        private void Update()
        {
            Time.timeScale = timeSpeed;
        }

        #endregion

        #region Logic

        public void SetState(GameState _CurrentState)
        {
            CurrentState = _CurrentState;
        }

        #endregion
    }
}