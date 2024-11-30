using System.Collections;
using Match3;
using UnityEngine;

public class Gem : MonoBehaviour
{
    public DestroyPattern destroyPattern;
    
    [HideInInspector]
    public Vector2Int posIndex;

    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private bool mousePressed;
    private float swipeAngle = 0;
    private Gem otherGem;

    public GlobalEnums.GemType type;
    public bool isMatch = false;
    private Vector2Int previousPos;
    public GameObject destroyEffect;
    public int scoreValue = 10;
    
    private GameLogic _gameLogic;

    void Update()
    {
        if (Vector2.Distance(transform.position, posIndex) > 0.01f)
            transform.position = Vector2.Lerp(transform.position, posIndex, GameVariables.Instance.gemSpeed * Time.deltaTime);
        else
        {
            transform.position = new Vector3(posIndex.x, posIndex.y, 0);
            _gameLogic.SetGem(posIndex.x, posIndex.y, this);
        }
        if (mousePressed && Input.GetMouseButtonUp(0))
        {
            mousePressed = false;
            if (_gameLogic.CurrentState == GlobalEnums.GameState.Move)
            {
                finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                CalculateAngle();
            }
        }
    }

    public void SetupGem(GameLogic gameLogic, Vector2Int position)
    {
        posIndex = position;
        _gameLogic = gameLogic;
    }

    private void OnMouseDown()
    {
        if (_gameLogic.CurrentState == GlobalEnums.GameState.Move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePressed = true;
        }
    }

    private void CalculateAngle()
    {
        swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x);
        swipeAngle = swipeAngle * 180 / Mathf.PI;

        if (Vector3.Distance(firstTouchPosition, finalTouchPosition) > .5f)
            MovePieces();
    }

    private void MovePieces()
    {
        previousPos = posIndex;

        if (swipeAngle < 45 && swipeAngle > -45 && posIndex.x < GameVariables.Instance.rowsSize - 1)
        {
            otherGem = _gameLogic.GetGem(posIndex.x + 1, posIndex.y);
            otherGem.posIndex.x--;
            posIndex.x++;

        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && posIndex.y < GameVariables.Instance.colsSize - 1)
        {
            otherGem = _gameLogic.GetGem(posIndex.x, posIndex.y + 1);
            otherGem.posIndex.y--;
            posIndex.y++;
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && posIndex.y > 0)
        {
            otherGem = _gameLogic.GetGem(posIndex.x, posIndex.y - 1);
            otherGem.posIndex.y++;
            posIndex.y--;
        }
        else if (swipeAngle > 135 || swipeAngle < -135 && posIndex.x > 0)
        {
            otherGem = _gameLogic.GetGem(posIndex.x - 1, posIndex.y);
            otherGem.posIndex.x++;
            posIndex.x--;
        }

        _gameLogic.SetGem(posIndex.x,posIndex.y, this);
        _gameLogic.SetGem(otherGem.posIndex.x, otherGem.posIndex.y, otherGem);

        StartCoroutine(CheckMoveCoroutine());
    }

    public IEnumerator CheckMoveCoroutine()
    {
        _gameLogic.SetState(GlobalEnums.GameState.Wait);

        yield return new WaitForSeconds(.5f);
        _gameLogic.FindAllMatches();

        if (otherGem != null)
        {
            if (isMatch == false && otherGem.isMatch == false)
            {
                otherGem.posIndex = posIndex;
                posIndex = previousPos;

                _gameLogic.SetGem(posIndex.x, posIndex.y, this);
                _gameLogic.SetGem(otherGem.posIndex.x, otherGem.posIndex.y, otherGem);

                yield return new WaitForSeconds(.5f);
                _gameLogic.SetState(GlobalEnums.GameState.Move);
            }
            else
            {
                _gameLogic.DestroyMatches();
            }
        }
    }
}
