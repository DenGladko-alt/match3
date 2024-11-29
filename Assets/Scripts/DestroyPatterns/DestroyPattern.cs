using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public abstract class DestroyPattern : ScriptableObject
    {
        public int Radius = 1;
        public abstract List<Vector2Int> GetPattern(Vector2Int position);
    }
}