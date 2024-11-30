﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    #region Variables

    [SerializeField] private int width = 7;
    [SerializeField] private int height = 7;
    [SerializeField] private Transform gemsFolder = null;

    private Gem[,] allGems;
    
    public int Width => width;
    public int Height => height;

    public List<Gem> CurrentMatches { get; private set; } = new List<Gem>();

    #endregion

    public void Setup()
    {
        allGems = new Gem[Width, Height];
        
        CreateBoardTiles();
    }

    private void CreateBoardTiles()
    {
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
            {
                Vector2 _pos = new Vector2(x, y);
                GameObject _bgTile = Instantiate(GameVariables.Instance.bgTilePrefabs, _pos, Quaternion.identity);
                _bgTile.transform.SetParent(gemsFolder);
                _bgTile.name = "BG Tile - " + x + ", " + y;
            }
    }

    public bool MatchesAt(Vector2Int _PositionToCheck, Gem _GemToCheck)
    {
        if (_PositionToCheck.x > 1)
        {
            if (allGems[_PositionToCheck.x - 1, _PositionToCheck.y].type == _GemToCheck.type &&
                allGems[_PositionToCheck.x - 2, _PositionToCheck.y].type == _GemToCheck.type)
                return true;
        }

        if (_PositionToCheck.y > 1)
        {
            if (allGems[_PositionToCheck.x, _PositionToCheck.y - 1].type == _GemToCheck.type &&
                allGems[_PositionToCheck.x, _PositionToCheck.y - 2].type == _GemToCheck.type)
                return true;
        }

        return false;
    }

    public void SetGem(int _X, int _Y, Gem _Gem)
    {
        allGems[_X, _Y] = _Gem;
    }
    public Gem GetGem(int _X, int _Y)
    {
       return allGems[_X, _Y];
    }

    public void FindAllMatches()
    {
        CurrentMatches.Clear();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Gem currentGem = allGems[x, y];
                if (currentGem != null)
                {
                    if (x > 0 && x < width - 1)
                    {
                        Gem leftGem = allGems[x - 1, y];
                        Gem rightGem = allGems[x + 1, y];
                        //checking no empty spots
                        if (leftGem != null && rightGem != null)
                        {
                            //Match
                            if (leftGem.type == currentGem.type && rightGem.type == currentGem.type)
                            {
                                currentGem.isMatch = true;
                                leftGem.isMatch = true;
                                rightGem.isMatch = true;
                                CurrentMatches.Add(currentGem);
                                CurrentMatches.Add(leftGem);
                                CurrentMatches.Add(rightGem);
                            }
                        }
                    }

                    if (y > 0 && y < height - 1)
                    {
                        Gem aboveGem = allGems[x, y - 1];
                        Gem bellowGem = allGems[x, y + 1];
                        //checking no empty spots
                        if (aboveGem != null && bellowGem != null)
                        {
                            //Match
                            if (aboveGem.type == currentGem.type && bellowGem.type == currentGem.type)
                            {
                                currentGem.isMatch = true;
                                aboveGem.isMatch = true;
                                bellowGem.isMatch = true;
                                CurrentMatches.Add(currentGem);
                                CurrentMatches.Add(aboveGem);
                                CurrentMatches.Add(bellowGem);
                            }
                        }
                    }
                }
            }

        if (CurrentMatches.Count > 0)
            CurrentMatches = CurrentMatches.Distinct().ToList();

        CheckForBombs();
    }

    private void CheckForBombs()
    {
        for (int i = 0; i < CurrentMatches.Count; i++)
        {
            Gem gem = CurrentMatches[i];
            int x = gem.posIndex.x;
            int y = gem.posIndex.y;

            CheckAndExecuteSpecialPattern(x, y + 1); // Above
            CheckAndExecuteSpecialPattern(x + 1, y); // Right
            CheckAndExecuteSpecialPattern(x, y - 1); // Below
            CheckAndExecuteSpecialPattern(x - 1, y); // Left
        }
    }
    
    private void CheckAndExecuteSpecialPattern(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return;
        
        Gem gem = allGems[x, y];
        
        if (gem == null || gem.type != GlobalEnums.GemType.Special) return;
        
        List<Vector2Int> patternPositions = gem.destroyPattern?.GetPattern(new Vector2Int(x, y));
        
        if (patternPositions == null) return;

        for (int i = 0; i < patternPositions.Count; i++)
        {
            Vector2Int pos = patternPositions[i];
            if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height) continue;

            Gem gemToDestroy = allGems[pos.x, pos.y];

            if (gemToDestroy == null) continue;

            gemToDestroy.isMatch = true;
            if (!CurrentMatches.Contains(gemToDestroy)) CurrentMatches.Add(gemToDestroy);
        }
    }
}

