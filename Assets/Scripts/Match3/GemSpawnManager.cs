using UnityEngine;
using Utility;
using Random = UnityEngine.Random;

namespace Match3
{
    public class GemSpawnManager : MonoBehaviourService<GemSpawnManager>
    {
        [SerializeField] private Transform gemsHolder;
        
        private GameVariablesService gameVariables;

        private void Start()
        {
            ServiceManager.Instance.TryGet(out gameVariables);
        }

        public void Setup(GameBoard gameBoard)
        {
            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    int _gemToUse = Random.Range(0, gameVariables.LevelConfig.Gems.Length);

                    int iterations = 0;
                    while (Match3Utilities.MatchesAt(new Vector2Int(x, y), gameVariables.LevelConfig.Gems[_gemToUse], gameBoard.AllGems) &&
                           iterations < 100)
                    {
                        _gemToUse = Random.Range(0, gameVariables.LevelConfig.Gems.Length);
                        iterations++;
                    }

                    SpawnGem(new Vector2Int(x, y), gameVariables.LevelConfig.Gems[_gemToUse], gameBoard);
                }
            }
        }
        
        public void SpawnGem(GemType gemType, Vector2Int position, GameBoard gameBoard)
        {
            Gem gemToSpawn = null;
            if (gemType == GemType.Bomb)
            {
                gemToSpawn = Instantiate(gameVariables.GameSettings.BombPrefab, new Vector3(position.x, position.y), Quaternion.identity, gemsHolder);
            }
        
            if (gemToSpawn == null) return;
            
            gemToSpawn.name = $"Gem [{position.x},{position.y}]";
            
            // TODO: Violation of Single Responsibility, make it to get Gem in GameBoard (?)
            gameBoard.AllGems[position.x, position.y] = gemToSpawn;
            gemToSpawn.SetupGem(position);
        }

        // public void SpawnGem(GemType gemType, Vector2Int position)
        // {
        //     Gem gemToSpawn = null;
        //     if (gemType == GemType.Bomb)
        //     {
        //         gemToSpawn = Instantiate(gameVariables.GameSettings.BombPrefab, new Vector3(position.x, position.y), Quaternion.identity, gemsHolder);
        //     }
        //
        //     if (gemToSpawn == null) return;
        //     
        //     gemToSpawn.name = $"Gem [{position.x},{position.y}]";
        //     
        //     // TODO: Violation of Single Responsibility, make it to get Gem in GameBoard (?)
        //     gameBoard.AllGems[position.x, position.y] = gemToSpawn;
        //     gemToSpawn.SetupGem(position);
        // }

        public void SpawnGem(Vector2Int position, Gem gemToSpawn, GameBoard gameBoard)
        {
            if (Random.Range(0, 100f) < gameVariables.LevelConfig.BombSpawnChance)
                gemToSpawn = gameVariables.GameSettings.BombPrefab;

            Gem gem = Instantiate(gemToSpawn,
                new Vector3(position.x, position.y, 0f), Quaternion.identity);
            gem.transform.SetParent(gemsHolder);
            gem.name = $"Gem [{position.x},{position.y}]";
            gameBoard.AllGems[position.x, position.y] = gem;
            gem.SetupGem(position);
        }
    }
}
