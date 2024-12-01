using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Match3
{
    public class GameLogic : MonoBehaviour
    {
        [SerializeField] private GameBoard gameBoard;
        [SerializeField] private GemSpawnManager gemSpawnManager;

        public GameState CurrentState { get; private set; } = GameState.Move;

        #region MonoBehaviour

        private void Awake() => Init();

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
            StartCoroutine(DestroyMatchedGemsCoroutine());
        }

        
        private IEnumerator DestroyMatchedGemsCoroutine()
        {
            // Get specials gems to destroy them later
            List<Gem> specialGems = new List<Gem>();

            for (int i = 0; i < gameBoard.CurrentMatches.Count; i++)
            {
                if (gameBoard.CurrentMatches[i].GemType == GemType.Special)
                {
                    specialGems.Add(gameBoard.CurrentMatches[i]);
                    gameBoard.CurrentMatches.Remove(gameBoard.CurrentMatches[i]);
                }
            }
            
            var groupedByDistance = gameBoard.CurrentMatches
                .GroupBy(gem => gem.DestroyOrder)
                .OrderBy(group => group.Key);
            
            // Destroy simple gems
            for (int i = 0; i < groupedByDistance.Count(); i++)
            {
                // Destroy all gems in the current group
                foreach (var gem in groupedByDistance.ElementAt(i))
                {
                    DestroyGem(gem);
                }

                // Wait before processing the next group
                // TODO: Change to config file or something
                yield return new WaitForSeconds(0.25f);
            }

            // Destroy special gems
            foreach (var gem in specialGems)
            {
                DestroyGem(gem);
            }
            
            StartCoroutine(DecreaseRowCoroutine());
        }

        private void DestroyGem(Gem gem)
        {
            if (gem != null)
            {
                Vector2Int gemPos = new Vector2Int(gem.posIndex.x, gem.posIndex.y);
                gem.DestroyGem();
                SetGem(gemPos.x, gemPos.y, null);
            }
        }

        private IEnumerator DecreaseRowCoroutine()
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

            StartCoroutine(FilledBoardCoroutine());
        }

        private IEnumerator FilledBoardCoroutine()
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

        #endregion
    }
}
