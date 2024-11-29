using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match3
{
    [CreateAssetMenu(fileName = "SquareDestroyPattern", menuName = "Destroy Patterns/Square Destroy Pattern")]
    public class SquareDestroyPattern : DestroyPattern
    {
        public override List<Vector2Int> GetPattern(Vector2Int position)
        {
            List<Vector2Int> positions = new List<Vector2Int>();

            for (int x = -Radius; x <= Radius; x++)
            {
                for (int y = -Radius; y <= Radius; y++)
                {
                    positions.Add(position + new Vector2Int(x, y));
                }
            }

            return positions.Distinct().ToList();
        }
    }
}
