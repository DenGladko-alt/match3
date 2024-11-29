using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public class DiamondDestroyPattern : IDestroyPattern
    {
        public List<Vector2Int> GetPattern(Vector2Int position, int radius)
        {
            List<Vector2Int> positions = new List<Vector2Int>();

            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= radius)
                    {
                        positions.Add(position + new Vector2Int(x, y));
                    }
                }
            }

            return positions;
        }
    }
}