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
        
        public GameBoard GameBoard => gameBoard;
        public GameState CurrentState { get; private set; } = GameState.Move;

        [SerializeField, Range(0, 1f)] private float timeSpeed = 1f;

        #region MonoBehaviour

        private void Awake() => Init();

        private void Update()
        {
            Time.timeScale = timeSpeed;
        }

        #endregion

        #region Logic

        private void Init()
        {
            gameBoard.Setup();
            gemSpawnManager.Setup(this);
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
            //HashSet<Gem> specialGems = new HashSet<Gem>();

            // foreach (Gem gem in gameBoard.CurrentMatches)
            // {
            //     if (gem.GemType == GemType.Bomb)
            //     {
            //         specialGems.Add(gem);
            //     }
            // }
            //
            // gameBoard.CurrentMatches.RemoveWhere(gem => specialGems.Contains(gem));
            
            // IOrderedEnumerable<IGrouping<int, Gem>> groupedByDistance = gameBoard.CurrentMatches
            //     .GroupBy(gem => gem.DestroyOrder)
            //     .OrderBy(group => group.Key);
            
            //List<IGrouping<int, Gem>> groupedMatchesList = gameBoard.GroupedMatches.ToList();
            List<IGrouping<int, Gem>> groupedMatchesList = gameBoard.GroupedMatches.ToList();

            for (int i = 0; i < groupedMatchesList.Count; i++)
            {
                IGrouping<int, Gem> group = groupedMatchesList[i];

                // Check if the group has 4 or more matches
                bool isSpecialGroup = group.Count() >= GLOBAL_VARIABLES.SPECIAL_SPAWN_COUNT_RULE;

                var groupArray = group.ToArray(); // Convert the group to an array to avoid repeated enumeration
                for (int j = 0; j < groupArray.Length; j++)
                {
                    Gem gem = groupArray[j];
                    if (gem == null) continue;

                    bool movedByPlayer = gem.MovedByPlayer;
                    Vector2Int posIndex = gem.PosIndex;

                    DestroyGem(gem);

                    // Spawn a special gem only if the group is special and the gem was moved by the player
                    if (isSpecialGroup && movedByPlayer)
                    {
                        gemSpawnManager.SpawnGem(GemType.Bomb, posIndex);
                        isSpecialGroup = false; // Spawn only one special gem per group
                    }
                }
            }

            foreach (var gem in gameBoard.allGems)
            {
                if (gem == null) continue;
                
                gem.MovedByPlayer = false;
            }
            
            // TODO: Move to config
            yield return new WaitForSeconds(0.25f);

            if (gameBoard.MarkedBySpecials.Count > 0)
            {
                IOrderedEnumerable<IGrouping<int, Gem>> groupedByDistance = gameBoard.MarkedBySpecials
                    .GroupBy(gem => gem.DestroyOrder)
                    .OrderBy(group => group.Key);

                foreach (var gem in groupedByDistance)
                {
                    for (int i = 0; i < gem.Count(); i++)
                    {
                        if (gameBoard.MatchedGems.Contains(gem.ElementAt(i))) continue;
                        
                        DestroyGem(gem.ElementAt(i));
                    }
                
                    // TODO: Move to config
                    yield return new WaitForSeconds(1f);
                }
            }
            
            StartCoroutine(DecreaseRowCoroutine());
        }

        private void DestroyGem(Gem gem)
        {
            if (gem != null)
            {
                Vector2Int gemPos = new Vector2Int(gem.PosIndex.x, gem.PosIndex.y);
                gem.DestroyGem();
                gameBoard.SetGem(gemPos.x, gemPos.y, null);
            }
        }

        private IEnumerator DecreaseRowCoroutine()
        {
            // TODO: Time delay move to config
            yield return new WaitForSeconds(0.25f);
            
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
                        _curGem.PosIndex.y -= nullCounter;
                        gameBoard.SetGem(x, y - nullCounter, _curGem);
                        gameBoard.SetGem(x, y, null);
                    }
                }

                nullCounter = 0;
            }

            StartCoroutine(FilledBoardCoroutine());
        }

        private IEnumerator FilledBoardCoroutine()
        {
            // TODO: Time delay move to config
            yield return new WaitForSeconds(0.5f);
            RefillBoard();
            // TODO: Time delay move to config
            yield return new WaitForSeconds(0.5f);
            
            // Wait and destroy matches if any
            if (gameBoard.FindAllMatches())
            {
                // TODO: Time delay move to config
                yield return new WaitForSeconds(0.5f);
                DestroyMatches();
            }
            else
            {
                // TODO: Time delay move to config
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
            {
                Destroy(g.gameObject);
            }
        }

        #endregion
    }
}