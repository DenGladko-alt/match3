using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    [CreateAssetMenu(fileName = "SingleDestroyPattern", menuName = "Destroy Patterns/Single Destroy Pattern")]
    public class SingleDestroyPattern : DestroyPattern
    {
        public override List<Vector3Int> GetPattern(Vector2Int position)
        {
            return new List<Vector3Int> { new Vector3Int(position.x, position.y, 0) };
        }
    }
}
