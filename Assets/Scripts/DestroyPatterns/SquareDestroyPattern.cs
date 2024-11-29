using System.Collections;
using System.Collections.Generic;
using Match3;
using UnityEngine;

namespace Match3
{
    public class SquareDestroyPattern : IDestroyPattern
    {
        public List<Vector2Int> GetPattern(Vector2Int position, int radius)
        {
            List<Vector2Int> positions = new List<Vector2Int>();

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    positions.Add(position + new Vector2Int(x, y));
                }
            }

            return positions;
        }
    }
}
