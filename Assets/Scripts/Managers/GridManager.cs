using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [SerializeField] private static int _width = 16, _height = 8;

    [SerializeField] private GameObject TilePrefab, MoveHighlight, EnemyHighlight, FOVHighlight, GadgetHighlight, WallX, WallY, WindowFrameX, WindowFrameY, DoorFrameX, DoorFrameY, HalfWallX, HalfWallY, UnitPrefab, UnitControlsUI, clockUI, FlagB, FlagR, ShrapnelGroup, SmokeHitbox, Smoke;

    [SerializeField] private List<MovesCount> movesCounts = new List<MovesCount>();

    [SerializeField] private CameraController cameraController;

    [SerializeField] private StartData.GameSettingsData data;

    [SerializeField] private GridTools.Map currentMap;
    [SerializeField] private GridTools.MapPreview currentPreview;

    StartData.gameMode gameMode;

    int[,] onBoardGadgets;
    public Unit[,] onBoardEntities;
    GameObject[,] onBoardChecks, FOVChecks;    // array of highlights (attack and move)
    Wall[,] BoardConnectionGridX;
    Wall[,] BoardConnectionGridY;

    [SerializeField] private GameObject[] blueTeam, redTeam;

    Flag flagB, flagR;
    int activeX = 0, activeY = 0, passiveX = 0, passiveY = 0, blueTeamCount, redTeamCount, gameTime;
    bool active = false, passive = false, turnActive = false, moveHighlightsOn = false, unitIsSelected = false, cameraZoomed = false, firstMove = true;
    public Unit selectedUnit;

    [SerializeField] private TextWriter clockCounter;

    [SerializeField] private bool turnSide;

    LayerMask wallMask, halfWallMask, smokeMask;

    public event EventHandler destroyHighlights, nextTurn, TurnB, TurnR, displayUnitInfoEvent, updateUnitsInfoList;

    [System.Serializable]
    public struct TileCoordinates
    {
        public int x;
        public int y;
        public TileCoordinates(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public GameObject[] getBlueTeam()
    {
        return blueTeam;
    }
    public GameObject[] getRedTeam()
    {
        return redTeam;
    }

    public List<TileCoordinates> MidpointCircleAlgorithmScan(int x, int y, int range, bool centerIncluded)
    {
        List <TileCoordinates> detectedObjects = new List<TileCoordinates>();

        //          center check
        if (centerIncluded)
        {
            detectedObjects.Add(new TileCoordinates(x, y));
        }
        //          cross check
        for (int i = 0; i < range; i++)
        {
            if (i + y - range >= 0 && i + y - range < _height)      // check down
            {
                    detectedObjects.Add(new TileCoordinates(x, i + y - range));
            }

            if (i + 1 + y >= 0 && i + 1 + y < _height)      // check up
            {
                    detectedObjects.Add(new TileCoordinates(x, i + 1 + y));
            }

            if (i + x - range >= 0 && i + x - range < _width)       // check left
            {
                    detectedObjects.Add(new TileCoordinates(i + x - range, y));
            }

            if (i + x + 1 >= 0 && i + x + 1 < _width)       // check right
            {
                    detectedObjects.Add(new TileCoordinates(i + x + 1, y));
            }

        }

        //          circle algorithm
        int m = y + range;
        for (int i = 1; i <= m - y; i++)
        {
            float pktE = (float)(Math.Sqrt(i * i + (m - y) * (m - y))), pktSE = (float)(Math.Sqrt(i * i + (m - y - 1) * (m - y - 1)));
            float ra = Helper.absDif(pktE, range), rb = Helper.absDif(pktSE, range); 
            if (ra > rb)
            {
                m--;
            }

            // if conditions: up -> (y + j < _height); down -> (y + j >= 0); left -> (x + i >= 0); right -> (x + i < _width)

            for (int j = 1 + i; j <= m - y; j++)            // up, right, mid
            {
                if (y + j < _height && x + i < _width)
                {
                        detectedObjects.Add(new TileCoordinates(x + i, y + j));
                }


                if (y - j >= 0 && x + i < _width)            // down, right, mid
                {
                        detectedObjects.Add(new TileCoordinates(x + i, y - j));
                }

                if (y + j < _height && x - i >= 0)        // up, left, mid
                {
                        detectedObjects.Add(new TileCoordinates(x - i, y + j));
                }


                if (y - j >= 0 && x - i >= 0)            // down, left, mid
                {
                        detectedObjects.Add(new TileCoordinates(x - i, y - j));
                }


                if (y - i >= 0 && x - j >= 0)            // down, left, side
                {
                        detectedObjects.Add(new TileCoordinates(x - j, y - i));
                }


                if (y - i >= 0 && x + j < _width)            // down, right, side
                { 
                        detectedObjects.Add(new TileCoordinates(x + j, y - i));
                }


                if (y + i < _height && x - j >= 0)            // up, left, side
                {
                        detectedObjects.Add(new TileCoordinates(x - j, y + i));
                }


                if (y + i < _height && x + j < _width)            // up, right, side
                {
                        detectedObjects.Add(new TileCoordinates(x + j, y + i));
                }
            }
        }
        
        // best bevels
        m = range;
        float _diagonalsA = range * (float)(Math.Sqrt(2));
        float _diagonalsB = (range - 1) * (float)(Math.Sqrt(2));
        while (_diagonalsA > range && _diagonalsB > range)
        {
            m--;
            _diagonalsA = m * (float)(Math.Sqrt(2));
            _diagonalsB = (m - 1) * (float)(Math.Sqrt(2));
        }

        if (Helper.absDif(_diagonalsA, range) > Helper.absDif(_diagonalsB, range))
        {
            m--;
        }

        for (int i = 1; i < m; i++)
        {
            if (y + i < _height && x + i < _width)          // up, right
            {
                    detectedObjects.Add(new TileCoordinates(x + i, y + i));
            }

            if (y - i >= 0 && x + i < _width)          // down, right
            {
                    detectedObjects.Add(new TileCoordinates(x + i, y - i));
            }

            if (y + i < _height && x - i >= 0)          // up, left
            {
                    detectedObjects.Add(new TileCoordinates(x - i, y + i));
            }

            if (y - i >= 0 && x - i >= 0)          // down, left
            {
                    detectedObjects.Add(new TileCoordinates(x - i, y - i));
            }
        }

        return detectedObjects;
    }

    public void eliminateUnit(bool team)
    {
        if(team)
        {
            blueTeamCount--;
        }
        else
        {
            redTeamCount--;
        }
        if(blueTeamCount <= 0)
        {
            GameManager.Instance.winGame(false);
        }
        if (redTeamCount <= 0)
        {
            GameManager.Instance.winGame(true);
        }
    }

    void clockTick(object sender, EventArgs e)
    {
        bool flagNotBlocked = true;
        bool flagContested = false;

        //flag checks
        if ((gameMode == StartData.gameMode.defenceB || gameMode == StartData.gameMode.defence) && GameManager.Instance.getGameState() == GameState.EnemyTurn)
        {
            if (onBoardEntities[currentMap.gameplayObjects[0].Value.x, currentMap.gameplayObjects[0].Value.y])
            {
                if (!onBoardEntities[currentMap.gameplayObjects[0].Value.x, currentMap.gameplayObjects[0].Value.y].whatTeam())
                {
                    flagContested = true;
                }
                else
                {
                    flagNotBlocked = false;
                }
            }
            if (onBoardEntities[currentMap.gameplayObjects[0].Value.x - 1, currentMap.gameplayObjects[0].Value.y])
            {
                if (!onBoardEntities[currentMap.gameplayObjects[0].Value.x - 1, currentMap.gameplayObjects[0].Value.y].whatTeam())
                {
                    flagContested = true;
                }
                else
                {
                    flagNotBlocked = false;
                }
            }
            if (onBoardEntities[currentMap.gameplayObjects[0].Value.x, currentMap.gameplayObjects[0].Value.y - 1])
            {
                if (!onBoardEntities[currentMap.gameplayObjects[0].Value.x, currentMap.gameplayObjects[0].Value.y - 1].whatTeam())
                {
                    flagContested = true;
                }
                else
                {
                    flagNotBlocked = false;
                }
            }
            if (onBoardEntities[currentMap.gameplayObjects[0].Value.x - 1, currentMap.gameplayObjects[0].Value.y - 1])
            {
                if (!onBoardEntities[currentMap.gameplayObjects[0].Value.x - 1, currentMap.gameplayObjects[0].Value.y - 1].whatTeam())
                {
                    flagContested = true;
                }
                else
                {
                    flagNotBlocked = false;
                }
            }
            if (flagNotBlocked && flagContested)
            {
                flagB.flagDown();
            }
        }
        else if ((gameMode == StartData.gameMode.defenceR || gameMode == StartData.gameMode.defence) && GameManager.Instance.getGameState() == GameState.PlayerTurn)
        {
            if (onBoardEntities[currentMap.gameplayObjects[1].Value.x, currentMap.gameplayObjects[1].Value.y])
            {
                if (onBoardEntities[currentMap.gameplayObjects[1].Value.x, currentMap.gameplayObjects[1].Value.y].whatTeam())
                {
                    flagContested = true;
                }
                else
                {
                    flagNotBlocked = false;
                }
            }
            if (onBoardEntities[currentMap.gameplayObjects[1].Value.x - 1, currentMap.gameplayObjects[1].Value.y])
            {
                if (onBoardEntities[currentMap.gameplayObjects[1].Value.x - 1, currentMap.gameplayObjects[1].Value.y].whatTeam())
                {
                    flagContested = true;
                }
                else
                {
                    flagNotBlocked = false;
                }
            }
            if (onBoardEntities[currentMap.gameplayObjects[1].Value.x, currentMap.gameplayObjects[1].Value.y - 1])
            {
                if (onBoardEntities[currentMap.gameplayObjects[1].Value.x, currentMap.gameplayObjects[1].Value.y - 1].whatTeam())
                {
                    flagContested = true;
                }
                else
                {
                    flagNotBlocked = false;
                }
            }
            if (onBoardEntities[currentMap.gameplayObjects[1].Value.x - 1, currentMap.gameplayObjects[1].Value.y - 1])
            {
                if (onBoardEntities[currentMap.gameplayObjects[1].Value.x - 1, currentMap.gameplayObjects[1].Value.y - 1].whatTeam())
                {
                    flagContested = true;
                }
                else
                {
                    flagNotBlocked = false;
                }
            }
            if (flagNotBlocked && flagContested)
            {
                flagR.flagDown();
            }
        }

        // clock

        if (!firstMove)
        {
            if (gameMode == StartData.gameMode.defenceR || gameMode == StartData.gameMode.defenceB)
            {
                if (!flagContested)
                {
                    if (gameMode == StartData.gameMode.defenceR)
                    {
                        gameTime--;
                        if (gameTime <= 0)
                        {
                            GameManager.Instance.winGame(false);
                        }
                    }
                    else if (gameMode == StartData.gameMode.defenceB)
                    {
                        gameTime--;
                        if (gameTime <= 0)
                        {
                            GameManager.Instance.winGame(true);
                        }
                    }
                    clockCounter.showMessage(gameTime.ToString());
                }
            }
            else
            {
                gameTime--;
                clockCounter.showMessage(gameTime.ToString());
                if (gameTime <= 0)
                {
                    CheckTheEndScore();
                }
            }
        }
        else
        {
            firstMove = false;
        }
    }

    void CheckTheEndScore()
    {
        int _ScoreB = 0, _ScoreR = 0;
        for(int i = 0; i < blueTeam.Length; i++)
        {
            if(blueTeam[i])
            {
                _ScoreB += blueTeam[i].GetComponent<Unit>().whatHP();
            }
        }

        for (int i = 0; i < redTeam.Length; i++)
        {
            if (blueTeam[i])
            {
                _ScoreR += redTeam[i].GetComponent<Unit>().whatHP();
            }
        }

        if (gameMode == StartData.gameMode.defence)
        {
            _ScoreB += flagB.getLevel() * 50;
            _ScoreR += flagR.getLevel() * 50;
        }

        if (_ScoreB > _ScoreR)
        {
            GameManager.Instance.winGame(true);
        }
        else if (_ScoreB < _ScoreR)
        {
            GameManager.Instance.winGame(false);
        }
        else
        {
            GameManager.Instance.gameEndDraw();
        }
    }

    void ClearBoardCheck(object sender, EventArgs e)
    {
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                if (onBoardChecks[i, j])
                {
                    onBoardChecks[i, j] = null;
                }
                if (FOVChecks[i, j])
                {
                    FOVChecks[i, j] = null;
                }
            }
        }
    }

    void Awake()
    {
        Instance = this;
        destroyHighlights += ClearBoardCheck;
        data = StartData.Instance.getData();
        wallMask = LayerMask.GetMask("Wall");
        halfWallMask = LayerMask.GetMask("HalfWall");
        smokeMask = LayerMask.GetMask("Smoke");
    }

    public Unit getUnitFromTile(int x, int y)
    {
        return onBoardEntities[x, y];
    }

    public void ResetHighlights()
    {
        if (destroyHighlights != null)
        {
            destroyHighlights(this, EventArgs.Empty);
        }
        if (selectedUnit.isGadgetActive())
        {
            HighlightForGadget(onBoardEntities[activeX, activeY].whatGadget());
        }
        else
        {
            HighlightPossibleMoves(activeX, activeY, onBoardEntities[activeX, activeY].whatRange());
        }
        moveHighlightsOn = true;
        UpdateMovesCount();
    }

    public void HighlightForGadget( Unit.gadget gadgetType)
    {
        switch(gadgetType)
        {
            case Unit.gadget.grenade:
                HighlightGadgetUse(activeX, activeY, 4, false, true);
                break;
            case Unit.gadget.smoke:
                HighlightGadgetUse(activeX, activeY, 4, false, true);
                break;
            default:
                break;
        }
    }

    public int getHeight()
    {
        return _height;
    }

    public int getWidth()
    {
        return _width;
    }

    public void GenerateGrid()
    {
        //for (int x = 0; x < _width; x++)
        //{
        //    for (int y = 0; y < _height; y++)
        //    {
        //        onBoardEntities[x,y] = null;
        //        onBoardGadgets[x, y] = 0;
        //    }
        //}
        //
        string filePath = data.mapFilePath;
        Debug.Log(filePath);
        string mapData = System.IO.File.ReadAllText(filePath);
        GridTools.MapIntermediate mapIntermediate = JsonUtility.FromJson< GridTools.MapIntermediate >(mapData);
        currentMap = GridTools.translateMapFromIntermediate(mapIntermediate);
        blueTeamCount = currentMap.teamSize.Value;
        redTeamCount = currentMap.teamSize.Value;
        Debug.Log("Loading finished");
        gameTime = currentMap.movesLimit;
        onBoardGadgets = new int[currentMap.width, currentMap.height];
        onBoardEntities = new Unit[currentMap.width, currentMap.height];
        onBoardChecks = new GameObject[currentMap.width, currentMap.height];    // array of highlights (attack and move)
        FOVChecks = new GameObject[currentMap.width, currentMap.height];    // array of highlights (attack and move)
        BoardConnectionGridX = new Wall[currentMap.height, currentMap.width - 1];
        BoardConnectionGridY = new Wall[currentMap.height - 1, currentMap.width];
        currentPreview = new GridTools.MapPreview(currentMap.width, currentMap.height, currentMap.teamSize.Value);
        _width = currentMap.width;
        _height = currentMap.height;
        for (int x = 0; x < currentMap.width; x++)
        {
            for (int y = 0; y < currentMap.height; y++)
            {
                GameObject spawnedTile = Instantiate(TilePrefab, new Vector3(x - y * 0.5f, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
                currentPreview.gridPreview[x, y] = spawnedTile.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>();
                switch (currentMap.tileGrid[x, y].tileType)
                {
                    case GridTools.tileType.blank:
                        currentPreview.gridPreview[x, y].SetCategoryAndLabel(currentPreview.gridPreview[x, y].GetCategory(), "Blank");
                        break;
                    case GridTools.tileType.cobblestone:
                        currentPreview.gridPreview[x, y].SetCategoryAndLabel(currentPreview.gridPreview[x, y].GetCategory(), "Cobblestone");
                        break;
                    case GridTools.tileType.sand:
                        currentPreview.gridPreview[x, y].SetCategoryAndLabel(currentPreview.gridPreview[x, y].GetCategory(), "Sand");
                        break;
                    case GridTools.tileType.sandRoad:
                        currentPreview.gridPreview[x, y].SetCategoryAndLabel(currentPreview.gridPreview[x, y].GetCategory(), "SandRoad");
                        break;
                    case GridTools.tileType.woodenFloor1:
                        currentPreview.gridPreview[x, y].SetCategoryAndLabel(currentPreview.gridPreview[x, y].GetCategory(), "WoodFloor1");
                        break;
                    default:
                        break;
                }
                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.GetComponent<Tile>().Init(isOffset, x, y);
            }
        }
        cameraController.SetDefaultPosition(((float)currentMap.width - ((float)currentMap.height - 1) * 0.5f) / 2 - 0.5f, (float)currentMap.height / 2 - 0.5f);
        cameraController.SetBorders(currentMap.width, currentMap.height);
        for (int x = 0; x < currentMap.width + 1; x++)
        {
            for (int y = 0; y < currentMap.height; y++)
            {
                if (currentMap.verticalWalls[x, y].HasValue)
                {
                    GameObject choosedWallPrefab;// wall type prefabs
                    switch (currentMap.verticalWalls[x, y].Value.wallType)
                    {
                        case Wall.wallType.frame:
                            choosedWallPrefab = DoorFrameY;
                            break;
                        case Wall.wallType.window:
                            choosedWallPrefab = WindowFrameY;
                            break;
                        case Wall.wallType.wall:
                            choosedWallPrefab = WallY;
                            break;
                        default:
                            choosedWallPrefab = HalfWallY;
                            break;
                    }
                    GameObject spawnedWall;
                    spawnedWall = Instantiate(choosedWallPrefab, new Vector3(x - y * 0.5f - 0.5f, y + 0.5f), Quaternion.identity);
                    currentPreview.verticalWalls[x, y] = spawnedWall.GetComponent<Wall>();
                    currentPreview.verticalWalls[x, y].setWallInfo(currentMap.verticalWalls[x, y].Value); 
                    currentPreview.verticalWalls[x, y].setLayer(currentMap.width, currentMap.height);
                    currentPreview.verticalWalls[x, y].spawnHitbox();
                }
            }
        }
        for (int x = 0; x < currentMap.width; x++)
        {
            for (int y = 0; y < currentMap.height + 1; y++)
            {
                if (currentMap.horizontalWalls[x, y].HasValue)
                {
                    GameObject choosedWallPrefab;// wall type prefabs
                    switch (currentMap.horizontalWalls[x, y].Value.wallType)
                    {
                        case Wall.wallType.frame:
                            choosedWallPrefab = DoorFrameX;
                            break;
                        case Wall.wallType.window:
                            choosedWallPrefab = WindowFrameX;
                            break;
                        case Wall.wallType.wall:
                            choosedWallPrefab = WallX;
                            break;
                        default:
                            choosedWallPrefab = HalfWallX;
                            break;
                    }
                    GameObject spawnedWall;
                    spawnedWall = Instantiate(choosedWallPrefab, new Vector3(x - y * 0.5f + 0.25f, y), Quaternion.identity);
                    currentPreview.horizontalWalls[x, y] = spawnedWall.GetComponent<Wall>();
                    currentPreview.horizontalWalls[x, y].setWallInfo(currentMap.horizontalWalls[x, y].Value);
                    currentPreview.horizontalWalls[x, y].setLayer(currentMap.width, currentMap.height);
                    currentPreview.horizontalWalls[x, y].spawnHitbox();
                }
            }
        }
        GameManager.Instance.ChangeGameState(GameState.SetLevel);
    }

    public void HideWall(int x, int y)
    {
        if (onBoardEntities[x, y] || onBoardChecks[x, y] || FOVChecks[x, y])
        {
            if (y > 0)
            {
                if (currentPreview.horizontalWalls[x, y])
                {
                    currentPreview.horizontalWalls[x, y].hide();
                }
                if (x > 0)
                {
                    if (currentPreview.verticalWalls[x, y - 1])
                    {
                        currentPreview.verticalWalls[x, y - 1].hide();
                    }
                }
            }
        }
    }

    public void UnHideWall(int x, int y)
    {
        if ((!onBoardEntities[x, y] && !onBoardChecks[x, y]) && (onBoardGadgets[x, y] <= 0 && !FOVChecks[x, y]))
        {
            if (y > 0)
            {
                if (currentPreview.horizontalWalls[x, y])
                {
                    currentPreview.horizontalWalls[x, y].appear();
                }
                if (x > 0)
                {
                    if (currentPreview.verticalWalls[x, y - 1])
                    {
                        currentPreview.verticalWalls[x, y - 1].appear();
                    }
                }
            }
        }
    }

    //onBoardGadgets

    public void HideWallForGadget(int x, int y)
    {
        onBoardGadgets[x, y]++;
        if (y > 0)
        {
            if (BoardConnectionGridY[y - 1, x])
            {
                BoardConnectionGridY[y - 1, x].hide();
            }
            if (x > 0)
            {
                if (BoardConnectionGridX[y - 1, x - 1])
                {
                    BoardConnectionGridX[y - 1, x - 1].hide();
                }
            }
        }
    }

    public void UnHideWallForGadget(int x, int y)
    {
        onBoardGadgets[x, y]--;
        if ((!onBoardEntities[x, y] && !onBoardChecks[x, y]) && onBoardGadgets[x, y] <= 0)
        {
            if (y > 0)
            {
                if (BoardConnectionGridY[y - 1, x])
                {
                    BoardConnectionGridY[y - 1, x].appear();
                }
                if (x > 0)
                {
                    if (BoardConnectionGridX[y - 1, x - 1])
                    {
                        BoardConnectionGridX[y - 1, x - 1].appear();
                    }
                }
            }
        }
    }

    public void SetBoard()
    {
        //for(int i = 0; i < _width; i++)       //debug character layers
        //{
        //    for(int j = 0; j < _height; j++)
        //    {
        //        GameObject newBlueUnit = Instantiate(UnitPrefab, new Vector3(i - 0.5f * j, j), Quaternion.identity);
        //        newBlueUnit.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        //        Unit newBlueUnitUnit = newBlueUnit.GetComponent<Unit>();
        //        newBlueUnitUnit.SetOnGridPosition(i, j);
        //        newBlueUnitUnit.SetData(data.BlueTeam[0]);
        //    }
        //}
        gameMode = data.GameMode;
        blueTeam = new GameObject[currentMap.teamSize.Value];
        redTeam = new GameObject[currentMap.teamSize.Value];
        for (int i = 0; i < currentMap.teamSize.Value; i++)
        {
            GameObject newBlueUnit = Instantiate(UnitPrefab, new Vector3(currentMap.spawnsBlue[i].Value.x - 0.5f * currentMap.spawnsBlue[i].Value.y, currentMap.spawnsBlue[i].Value.y), Quaternion.identity);
            blueTeam[i] = newBlueUnit;
            newBlueUnit.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            onBoardEntities[currentMap.spawnsBlue[i].Value.x, currentMap.spawnsBlue[i].Value.y] = newBlueUnit.GetComponent<Unit>();
            onBoardEntities[currentMap.spawnsBlue[i].Value.x, currentMap.spawnsBlue[i].Value.y].SetOnGridPosition(currentMap.spawnsBlue[i].Value.x, currentMap.spawnsBlue[i].Value.y);
            onBoardEntities[currentMap.spawnsBlue[i].Value.x, currentMap.spawnsBlue[i].Value.y].SetData(data.BlueTeam[i]);
            HideWall(currentMap.spawnsBlue[i].Value.x, currentMap.spawnsBlue[i].Value.y);

            GameObject newRedUnit = Instantiate(UnitPrefab, new Vector3(currentMap.spawnsRed[i].Value.x - 0.5f * currentMap.spawnsRed[i].Value.y, currentMap.spawnsRed[i].Value.y), Quaternion.identity);
            redTeam[i] = newRedUnit;
            newRedUnit.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
            onBoardEntities[currentMap.spawnsRed[i].Value.x, currentMap.spawnsRed[i].Value.y] = newRedUnit.GetComponent<Unit>();
            onBoardEntities[currentMap.spawnsRed[i].Value.x, currentMap.spawnsRed[i].Value.y].SetOnGridPosition(currentMap.spawnsRed[i].Value.x, currentMap.spawnsRed[i].Value.y);
            onBoardEntities[currentMap.spawnsRed[i].Value.x, currentMap.spawnsRed[i].Value.y].SetData(data.RedTeam[i]);
            HideWall(currentMap.spawnsRed[i].Value.x, currentMap.spawnsRed[i].Value.y);
        }

        gameMode = data.GameMode;

        if (gameMode == StartData.gameMode.defence || gameMode == StartData.gameMode.defenceB)
        {
            FlagB = Instantiate(FlagB, new Vector3((float)currentMap.gameplayObjects[0].Value.x - 0.5f * (float)(currentMap.gameplayObjects[0].Value.y - 1) - 0.75f, (float)(currentMap.gameplayObjects[0].Value.y - 1) + 0.5f), Quaternion.identity);
            flagB = FlagB.GetComponent<Flag>();
            flagB.setLayer(currentMap.gameplayObjects[0].Value.x, currentMap.gameplayObjects[0].Value.y - 1, _width, _height);
        }

        if (gameMode == StartData.gameMode.defence || gameMode == StartData.gameMode.defenceR)
        {
            FlagR = Instantiate(FlagR, new Vector3(currentMap.gameplayObjects[1].Value.x - 0.5f * (currentMap.gameplayObjects[1].Value.y - 1) - 0.75f, (currentMap.gameplayObjects[1].Value.y - 1) + 0.5f), Quaternion.identity);
            flagR = FlagR.GetComponent<Flag>();
            flagR.setLayer(currentMap.gameplayObjects[0].Value.x, currentMap.gameplayObjects[1].Value.y - 1, _width, _height);
        }

        clockCounter.showMessage(gameTime.ToString());

        switch (gameMode)
        {
            case StartData.gameMode.defenceR:
                TurnR += clockTick;
                clockUI.SetActive(true);
                break;
            case StartData.gameMode.defenceB:
                TurnB += clockTick;
                clockUI.SetActive(true);
                break;
            default:
                gameTime *= 2;
                clockCounter.showMessage(gameTime.ToString());
                nextTurn += clockTick;
                clockUI.SetActive(true);
                break;
        }

        Turn(true);
    }

    public static bool IsMouseOverUI()
    {
        //return EventSystem.current.IsPointerOverGameObject();
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            if(raycastResultList[i].gameObject.GetComponent<BlockMouse>() == null)
            {
                raycastResultList.RemoveAt(i);
                i--;
            }
        }
        return raycastResultList.Count > 0;
    }

    public void OnTileClick(int x, int y)
    {
        //Debug.Log(x + " " + y);//
        if(turnActive)
        {
            if(active)
            {
                passiveX = x;
                passiveY = y;
                passive = true;
            }
            else
            {
                if(onBoardEntities[x, y] && onBoardEntities[x, y].whatTeam() == turnSide)
                {
                    activeX = x;
                    activeY = y;
                    active = true;
                }
                
            }
        }
    }

    public void Turn (bool side)
    {
        if (nextTurn != null)
        {
            nextTurn(this, EventArgs.Empty);
        }
        turnSide = side;
        turnActive = true;
        if(side)
        {
            GameManager.Instance.ChangeGameState(GameState.PlayerTurn);
            if (TurnR != null)
            {
                TurnR(this, EventArgs.Empty);
            }

            for(int i = 0; i < blueTeam.Length; i++)
            {
                if (blueTeam[i])
                {
                    blueTeam[i].GetComponent<Unit>().giveMove();
                }
            }
            
            for(int i = 0; i < redTeam.Length; i++)
            {
                if (redTeam[i])                                                    // to make with event?
                {
                    while (redTeam[i].GetComponent<Unit>().howManyMoves() != 0)
                    {
                        redTeam[i].GetComponent<Unit>().takeMove();
                    }
                }
            }
        }
        else
        {
            GameManager.Instance.ChangeGameState(GameState.EnemyTurn);
            if (TurnB != null)
            {
                TurnB(this, EventArgs.Empty);
            }
            for (int i = 0; i < redTeam.Length; i++)
            {
                if (redTeam[i])
                {
                    redTeam[i].GetComponent<Unit>().giveMove();
                }
            }

            for (int i = 0; i < blueTeam.Length; i++)
            {
                if (blueTeam[i])                                                    // to delete in UI rework
                {
                    while (blueTeam[i].GetComponent<Unit>().howManyMoves() != 0)
                    {
                        blueTeam[i].GetComponent<Unit>().takeMove();
                    }
                }
            }
        }
        UpdateMovesCount();
    }

    public bool whatSide()
    {
        return turnSide;
    }   
    
    public void ChangeTurn()
    {
        turnActive = false;
        unitIsSelected = false;
        moveHighlightsOn = false;
        if (destroyHighlights != null)
        {
            destroyHighlights(this, EventArgs.Empty);
        }
        active = false;

        Turn(!turnSide);
    }

    public void UpdateMovesCount()  //to make the spawn in ui rewok
    {
        if (updateUnitsInfoList != null)
        {
            updateUnitsInfoList(this, EventArgs.Empty);
        }
    }

    Vector2 halfWallCheckPoint(Vector2 startPoint, Vector2 endPoint)
    {
        Vector2 checkPointShift = new Vector2(startPoint.x - endPoint.x, startPoint.y - endPoint.y);
        return endPoint + 0.5f * checkPointShift;
    }

    public bool WalkHighlightCheck(int x, int y, Vector2 wallCheckLineStart, int range, GridTools.TileCoordinates parentTile)
    {
        Vector2  wallCheckLineEnd = new Vector2(x, y + 3 * _height);
        bool blockedByWall = true;
        //Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.green, 100f);  //debug line
        if (!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask))
        {
            blockedByWall = false;
            Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.green, 100f);  //debug line
        }
        else//debug
        {
            Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 100f);  //debug line
        }

        if (!onBoardEntities[x, y] && !blockedByWall)
        {
            GameObject MoveCheck = Instantiate(MoveHighlight, new Vector3(x - y * 0.5f, y), Quaternion.identity);
            MoveHighlight MoveCheckHighlight = MoveCheck.GetComponent<MoveHighlight>();
            Queue<GridTools.TileCoordinates> movesQueue = new Queue<GridTools.TileCoordinates>();
            if(onBoardChecks[parentTile.x, parentTile.y].GetComponent<MoveHighlight>())
            {
                if (onBoardChecks[parentTile.x, parentTile.y].GetComponent<MoveHighlight>().getMovesQeue().Count != 0)
                {
                    GridTools.TileCoordinates[] parentsPath = onBoardChecks[parentTile.x, parentTile.y].GetComponent<MoveHighlight>().getMovesQeue().ToArray();
                    for(int i = 0; i < parentsPath.Length; i++)
                    {
                        movesQueue.Enqueue(parentsPath[i]);
                    }
                }
            }
            movesQueue.Enqueue(new GridTools.TileCoordinates(x, y));
            MoveCheckHighlight.setMovesQueue(movesQueue);
            onBoardChecks[x, y] = MoveCheck;
            MoveCheck.name = "MoveCheck";
            Highlight highlightScript = MoveCheck.GetComponent<Highlight>();
            highlightScript.setCoordinates(x, y);
            highlightScript.setRange(range + 1);
            return true;
        }
        return false;
    }

    public void HighlightPossibleMoves(int x, int y, int range)
    {
        Vector2 wallCheckLineStart, wallCheckLineEnd;
        
        List<TileCoordinates> fov = MidpointCircleAlgorithmScan(x, y, range, false);
        bool seekedTeam = !onBoardEntities[x, y].whatTeam();

        //highlight possible walk

        int moveRangeCount = 0;
        int moveRangeCountLimit = onBoardEntities[x, y].isCrouched() ? onBoardEntities[x, y].howManyMoves() - 1 : onBoardEntities[x, y].howManyMoves();
        Queue<GridTools.TileCoordinates> tilesQueue = new Queue<GridTools.TileCoordinates>();
        if (moveRangeCount < moveRangeCountLimit)
        {
            GameObject MoveCheck = new GameObject();
            onBoardChecks[x, y] = MoveCheck;
            MoveCheck.name = "MidCheck";
            tilesQueue.Enqueue(new GridTools.TileCoordinates(x, y));
        }
        while(moveRangeCount < moveRangeCountLimit)
        {
            Queue<GridTools.TileCoordinates> newTilesQueue = new Queue<GridTools.TileCoordinates>();
            while (tilesQueue.Count != 0)
            {
                GridTools.TileCoordinates currentTile = tilesQueue.Dequeue();
                wallCheckLineStart = new Vector2(currentTile.x, currentTile.y + 3 * _height);
                if (currentTile.x != 0 && !onBoardChecks[currentTile.x - 1, currentTile.y])
                {
                    if(WalkHighlightCheck(currentTile.x - 1, currentTile.y, wallCheckLineStart, moveRangeCount, currentTile))
                    {
                        newTilesQueue.Enqueue(new GridTools.TileCoordinates(currentTile.x - 1, currentTile.y));
                    }
                }
                if (currentTile.x != _width - 1 && !onBoardChecks[currentTile.x + 1, currentTile.y])
                {
                    if (WalkHighlightCheck(currentTile.x + 1, currentTile.y, wallCheckLineStart, moveRangeCount, currentTile))
                    {
                        newTilesQueue.Enqueue(new GridTools.TileCoordinates(currentTile.x + 1, currentTile.y));
                    }
                }
                if (currentTile.y != 0 && !onBoardChecks[currentTile.x, currentTile.y - 1])
                {
                    if (WalkHighlightCheck(currentTile.x, currentTile.y - 1, wallCheckLineStart, moveRangeCount, currentTile))
                    {
                        newTilesQueue.Enqueue(new GridTools.TileCoordinates(currentTile.x, currentTile.y - 1));
                    }
                }
                if (currentTile.y != _height - 1 && !onBoardChecks[currentTile.x, currentTile.y + 1])
                {
                    if (WalkHighlightCheck(currentTile.x, currentTile.y + 1, wallCheckLineStart, moveRangeCount, currentTile))
                    {
                        newTilesQueue.Enqueue(new GridTools.TileCoordinates(currentTile.x, currentTile.y + 1));
                    }
                }
            }
            tilesQueue = newTilesQueue;
            moveRangeCount++;
        }

        //highlight possible attacks

        wallCheckLineStart = new Vector2(x, y + 3 * _height);
        for (int i = 0; i < fov.Count; i++)
        {
            wallCheckLineEnd = new Vector2(fov[i].x, fov[i].y + 3 * _height);
            if (onBoardEntities[fov[i].x, fov[i].y] && onBoardEntities[fov[i].x, fov[i].y].whatTeam() == seekedTeam )
            {
                //Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                //Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line
                if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[fov[i].x, fov[i].y].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[fov[i].x, fov[i].y].isCrouched()))))
                {
                    GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(fov[i].x - fov[i].y * 0.5f, fov[i].y), Quaternion.identity);
                    onBoardChecks[fov[i].x, fov[i].y] = EnemyCheck;
                    EnemyCheck.name = "EnemyCheck";
                    Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                    highlightScript.setCoordinates(fov[i].x, fov[i].y);
                }
                // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                {
                    bool isNotTaken = false;
                    if (!FOVChecks[fov[i].x, fov[i].y])
                    {
                        if(onBoardChecks[fov[i].x, fov[i].y])
                        {
                            if (onBoardChecks[fov[i].x, fov[i].y].name == "EnemyCheck")
                            {
                                isNotTaken = true;
                            }
                        }
                        else
                        {
                            isNotTaken = true;
                        }
                    }
                    
                    if(isNotTaken)
                    {
                        GameObject FOVCheck = Instantiate(FOVHighlight, new Vector3(fov[i].x - fov[i].y * 0.5f, fov[i].y), Quaternion.identity);
                        FOVChecks[fov[i].x, fov[i].y] = FOVCheck;
                        FOVCheck.name = "FOVCheck";
                        Highlight highlightScript = FOVCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(fov[i].x, fov[i].y);
                    }
                }
            }
            else
            {
                // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                {
                    bool _isNotTaken = false;
                    if (!FOVChecks[fov[i].x, fov[i].y])
                    {
                        if (onBoardChecks[fov[i].x, fov[i].y])
                        {
                            if (onBoardChecks[fov[i].x, fov[i].y].name == "MoveCheck")
                            {
                                _isNotTaken = true;
                            }
                        }
                        else
                        {
                            _isNotTaken = true;
                        }
                    }

                    if (_isNotTaken)
                    {
                        GameObject FOVCheck = Instantiate(FOVHighlight, new Vector3(fov[i].x - fov[i].y * 0.5f, fov[i].y), Quaternion.identity);
                        FOVChecks[fov[i].x, fov[i].y] = FOVCheck;
                        FOVCheck.name = "FOVCheck";
                        Highlight highlightScript = FOVCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(fov[i].x, fov[i].y);
                    }
                }
            }
        }
    }

    void HighlightGadgetUse(int x, int y, int range, bool halfWallBlocked, bool centerIncluded) // n - range
    {
        Vector2 wallCheckLineStart, wallCheckLineEnd;
        wallCheckLineStart = new Vector2(x, y + 3 * _height);
        LayerMask wallMask = LayerMask.GetMask("Wall");
        LayerMask halfWallMask = LayerMask.GetMask("HalfWall");
        List<TileCoordinates> fov = MidpointCircleAlgorithmScan(x, y, range, centerIncluded);

        for (int i = 0; i < fov.Count; i++)
        {
            wallCheckLineEnd = new Vector2(fov[i].x, fov[i].y + 3 * _height);
            // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
            if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
            {
                GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(fov[i].x - (fov[i].y) * 0.5f, fov[i].y), Quaternion.identity);
                onBoardChecks[fov[i].x, fov[i].y] = GadgetCheck;
                GadgetCheck.name = "GadgetCheck";
                Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                highlightScript.setCoordinates(fov[i].x, fov[i].y);
            }
        }
    }

    public void GrenadeExplosion( int _x, int _y)
    {
        bool blockedByWall = true;
        Vector2 lineStart, lineEnd;
        lineStart = new Vector2(_x, _y + 3 * _height);
        LayerMask wallMask = LayerMask.GetMask("Wall");
        LayerMask halfWallMask = LayerMask.GetMask("HalfWall");

        if (onBoardEntities[_x, _y])
        {
            onBoardEntities[_x, _y].takeDamage(50);
        }

        if (_x != 0)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x - 1, _y + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                for(int i = 0; i < 5; i++)
                {
                    float randomY = UnityEngine.Random.Range(-0.5f, 0.5f);
                    float randomX = UnityEngine.Random.Range(-0.5f, 0.5f);
                    Instantiate(ShrapnelGroup, new Vector3(((float)_x + randomX) - 1 - ((float)_y + randomY) * 0.5f, ((float)_y + randomY)), Quaternion.identity);
                }
                if (onBoardEntities[_x - 1, _y])
                {
                    onBoardEntities[_x - 1, _y].takeDamage(50);
                }
            }
        }
        if (_x != _width - 1)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x + 1, _y + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                for (int i = 0; i < 5; i++)
                {
                    float randomY = UnityEngine.Random.Range(-0.5f, 0.5f);
                    float randomX = UnityEngine.Random.Range(-0.5f, 0.5f);
                    Instantiate(ShrapnelGroup, new Vector3(((float)_x + randomX) + 1 - ((float)_y + randomY) * 0.5f, ((float)_y + randomY)), Quaternion.identity);
                }
                if (onBoardEntities[_x + 1, _y])
                {
                    onBoardEntities[_x + 1, _y].takeDamage(50);
                }
            }
        }
        if (_y != 0)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x, _y - 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                for (int i = 0; i < 5; i++)
                {
                    float randomY = UnityEngine.Random.Range(-0.5f, 0.5f);
                    float randomX = UnityEngine.Random.Range(-0.5f, 0.5f);
                    Instantiate(ShrapnelGroup, new Vector3(((float)_x + randomX) - ((float)_y + randomY - 1f) * 0.5f, ((float)_y + randomY) - 1), Quaternion.identity);
                }
                if (onBoardEntities[_x, _y - 1])
                {
                        onBoardEntities[_x, _y - 1].takeDamage(50);
                }
            }

        }
        if (_y != _height - 1)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x, _y + 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                for (int i = 0; i < 5; i++)
                {
                    float randomY = UnityEngine.Random.Range(-0.5f, 0.5f);
                    float randomX = UnityEngine.Random.Range(-0.5f, 0.5f);
                    Instantiate(ShrapnelGroup, new Vector3(((float)_x + randomX) - ((float)_y + randomY + 1f) * 0.5f, ((float)_y + randomY) + 1), Quaternion.identity);
                }
                if (onBoardEntities[_x, _y + 1])
                {
                    onBoardEntities[_x, _y + 1].takeDamage(50);
                }
            }
        }

        if (_x != 0 && _y != 0)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x - 1, _y - 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                for (int i = 0; i < 5; i++)
                {
                    float randomY = UnityEngine.Random.Range(-0.5f, 0.5f);
                    float randomX = UnityEngine.Random.Range(-0.5f, 0.5f);
                    Instantiate(ShrapnelGroup, new Vector3(((float)_x + randomX) - 1 - ((float)_y + randomY - 1f) * 0.5f, ((float)_y + randomY) - 1), Quaternion.identity);
                }
                if (onBoardEntities[_x - 1, _y - 1])
                {
                    onBoardEntities[_x - 1, _y - 1].takeDamage(50);
                }
            }
        }

        if (_x != 0 && _y != _height - 1)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x - 1, _y + 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                for (int i = 0; i < 5; i++)
                {
                    float randomY = UnityEngine.Random.Range(-0.5f, 0.5f);
                    float randomX = UnityEngine.Random.Range(-0.5f, 0.5f);
                    Instantiate(ShrapnelGroup, new Vector3(((float)_x + randomX) - 1 - ((float)_y + randomY + 1f) * 0.5f, ((float)_y + randomY) + 1), Quaternion.identity);
                }
                if (onBoardEntities[_x - 1, _y + 1])
                {
                    onBoardEntities[_x - 1, _y + 1].takeDamage(50);
                }
            }
        }
        if (_x != _width - 1 && _y != 0)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x + 1, _y - 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                for (int i = 0; i < 5; i++)
                {
                    float randomY = UnityEngine.Random.Range(-0.5f, 0.5f);
                    float randomX = UnityEngine.Random.Range(-0.5f, 0.5f);
                    Instantiate(ShrapnelGroup, new Vector3(((float)_x + randomX) + 1 - ((float)_y + randomY - 1f) * 0.5f, ((float)_y + randomY) - 1), Quaternion.identity);
                }
                if (onBoardEntities[_x + 1, _y - 1])
                {
                    onBoardEntities[_x + 1, _y - 1].takeDamage(50);
                }
            }
        }

        if (_x != _width - 1 && _y != _height - 1)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x + 1, _y + 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                for (int i = 0; i < 5; i++)
                {
                    float randomY = UnityEngine.Random.Range(-0.5f, 0.5f);
                    float randomX = UnityEngine.Random.Range(-0.5f, 0.5f);
                    Instantiate(ShrapnelGroup, new Vector3(((float)_x + randomX) + 1 - ((float)_y + randomY + 1f) * 0.5f, ((float)_y + randomY) + 1), Quaternion.identity);
                }
                if (onBoardEntities[_x + 1, _y + 1])
                {
                    onBoardEntities[_x + 1, _y + 1].takeDamage(50);
                }
            }
        }
    }

    public void SmokeExplosion(int _x, int _y)
    {
        bool blockedByWall = true;
        Vector2 lineStart, lineEnd;
        lineStart = new Vector2(_x, _y + 3 * _height);
        LayerMask wallMask = LayerMask.GetMask("Wall");
        Vector3 _adjustment = new Vector3(0f, 0.455f, 0f);

        GameObject _Smoke = Instantiate(Smoke, new Vector3(_x - _y * 0.5f, _y) + _adjustment, Quaternion.identity);
        _Smoke.GetComponent<Smoke>().setCoordinates(_x, _y);
        Instantiate(SmokeHitbox, new Vector3(_x, _y + 3 * _height - 0.5f) + _adjustment, Quaternion.identity);

        if (_x != 0)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x - 1, _y + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                _Smoke = Instantiate(Smoke, new Vector3(_x - 1 - _y * 0.5f, _y) + _adjustment, Quaternion.identity);
                _Smoke.GetComponent<Smoke>().setCoordinates(_x - 1, _y);
                Instantiate(SmokeHitbox, new Vector3(_x - 1, _y + 3 * _height - 0.5f) + _adjustment, Quaternion.identity);
            }
        }
        if (_x != _width - 1)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x + 1, _y + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                _Smoke = Instantiate(Smoke, new Vector3(_x + 1 - _y * 0.5f, _y) + _adjustment, Quaternion.identity);
                _Smoke.GetComponent<Smoke>().setCoordinates(_x + 1, _y);
                Instantiate(SmokeHitbox, new Vector3(_x + 1, _y + 3 * _height - 0.5f) + _adjustment, Quaternion.identity);
            }
        }
        if (_y != 0)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x, _y - 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                _Smoke = Instantiate(Smoke, new Vector3(_x - (_y - 1) * 0.5f, _y - 1) + _adjustment, Quaternion.identity);
                _Smoke.GetComponent<Smoke>().setCoordinates(_x, _y - 1);
                Instantiate(SmokeHitbox, new Vector3(_x, _y - 1 + 3 * _height - 0.5f) + _adjustment, Quaternion.identity);
            }

        }
        if (_y != _height - 1)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x, _y + 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                _Smoke = Instantiate(Smoke, new Vector3(_x - (_y + 1) * 0.5f, _y + 1) + _adjustment, Quaternion.identity);
                _Smoke.GetComponent<Smoke>().setCoordinates(_x, _y + 1);
                Instantiate(SmokeHitbox, new Vector3(_x, _y + 1 + 3 * _height - 0.5f) + _adjustment, Quaternion.identity);
            }
        }

        if (_x != 0 && _y != 0)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x - 1, _y - 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                _Smoke = Instantiate(Smoke, new Vector3(_x - 1 - (_y - 1) * 0.5f, _y - 1) + _adjustment, Quaternion.identity);
                _Smoke.GetComponent<Smoke>().setCoordinates(_x - 1, _y - 1);
                Instantiate(SmokeHitbox, new Vector3(_x - 1, _y - 1 + 3 * _height - 0.5f) + _adjustment, Quaternion.identity);
            }
        }

        if (_x != 0 && _y != _height - 1)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x - 1, _y + 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                _Smoke = Instantiate(Smoke, new Vector3(_x - 1 - (_y + 1) * 0.5f, _y + 1) + _adjustment, Quaternion.identity);
                _Smoke.GetComponent<Smoke>().setCoordinates(_x - 1, _y + 1);
                Instantiate(SmokeHitbox, new Vector3(_x - 1, _y + 1 + 3 * _height - 0.5f) + _adjustment, Quaternion.identity);
            }
        }
        if (_x != _width - 1 && _y != 0)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x + 1, _y - 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                _Smoke = Instantiate(Smoke, new Vector3(_x + 1 - (_y - 1) * 0.5f, _y - 1) + _adjustment, Quaternion.identity);
                _Smoke.GetComponent<Smoke>().setCoordinates(_x + 1, _y - 1);
                Instantiate(SmokeHitbox, new Vector3(_x + 1, _y - 1 + 3 * _height - 0.5f) + _adjustment, Quaternion.identity);
            }
        }

        if (_x != _width - 1 && _y != _height - 1)
        {
            blockedByWall = true;
            lineEnd = new Vector2(_x + 1, _y + 1 + 3 * _height);
            Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask))
            {
                blockedByWall = false;
            }

            if (!blockedByWall)
            {
                _Smoke = Instantiate(Smoke, new Vector3(_x + 1 - (_y + 1) * 0.5f, _y + 1) + _adjustment, Quaternion.identity);
                _Smoke.GetComponent<Smoke>().setCoordinates(_x + 1, _y + 1);
                Instantiate(SmokeHitbox, new Vector3(_x + 1, _y + 1 + 3 * _height - 0.5f) + _adjustment, Quaternion.identity);
            }
        }
    }

    void Attack()
    {
        if(onBoardEntities[activeX, activeY].CanShoot())
        {
            onBoardEntities[activeX, activeY].ShootAt(passiveX - passiveY * 0.5f, passiveY + 1, passiveX, passiveY);
            UpdateMovesCount();
        }
    }

    void Move(int range)
    {
        onBoardEntities[activeX, activeY].Walk(passiveX, passiveY, onBoardChecks[passiveX, passiveY].GetComponent<MoveHighlight>());

        for (int i = 1; i <= range; i++)
        {
            onBoardEntities[activeX, activeY].takeMove();
        }

        onBoardEntities[passiveX, passiveY] = onBoardEntities[activeX, activeY];
        HideWall(passiveX, passiveY);
        onBoardEntities[activeX, activeY] = null;
        UnHideWall(activeX, activeY);
        UpdateMovesCount();

        activeX = passiveX;
        activeY = passiveY;
    }

    void UseGadget()
    {
        onBoardEntities[activeX, activeY].UseGadget(passiveX, passiveY);
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    void Update()
    {
        if(turnActive)
        {
            //active

            if(active && onBoardEntities[activeX, activeY])
            {
                unitIsSelected = true;
                selectedUnit = onBoardEntities[activeX, activeY];
                if (displayUnitInfoEvent != null)
                {
                    displayUnitInfoEvent(this, EventArgs.Empty);
                }
                UnitControlsUI.SetActive(true);
                if (!moveHighlightsOn)
                {
                    if (!selectedUnit.isGadgetActive())
                    {
                        HighlightPossibleMoves(activeX, activeY, onBoardEntities[activeX, activeY].whatRange());
                    }
                    else
                    {
                        HighlightForGadget(onBoardEntities[activeX, activeY].whatGadget());
                    }
                    moveHighlightsOn = true;
                }
                if(unitIsSelected && !cameraZoomed)
                {
                    cameraController.MoveCamera(activeX - activeY * 0.5f, activeY, onBoardEntities[activeX, activeY].whatRange());
                    cameraZoomed = true;
                }

                
                
                //passive

                if (passive)
                {
                    if(onBoardChecks[passiveX,passiveY] && onBoardEntities[activeX, activeY].CanMove())
                    {
                        if(onBoardChecks[passiveX, passiveY].name == "MoveCheck")
                        {
                            Move(onBoardChecks[passiveX, passiveY].GetComponent<Highlight>().getRange());
                            if (unitIsSelected && !cameraZoomed)
                            {
                                cameraController.MoveCamera(activeX - activeY * 0.5f, activeY, onBoardEntities[activeX, activeY].whatRange());
                                cameraZoomed = true;
                            }
                        }
                        else if (onBoardChecks[passiveX, passiveY].name == "EnemyCheck")
                        {
                            Attack();
                        }
                        else if (onBoardChecks[passiveX, passiveY].name == "GadgetCheck")
                        {
                            UseGadget();
                        }
                        else
                        {
                            active = false;
                        }
                    }
                    else
                    {
                        active = false;
                    }
                    passive = false;


                    if(destroyHighlights != null)
                    {
                        destroyHighlights(this, EventArgs.Empty);
                    }

                    moveHighlightsOn = false;

                }

            }
        else
            {
                active = false;
                UnitControlsUI.SetActive(false);
                unitIsSelected = false;
                if (!unitIsSelected && cameraZoomed)
                    {
                    cameraController.ResetCamera();
                    cameraZoomed = false;
                    }
            }
        }
            
    }
    
}

