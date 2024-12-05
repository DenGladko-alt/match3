using UnityEngine;
using Random = UnityEngine.Random;

namespace Match3
{
    public class GemSpawnManager : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private Transform gemsHolder;

        public void Setup(GameLogic gameLogic)
        {
            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    int _gemToUse = Random.Range(0, GameVariables.Instance.gems.Length);

                    int iterations = 0;
                    while (gameBoard.MatchesAt(new Vector2Int(x, y), GameVariables.Instance.gems[_gemToUse]) &&
                           iterations < 100)
                    {
                        _gemToUse = Random.Range(0, GameVariables.Instance.gems.Length);
                        iterations++;
                    }

                    SpawnGem(new Vector2Int(x, y), GameVariables.Instance.gems[_gemToUse], gameLogic);
                }
            }
        }

        public void SpawnGem(GemType gemType, Vector2Int position)
        {
            Gem gemToSpawn = null;
            if (gemType == GemType.Bomb)
            {
                gemToSpawn = Instantiate(GameVariables.Instance.bomb, new Vector3(position.x, position.y), Quaternion.identity, gemsHolder);
            }

            if (gemToSpawn == null) return;
            
            gemToSpawn.name = $"Gem [{position.x},{position.y}]";
            gameBoard.SetGem(position.x, position.y, gemToSpawn);
            gemToSpawn.SetupGem(position);
        }

        public void SpawnGem(Vector2Int _Position, Gem _GemToSpawn, GameLogic gameLogic)
        {
            if (Random.Range(0, 100f) < GameVariables.Instance.bombChance)
                _GemToSpawn = GameVariables.Instance.bomb;

            Gem _gem = Instantiate(_GemToSpawn,
                new Vector3(_Position.x, _Position.y + GameVariables.Instance.dropHeight, 0f), Quaternion.identity);
            _gem.transform.SetParent(gemsHolder);
            _gem.name = $"Gem [{_Position.x},{_Position.y}]";
            gameBoard.SetGem(_Position.x, _Position.y, _gem);
            _gem.SetupGem(_Position);
        }
    }
}
