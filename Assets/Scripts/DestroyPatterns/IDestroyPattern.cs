using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public interface IDestroyPattern
    {
        List<Vector2Int> GetPattern(Vector2Int position, int radius);
    }
}