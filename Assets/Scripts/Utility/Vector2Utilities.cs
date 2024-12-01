using UnityEngine;

namespace Utility
{
    public static class Vector2Utilities
    {
        public static bool CellsAreNeighbors(Vector2Int firstPos, Vector2Int secondPos)
        {
            int deltaX = Mathf.Abs(firstPos.x - secondPos.x);
            int deltaY = Mathf.Abs(firstPos.y - secondPos.y);
            
            return (deltaX == 1 && deltaY == 0) || (deltaX == 0 && deltaY == 1);
        }
    }
}
