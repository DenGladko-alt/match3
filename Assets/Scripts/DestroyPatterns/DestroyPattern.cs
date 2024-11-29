using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public abstract class DestroyPattern : ScriptableObject
    {
        public abstract List<Vector2Int> GetPattern(Vector2Int position, int radius);
    }
}