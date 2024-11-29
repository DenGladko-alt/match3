using System.Collections;
using System.Collections.Generic;
using Match3;
using UnityEngine;

public class SingleDestroyPattern : IDestroyPattern
{
    public List<Vector2Int> GetPattern(Vector2Int position, int radius)
    {
        return new List<Vector2Int> { position };
    }
}
