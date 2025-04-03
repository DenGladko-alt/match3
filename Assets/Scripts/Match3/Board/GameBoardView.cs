using System;
using UnityEngine;
using Utility;

namespace Match3
{
    public class GameBoardView : MonoBehaviour
    {
        [SerializeField] private Transform boardTilesHolder;
        
        private GameVariablesManager gameVariables;

        private void Start()
        {
            ServiceLocator.Instance.TryGet(out gameVariables);

            CreateBoardTiles();
        }

        private void CreateBoardTiles()
        {
            if (gameVariables == null) ServiceLocator.Instance.TryGet(out gameVariables);
            
            for (int x = 0; x < gameVariables.LevelConfig.BoardWidth; x++)
            {
                for (int y = 0; y < gameVariables.LevelConfig.BoardHeight; y++)
                {
                    Vector2 _pos = new Vector2(x, y);
                    GameObject _bgTile = Instantiate(gameVariables.GameSettings.BoardBackgroundTilesPrefab, 
                        _pos, Quaternion.identity);
                    
                    _bgTile.transform.SetParent(boardTilesHolder);
                    _bgTile.name = "BG Tile - " + x + ", " + y;
                }
            }
        }
    }
}
