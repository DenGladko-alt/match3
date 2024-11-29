using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match3
{
    [CreateAssetMenu(fileName = "DiamondDestroyPattern", menuName = "Destroy Patterns/Diamond Destroy Pattern")]
    public class DiamondDestroyPattern : DestroyPattern
    {
        public override List<Vector2Int> GetPattern(Vector2Int position)
        {
            List<Vector2Int> positions = new List<Vector2Int>();

            for (int x = -Radius; x <= Radius; x++)
            {
                for (int y = -Radius; y <= Radius; y++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) <= Radius)
                    {
                        positions.Add(position + new Vector2Int(x, y));
                    }
                }
            }

            return positions.Distinct().ToList();
        }
    }
}