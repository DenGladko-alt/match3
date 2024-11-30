using System;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static event Action<Gem> OnGemSelected;
    public static event Action<Gem, Gem> OnGemsSwipe;

    private Gem firstGem = null;

    private void Update()
    {
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
            OnGemsSwipe?.Invoke(firstGem, secondGem);
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