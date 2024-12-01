using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Match3
{
    public class GameLogic : MonoBehaviour
    {
        public static event Action<int> OnScoreChanged;

        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private GemSpawnManager gemSpawnManager;

        private int score = 0;

        public GameState CurrentState { get; private set; } = GameState.Move;

        #region MonoBehaviour

        private void Awake()
        {
            Init();
        }

        #endregion

        #region Logic

        private void Init()
        {
            gameBoard.Setup();
            gemSpawnManager.Setup(this);
        }

        public void SetGem(int _X, int _Y, Gem _Gem)
        {
            gameBoard.SetGem(_X, _Y, _Gem);
        }

        public Gem GetGem(int _X, int _Y)
        {
            return gameBoard.GetGem(_X, _Y);
        }

        public void SetState(GameState _CurrentState)
        {
            CurrentState = _CurrentState;
        }

        public void DestroyMatches()
        {
            for (int i = 0; i < gameBoard.CurrentMatches.Count; i++)
                if (gameBoard.CurrentMatches[i] != null)
                {
                    ScoreCheck(gameBoard.CurrentMatches[i]);
                    DestroyMatchedGemsAt(gameBoard.CurrentMatches[i].posIndex);
                }

            StartCoroutine(DecreaseRowCo());
        }

        private IEnumerator DecreaseRowCo()
        {
            yield return new WaitForSeconds(.2f);

            int nullCounter = 0;
            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    Gem _curGem = gameBoard.GetGem(x, y);
                    if (_curGem == null)
                    {
                        nullCounter++;
                    }
                    else if (nullCounter > 0)
                    {
                        _curGem.posIndex.y -= nullCounter;
                        SetGem(x, y - nullCounter, _curGem);
                        SetGem(x, y, null);
                    }
                }

                nullCounter = 0;
            }

            StartCoroutine(FilledBoardCo());
        }

        private void ScoreCheck(Gem gemToCheck)
        {
            score += gemToCheck.scoreValue;

            OnScoreChanged?.Invoke(score);
        }

        private void DestroyMatchedGemsAt(Vector2Int _Pos)
        {
            Gem _curGem = gameBoard.GetGem(_Pos.x, _Pos.y);
            if (_curGem != null)
            {
                _curGem.PlayDestroyEffect();
                Destroy(_curGem.gameObject);
                SetGem(_Pos.x, _Pos.y, null);
            }
        }

        private IEnumerator FilledBoardCo()
        {
            yield return new WaitForSeconds(0.5f);
            RefillBoard();
            yield return new WaitForSeconds(0.5f);
            gameBoard.FindAllMatches();
            if (gameBoard.CurrentMatches.Count > 0)
            {
                yield return new WaitForSeconds(0.5f);
                DestroyMatches();
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                CurrentState = GameState.Move;
            }
        }

        private void RefillBoard()
        {
            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    Gem _curGem = gameBoard.GetGem(x, y);
                    if (_curGem == null)
                    {
                        int gemToUse = Random.Range(0, GameVariables.Instance.gems.Length);
                        gemSpawnManager.SpawnGem(new Vector2Int(x, y), GameVariables.Instance.gems[gemToUse], this);
                    }
                }
            }

            CheckMisplacedGems();
        }

        private void CheckMisplacedGems()
        {
            List<Gem> foundGems = new List<Gem>();
            foundGems.AddRange(FindObjectsOfType<Gem>());
            for (int x = 0; x < gameBoard.Width; x++)
            {
                for (int y = 0; y < gameBoard.Height; y++)
                {
                    Gem _curGem = gameBoard.GetGem(x, y);
                    if (foundGems.Contains(_curGem))
                        foundGems.Remove(_curGem);
                }
            }

            foreach (Gem g in foundGems)
                Destroy(g.gameObject);
        }

        public void FindAllMatches()
        {
            gameBoard.FindAllMatches();
        }

        #endregion
    }
}
