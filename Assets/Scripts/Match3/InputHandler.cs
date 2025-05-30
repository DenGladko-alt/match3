using System;
using UnityEngine;
using Utility;

namespace Match3
{
    public class InputHandler : MonoBehaviour
    {
        // Events
        public static event Action<Gem> OnGemSelected;
        public static event Action<Gem, Gem> OnGemsSwipe;
        public static event Action<Gem> OnGemDeselected;
        
        // Variables
        private Gem firstGem;
        private GameLogic gameLogic;

        private void Start()
        {
            ServiceLocator.Instance.TryGet(out gameLogic);
        }

        private void Update()
        {
            //if (gameLogic.CurrentState != GameState.WaitingInput) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseDown();
            }

            if (Input.GetMouseButton(0) && firstGem != null)
            {
                HandleMouseDrag();
            }
        }

        // First click/touch
        private void HandleMouseDown()
        {
            Gem hitGem = GetGemAtMousePosition();

            if (hitGem != null)
            {
                firstGem = hitGem;
                OnGemSelected?.Invoke(firstGem);
            }
        }

        // Dragging/Swiping
        private void HandleMouseDrag()
        {
            Gem secondGem = GetGemAtMousePosition();

            if (secondGem != null && secondGem != firstGem)
            {
                if (Vector2Utilities.CellsAreNeighbors(firstGem.PosIndex, secondGem.PosIndex))
                {
                    OnGemsSwipe?.Invoke(firstGem, secondGem);
                }

                OnGemDeselected?.Invoke(firstGem);
                firstGem = null;
            }
        }

        private Gem GetGemAtMousePosition()
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);

            return hit.collider != null ? hit.collider.GetComponent<Gem>() : null;
        }
    }
}