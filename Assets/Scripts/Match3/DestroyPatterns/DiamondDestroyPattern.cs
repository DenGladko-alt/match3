using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match3
{
    [CreateAssetMenu(fileName = "DiamondDestroyPattern", menuName = "Destroy Patterns/Diamond Destroy Pattern")]
    public class DiamondDestroyPattern : DestroyPattern
    {
        public override List<Vector3Int> GetPattern(Vector2Int position)
        {
            List<Vector3Int> positions = new List<Vector3Int>();
            
            for (int x = -Radius; x <= Radius; x++)
            {
                for (int y = -Radius; y <= Radius; y++)
                {
                    int distance = Math.Abs(x) + Math.Abs(y);
                    if (distance <= Radius)
                    {
                        positions.Add(new Vector3Int(position.x + x, position.y + y, distance));
                    }
                }
            }

            return positions;
        }
    }
}