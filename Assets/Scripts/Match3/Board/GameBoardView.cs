using System;
using UnityEngine;
using Utility;

namespace Match3
{
    public class GameBoardView : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private Transform boardTilesHolder;
        
        private GameVariablesService gameVariables;
        
        private bool initialized = false;

        private void OnEnable()
        {
            if (!initialized)
            {
                gameBoard.OnGameBoardSetup += OnGameBoardSetup;
            }
        }

        private void OnDisable()
        {
            gameBoard.OnGameBoardSetup -= OnGameBoardSetup;
        }

        private void OnGameBoardSetup()
        {
            CreateBoardTiles();
            initialized = true;
        }

        private void Start()
        {
            ServiceManager.Instance.TryGet(out gameVariables);
        }

        private void CreateBoardTiles()
        {
            if (gameVariables == null) ServiceManager.Instance.TryGet(out gameVariables);
            
            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
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
