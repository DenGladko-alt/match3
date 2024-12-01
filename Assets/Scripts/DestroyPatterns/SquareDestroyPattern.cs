using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match3
{
    [CreateAssetMenu(fileName = "SquareDestroyPattern", menuName = "Destroy Patterns/Square Destroy Pattern")]
    public class SquareDestroyPattern : DestroyPattern
    {
        public override List<Vector3Int> GetPattern(Vector2Int position)
        {
            List<Vector3Int> positions = new List<Vector3Int>();

            for (int x = -Radius; x <= Radius; x++)
            {
                for (int y = -Radius; y <= Radius; y++)
                {
                    positions.Add(new Vector3Int(position.x + x, position.y + y, 0));
                }
            }

            return positions.Distinct().ToList();
        }
    }
}
