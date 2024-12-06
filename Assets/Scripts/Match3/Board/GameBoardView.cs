using Match3;
using UnityEngine;

public class GameBoardView : MonoBehaviour
{
    [SerializeField] private GameBoard gameBoard;
    [SerializeField] private Transform boardTilesHolder = null;

    private void Start()
    {
        CreateBoardTiles();
    }

    private void CreateBoardTiles()
    {
        for (int x = 0; x < gameBoard.Width; x++)
        {
            for (int y = 0; y < gameBoard.Height; y++)
            {
                Vector2 _pos = new Vector2(x, y);
                GameObject _bgTile = Instantiate(GameVariables.Instance.bgTilePrefabs, _pos, Quaternion.identity);
                _bgTile.transform.SetParent(boardTilesHolder);
                _bgTile.name = "BG Tile - " + x + ", " + y;
            }
        }
    }
}
