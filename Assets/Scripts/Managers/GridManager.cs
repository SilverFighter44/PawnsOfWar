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

    [SerializeField] private Tile _tilePrefab;

    [SerializeField] private GameObject MoveHighlight, EnemyHighlight, FOVHighlight, GadgetHighlight, WallX, WallY, WindowFrameX, WindowFrameY, DoorFrameX, DoorFrameY, UnitPrefab, WallHitBox, WindowHitBox, DoorHitBox, UnitControlsUI, FlagB, FlagR, ShrapnelGroup, SmokeHitbox, Smoke;

    [SerializeField] private MovesCount B1Count, B2Count, B3Count, B4Count, R1Count, R2Count, R3Count, R4Count;

    [SerializeField] private CameraController cameraController;

    StartData.gameMode gameMode;

    private Spawn spawnB, spawnR;

    int[,] onBoardGadgets = new int[_width, _height];
    Unit[,] onBoardEntities = new Unit[_width, _height];
    GameObject[,] boardCheck = new GameObject[_width, _height];    // array of highlights (attack and move)
    Wall[,] BoardConnectionGridX = new Wall[_height, _width - 1];
    Wall[,] BoardConnectionGridY = new Wall[_height - 1, _width];

    [SerializeField] private GameObject pO1, pO2, pO3, pO4, eO1, eO2, eO3, eO4;

    int activeX = 0, activeY = 0, passiveX = 0, passiveY = 0, teamB = 4, teamR = 4, gameTime = 15, flagB = 4, flagR = 4;
    bool active = false, passive = false, turnActive = false, moveHighlightsOn = false, unitIsSelected = false, cameraZoomed = false;
    Unit selectedUnit;

    [SerializeField] private TextWriter UnitControlsHP, UnitControlsAmmo, UnitControlsNumber, UnitControlsMoves;
    [SerializeField] private UI_ClassSymbol UnitControlsSymbol;

    [SerializeField] private bool turnSide;

    public event EventHandler destroyHighlights;

    public event EventHandler nextTurn;

    public event EventHandler TurnB;

    public event EventHandler TurnR;

    public struct Spawn
    {
        public Vector2 tile; //bottom right position
    };

    public void eliminateUnit(bool team)
    {
        if(team)
        {
            teamB--;
        }
        else
        {
            teamR--;
        }
        if(teamB <= 0)
        {
            GameManager.Instance.winGame(false);
        }
        if (teamR <= 0)
        {
            GameManager.Instance.winGame(true);
        }
    }

    void clockTick(object sender, EventArgs e)
    {
        // clock
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

        //flag checks
        if ((gameMode == StartData.gameMode.defenceB || gameMode == StartData.gameMode.defence) && GameManager.Instance.getGameState() == GameState.PlayerTurn)
        {
            if(onBoardEntities[(int)spawnB.tile.x,(int)spawnB.tile.y])
            {
                if(!onBoardEntities[(int)spawnB.tile.x,(int)spawnB.tile.y].whatTeam())
                {
                    flagB--;
                }
            }
            if (onBoardEntities[(int)spawnB.tile.x - 1,(int)spawnB.tile.y])
            {
                if (!onBoardEntities[(int)spawnB.tile.x - 1,(int)spawnB.tile.y].whatTeam())
                {
                    flagB--;
                }
            }
            if (onBoardEntities[(int)spawnB.tile.x,(int)spawnB.tile.y + 1])
            {
                if (!onBoardEntities[(int)spawnB.tile.x,(int)spawnB.tile.y + 1].whatTeam())
                {
                    flagB--;
                }
            }
            if (onBoardEntities[(int)spawnB.tile.x - 1,(int)spawnB.tile.y + 1])
            {
                if (!onBoardEntities[(int)spawnB.tile.x - 1,(int)spawnB.tile.y + 1].whatTeam())
                {
                    flagB--;
                }
            }
            if (flagB <= 0)
            {
                GameManager.Instance.winGame(false);
            }
        }
        else if ((gameMode == StartData.gameMode.defenceR || gameMode == StartData.gameMode.defence) && GameManager.Instance.getGameState() == GameState.EnemyTurn)
        {
            if (onBoardEntities[(int)spawnR.tile.x,(int)spawnR.tile.y])
            {
                if (onBoardEntities[(int)spawnR.tile.x,(int)spawnR.tile.y].whatTeam())
                {
                    flagR--;
                }
            }
            if (onBoardEntities[(int)spawnR.tile.x - 1,(int)spawnR.tile.y])
            {
                if (onBoardEntities[(int)spawnR.tile.x - 1,(int)spawnR.tile.y].whatTeam())
                {
                    flagR--;
                }
            }
            if (onBoardEntities[(int)spawnR.tile.x,(int)spawnR.tile.y + 1])
            {
                if (onBoardEntities[(int)spawnR.tile.x,(int)spawnR.tile.y + 1].whatTeam())
                {
                    flagR--;
                }
            }
            if (onBoardEntities[(int)spawnR.tile.x - 1,(int)spawnR.tile.y + 1])
            {
                if (onBoardEntities[(int)spawnR.tile.x - 1,(int)spawnR.tile.y + 1].whatTeam())
                {
                    flagR--;
                }
            }
            if (flagR <= 0)
            {
                GameManager.Instance.winGame(true);
            }
        }
    }

    void ClearBoardCheck(object sender, EventArgs e)
    {
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                if (boardCheck[i, j])
                {
                    boardCheck[i, j] = null;
                }
            }
        }
    }

    void Awake()
    {
        Instance = this;
        GridManager.Instance.destroyHighlights += ClearBoardCheck;
    }

    public static float absDif(float a, float b)
    {
        if (a > b)
            return a - b;
        return b - a;
    }

    public Unit getUnitFromTile(int x, int y)
    {
        return onBoardEntities[x, y];
    }

    public void ReloadButtton()
    {
        selectedUnit.reload();
        UpdateMovesCount();
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
            HighlightPossibleMoves();
            HighlightPossibleAttacks(activeX, activeY, onBoardEntities[activeX, activeY].whatRange());
        }
        moveHighlightsOn = true;
        UpdateMovesCount();
    }

    public void CrouchButtton()
    {
        selectedUnit.crouch();
        ResetHighlights();
    }

    public void Gadget1Buttton()
    {
        selectedUnit.useGadget1();
        ResetHighlights();
    }

    public void Gadget2Buttton()
    {
        selectedUnit.useGadget2();
        ResetHighlights();
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
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x - y * 0.5f, y), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Init(isOffset, x, y);

            }
        }

        //_width = 16, _height = 8

        int[,] _BoardConnectionGridX = new int[8, 15]{
                { 0, 0, 1, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 0, 0, 3, 0, 0, 3, 3, 0, 3, 0, 3, 0, 0, 3, 0},
                { 0, 0, 0, 0, 0, 0, 2, 0, 3, 0, 1, 0, 0, 3, 0},
                { 0, 3, 0, 2, 0, 0, 0, 0, 0, 0, 2, 0, 3, 0, 0},
                { 0, 1, 0, 3, 0, 0, 3, 2, 0, 0, 0, 0, 0, 0, 0},
                { 0, 0, 0, 0, 0, 0, 3, 3, 0, 0, 0, 0, 0, 0, 0},
                { 0, 0, 3, 0, 3, 0, 2, 0, 1, 0, 3, 0, 3, 0, 0},
                { 0, 0, 3, 0, 1, 0, 1, 0, 3, 0, 1, 0, 1, 0, 0}};



        int[,] _BoardConnectionGridY = new int[7, 16]{
                { 0, 0, 0, 0, 0, 0, 0, 1, 3, 0, 0, 3, 1, 3, 0, 0},
                { 3, 1, 3, 3, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 0, 0, 3, 1, 0, 0, 0, 3, 1, 0, 0, 0, 0, 1, 0, 0},
                { 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 3, 3, 0, 0, 0},
                { 0, 0, 3, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                { 0, 0, 0, 1, 3, 0, 0, 0, 2, 0, 0, 2, 3, 0, 0, 0},
                { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}};

        spawnB.tile = new Vector2 (1, 3);                             // To improve when reworking map edit
        spawnR.tile = new Vector2 (15, 3);

        for (int x = 0; x < _height; x++)
        {
            for (int y = 0; y < _width - 1; y++)
            {
                if(_BoardConnectionGridX[x, y] == 3)
                {
                    GameObject spawnedWall = Instantiate(WallY, new Vector3(y - x * 0.5f + 0.5f , x + 0.5f), Quaternion.identity);
                    BoardConnectionGridX[x, y] = spawnedWall.GetComponent<Wall>();
                    BoardConnectionGridX[x, y].setLayer(x, y, true);
                }
                else if (_BoardConnectionGridX[x, y] == 2)
                {
                    GameObject spawnedWall = Instantiate(WindowFrameY, new Vector3(y - x * 0.5f + 0.5f, x + 0.5f), Quaternion.identity);
                    BoardConnectionGridX[x, y] = spawnedWall.GetComponent<Wall>();
                    BoardConnectionGridX[x, y].setLayer(x, y, true);
                }
                else if (_BoardConnectionGridX[x, y] == 1)
                {
                    GameObject spawnedWall = Instantiate(DoorFrameY, new Vector3(y - x * 0.5f + 0.5f, x + 0.5f), Quaternion.identity);
                    BoardConnectionGridX[x, y] = spawnedWall.GetComponent<Wall>();
                    BoardConnectionGridX[x, y].setLayer(x, y, true);
                }
            }
        }

        for (int x = 0; x < _height - 1; x++)
        {
            for (int y = 0; y < _width; y++)
            {
                if (_BoardConnectionGridY[x, y] == 3)
                {
                    GameObject spawnedWall = Instantiate(WallX, new Vector3(y - x * 0.5f - 0.25f, x + 1), Quaternion.identity);
                    BoardConnectionGridY[x, y] = spawnedWall.GetComponent<Wall>();
                    BoardConnectionGridY[x, y].setLayer(x, y, false);
                }
                else if (_BoardConnectionGridY[x, y] == 2)
                {
                    GameObject spawnedWall = Instantiate(WindowFrameX, new Vector3(y - x * 0.5f - 0.25f, x + 1), Quaternion.identity);
                    BoardConnectionGridY[x, y] = spawnedWall.GetComponent<Wall>();
                    BoardConnectionGridY[x, y].setLayer(x, y, false);
                }
                else if (_BoardConnectionGridY[x, y] == 1)
                {
                    GameObject spawnedWall =  Instantiate(DoorFrameX, new Vector3(y - x * 0.5f - 0.25f, x + 1), Quaternion.identity);
                    BoardConnectionGridY[x, y] = spawnedWall.GetComponent<Wall>();
                    BoardConnectionGridY[x, y].setLayer(x, y, false);
                }
            }
        }

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 15; y++)
            {
                if (BoardConnectionGridX[x, y])
                {
                    switch (BoardConnectionGridX[x, y].wallType)
                    {
                        case 3:
                            Instantiate(WallHitBox, new Vector3(y + 0.5f, x + 3 * _height), Quaternion.identity);
                            break;
                        case 2:
                            Instantiate(WindowHitBox, new Vector3(y + 0.5f, x + 3 * _height), Quaternion.identity);
                            break;
                        case 1:
                            Instantiate(DoorHitBox, new Vector3(y + 0.5f, x + 3 * _height), Quaternion.identity);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        for (int x = 0; x < 7; x++)
        {
            for (int y = 0; y < 16; y++)
            {
                if (BoardConnectionGridY[x, y])
                {
                    switch (BoardConnectionGridY[x, y].wallType)
                    {
                        case 3:
                            Instantiate(WallHitBox, new Vector3(y, x + 0.5f + 3 * _height), Quaternion.Euler(0, 0, 90));
                            break;
                        case 2:
                        Instantiate(WindowHitBox, new Vector3(y, x + 0.5f + 3 * _height), Quaternion.Euler(0, 0, 90));
                            break;
                        case 1:
                        Instantiate(DoorHitBox, new Vector3(y, x + 0.5f + 3 * _height), Quaternion.Euler(0, 0, 90));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        // _cam.transform.position = new Vector3(((float)_width - ((float)_height - 1) * 0.5f) / 2 - 0.5f, (float)_height / 2 - 0.5f , -10);   //camera
        cameraController.SetDefaultPosition(((float)_width - ((float)_height - 1) * 0.5f) / 2 - 0.5f, (float)_height / 2 - 0.5f);

        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                onBoardEntities[x,y] = null;
                onBoardGadgets[x, y] = 0;
            }
        }


        GameManager.Instance.ChangeGameState(GameState.SetLevel);
    }

    public void HideWall(int x, int y)
    {
        if (onBoardEntities[x, y] || boardCheck[x, y])
        {
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
    }

    public void UnHideWall(int x, int y)
    {
        if ((!onBoardEntities[x, y] && !boardCheck[x, y]) && onBoardGadgets[x, y] <= 0)
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
        if ((!onBoardEntities[x, y] && !boardCheck[x, y]) && onBoardGadgets[x, y] <= 0)
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
        StartData.GameSettingsData data = StartData.Instance.getData();
        gameMode = data.GameMode;
        pO1 = Instantiate(UnitPrefab, new Vector3(-1 , 4), Quaternion.identity);
        pO1.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        onBoardEntities[1, 4] = pO1.GetComponent<Unit>();
        onBoardEntities[1, 4].SetPosition(4, 1);
        onBoardEntities[1, 4].SetData(data.BlueTeam[0]);
        HideWall(1, 4);

        pO2 = Instantiate(UnitPrefab, new Vector3(-2, 4), Quaternion.identity);
        pO2.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        onBoardEntities[0, 4] = pO2.GetComponent<Unit>();
        onBoardEntities[0, 4].SetPosition(4, 0);
        onBoardEntities[0, 4].SetData(data.BlueTeam[1]);
        HideWall(0, 4);

        pO3 = Instantiate(UnitPrefab, new Vector3(-0.5f , 3), Quaternion.identity);
        pO3.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        onBoardEntities[1, 3] = pO3.GetComponent<Unit>();
        onBoardEntities[1, 3].SetPosition(3, 1);
        onBoardEntities[1, 3].SetData(data.BlueTeam[2]);
        HideWall(1, 3);

        pO4 = Instantiate(UnitPrefab, new Vector3(-1.5f, 3), Quaternion.identity);
        pO4.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        onBoardEntities[0, 3] = pO4.GetComponent<Unit>();
        onBoardEntities[0, 3].SetPosition(3, 0);
        onBoardEntities[0, 3].SetData(data.BlueTeam[3]);
        HideWall(0, 3);

        if(gameMode == StartData.gameMode.defence || gameMode == StartData.gameMode.defenceB)
        {
            FlagB = Instantiate(FlagB, new Vector3((float)spawnB.tile.x - 0.5f * (float)spawnB.tile.y - 0.75f, (float)spawnB.tile.y + 0.5f), Quaternion.identity);
            FlagB.GetComponent<Flag>().setLayer((int)spawnB.tile.y);
        }


        eO1 = Instantiate(UnitPrefab, new Vector3(_width - 4, 4), Quaternion.identity);
        eO1.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        onBoardEntities[_width - 2, 4] = eO1.GetComponent<Unit>();
        onBoardEntities[_width - 2, 4].SetPosition(4, _width - 2);
        onBoardEntities[_width - 2, 4].SetData(data.RedTeam[0]);
        HideWall(_width - 2, 4);

        eO2 = Instantiate(UnitPrefab, new Vector3(_width - 3, 4), Quaternion.identity);
        eO2.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        onBoardEntities[_width - 1, 4] = eO2.GetComponent<Unit>();
        onBoardEntities[_width - 1, 4].SetPosition(4, _width - 1);
        onBoardEntities[_width - 1, 4].SetData(data.RedTeam[1]);
        HideWall(_width - 1, 4);

 
        eO3 = Instantiate(UnitPrefab, new Vector3(_width - 3.5f, 3), Quaternion.identity);
        eO3.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        onBoardEntities[_width - 2, 3] = eO3.GetComponent<Unit>();
        onBoardEntities[_width - 2, 3].SetPosition(3, _width - 2);
        onBoardEntities[_width - 2, 3].SetData(data.RedTeam[2]);
        HideWall(_width - 2, 3);

        eO4 = Instantiate(UnitPrefab, new Vector3(_width - 2.5f, 3), Quaternion.identity);
        eO4.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        onBoardEntities[_width - 1, 3] = eO4.GetComponent<Unit>();
        onBoardEntities[_width - 1, 3].SetPosition(3, _width - 1);
        onBoardEntities[_width - 1, 3].SetData(data.RedTeam[3]);
        HideWall(_width - 1, 3);

        if (gameMode == StartData.gameMode.defence || gameMode == StartData.gameMode.defenceR)
        {
            FlagR = Instantiate(FlagR, new Vector3(spawnR.tile.x - 0.5f * spawnR.tile.y - 0.75f, spawnR.tile.y + 0.5f), Quaternion.identity);
            FlagR.GetComponent<Flag>().setLayer((int)spawnR.tile.y);
        }

        if (gameMode == StartData.gameMode.defenceR)
        {
            TurnB += clockTick;
        }
        else if (gameMode == StartData.gameMode.defenceB)
        {
            TurnR += clockTick;
        }
        else if (gameMode == StartData.gameMode.defence)
        {
            nextTurn += clockTick;
        }

        Turn(true);
    }

    public bool IsMouseOverUI()
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
            if (pO1)
            {
                pO1.GetComponent<Unit>().giveMove();
            }
            if (pO2)
            {
                pO2.GetComponent<Unit>().giveMove();
            }
            if (pO3)
            {
                pO3.GetComponent<Unit>().giveMove();
            }
            if (pO4)
            {
                pO4.GetComponent<Unit>().giveMove();
            }
            if (eO1)                                                    // to delete in UI rework
            {
                while (eO1.GetComponent<Unit>().howManyMoves() != 0)
                {
                    eO1.GetComponent<Unit>().takeMove();
                }
            }
            if (eO2)                                                    // to delete in UI rework
            {
                while (eO2.GetComponent<Unit>().howManyMoves() != 0)
                {
                    eO2.GetComponent<Unit>().takeMove();
                }
            }
            if (eO3)                                                    // to delete in UI rework
            {
                while (eO3.GetComponent<Unit>().howManyMoves() != 0)
                {
                    eO3.GetComponent<Unit>().takeMove();
                }
            }
            if (eO4)                                                    // to delete in UI rework
            {
                while (eO4.GetComponent<Unit>().howManyMoves() != 0)
                {
                    eO4.GetComponent<Unit>().takeMove();
                }
            }
            UpdateMovesCount();

        }
        else
        {
            GameManager.Instance.ChangeGameState(GameState.EnemyTurn);
            if (TurnB != null)
            {
                TurnB(this, EventArgs.Empty);
            }
            if (eO1)
            {
                eO1.GetComponent<Unit>().giveMove();
            }
            if (eO2)
            {
                eO2.GetComponent<Unit>().giveMove();
            }
            if (eO3)
            {
                eO3.GetComponent<Unit>().giveMove();
            }
            if (eO4)
            {
                eO4.GetComponent<Unit>().giveMove();
            }
            if (pO1)                                                    // to delete in UI rework
            {
                while (pO1.GetComponent<Unit>().howManyMoves() != 0)
                {
                    pO1.GetComponent<Unit>().takeMove();
                }
            }
            if (pO2)                                                    // to delete in UI rework
            {
                while (pO2.GetComponent<Unit>().howManyMoves() != 0)
                {
                    pO2.GetComponent<Unit>().takeMove();
                }
            }
            if (pO3)                                                    // to delete in UI rework
            {
                while (pO3.GetComponent<Unit>().howManyMoves() != 0)
                {
                    pO3.GetComponent<Unit>().takeMove();
                }
            }
            if (pO4)                                                    // to delete in UI rework
            {
                while (pO4.GetComponent<Unit>().howManyMoves() != 0)
                {
                    pO4.GetComponent<Unit>().takeMove();
                }
            }
            UpdateMovesCount();
        }
        
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

    public void UpdateMovesCount()
    {
        if (pO1)
        {
            B1Count.updateMovesCount(pO1.GetComponent<Unit>().howManyMoves());
        }
        if (pO2)
        {
            B2Count.updateMovesCount(pO2.GetComponent<Unit>().howManyMoves());
        }
        if (pO3)
        {
            B3Count.updateMovesCount(pO3.GetComponent<Unit>().howManyMoves());
        }
        if (pO4)
        {
            B4Count.updateMovesCount(pO4.GetComponent<Unit>().howManyMoves());
        }
        if (eO1)
        {
            R1Count.updateMovesCount(eO1.GetComponent<Unit>().howManyMoves());
        }
        if (eO2)
        {
            R2Count.updateMovesCount(eO2.GetComponent<Unit>().howManyMoves());
        }
        if (eO3)
        {
            R3Count.updateMovesCount(eO3.GetComponent<Unit>().howManyMoves());
        }
        if (eO4)
        {
            R4Count.updateMovesCount(eO4.GetComponent<Unit>().howManyMoves());
        }
    }

    public void HighlightPossibleMoves()
    {
        bool blockedByWall = true; // = true // to delete
        Vector2 lineStart, lineEnd;
        lineStart = new Vector2( activeX, activeY + 3 * _height);
        LayerMask wallMask = LayerMask.GetMask("Wall");
        LayerMask halfWallMask = LayerMask.GetMask("HalfWall");

        if (activeX != 0)
        {
            blockedByWall = true;
            lineEnd = new Vector2(activeX - 1, activeY + 3 * _height);
            //Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!onBoardEntities[activeX - 1, activeY] && !blockedByWall)
            {
                GameObject MoveCheck = Instantiate(MoveHighlight, new Vector3(activeX - 1 - activeY * 0.5f, activeY), Quaternion.identity);
                boardCheck[activeX - 1, activeY] = MoveCheck;
                MoveCheck.name = "MoveCheck";
                Highlight highlightScript = MoveCheck.GetComponent<Highlight>();
                highlightScript.setCoordinates(activeX - 1, activeY);
            }
        }
        if (activeX != _width - 1)
        {
            blockedByWall = true;
            lineEnd = new Vector2(activeX + 1, activeY + 3 * _height);
            //Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!onBoardEntities[activeX + 1, activeY] && !blockedByWall)
            {
                GameObject MoveCheck = Instantiate(MoveHighlight, new Vector3(activeX + 1 - activeY * 0.5f, activeY), Quaternion.identity);
                boardCheck[activeX + 1, activeY] = MoveCheck;
                MoveCheck.name = "MoveCheck";
                Highlight highlightScript = MoveCheck.GetComponent<Highlight>();
                highlightScript.setCoordinates(activeX + 1, activeY);
            }
        }
        if (activeY != 0)
        {
            blockedByWall = true;
            lineEnd = new Vector2(activeX, activeY - 1 + 3 * _height);
            //Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!onBoardEntities[activeX, activeY - 1] && !blockedByWall)
            {
                GameObject MoveCheck = Instantiate(MoveHighlight, new Vector3(activeX - (activeY - 1) * 0.5f, activeY - 1), Quaternion.identity);
                boardCheck[activeX, activeY - 1] = MoveCheck;
                MoveCheck.name = "MoveCheck";
                Highlight highlightScript = MoveCheck.GetComponent<Highlight>();
                highlightScript.setCoordinates(activeX, activeY - 1);
            }

        }
        if (activeY != _height - 1)
        {
            blockedByWall = true;
            lineEnd = new Vector2(activeX, activeY + 1 + 3 * _height);
            //Debug.DrawLine(lineStart, lineEnd, Color.green, 10f);  //debug line
            if (!Physics2D.Linecast(lineStart, lineEnd, wallMask) && !Physics2D.Linecast(lineStart, lineEnd, halfWallMask))
            {
                blockedByWall = false;
            }

            if (!onBoardEntities[activeX, activeY + 1] && !blockedByWall)
            {
                GameObject MoveCheck = Instantiate(MoveHighlight, new Vector3(activeX - (activeY + 1) * 0.5f, activeY + 1), Quaternion.identity);
                boardCheck[activeX, activeY + 1] = MoveCheck;
                MoveCheck.name = "MoveCheck";
                Highlight highlightScript = MoveCheck.GetComponent<Highlight>();
                highlightScript.setCoordinates(activeX, activeY + 1);
            }
        }
    }

    Vector2 halfWallCheckPoint(Vector2 startPoint, Vector2 endPoint)
    {
        Vector2 checkPointShift = new Vector2(startPoint.x -endPoint.x, startPoint.y - endPoint.y);
        return endPoint + 0.5f * checkPointShift;
    }

    void HighlightPossibleAttacks(int x, int y, int n) // n - range
    {
        Vector2 wallCheckLineStart, wallCheckLineEnd;
        wallCheckLineStart = new Vector2(x, y + 3 * _height);
        LayerMask wallMask = LayerMask.GetMask("Wall");
        LayerMask halfWallMask = LayerMask.GetMask("HalfWall");
        LayerMask smokeMask = LayerMask.GetMask("Smoke");


        //          cross check
        for (int i = 0; i < n; i++)
        {
            if (i + y - n >= 0 && i + y - n < _height && !boardCheck[x, i + y - n])      // check down
            {
                wallCheckLineEnd = new Vector2(x, i + y - n + 3 * _height);
                if (onBoardEntities[x, i + y - n] && onBoardEntities[x, i + y - n].whatTeam() != turnSide)
                {
                    
                     //Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                     Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x, i + y - n].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart ,wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x, i + y - n].isCrouched()))))
                    {
                        GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x - (i + y - n) * 0.5f, i + y - n), Quaternion.identity);
                        boardCheck[x, i + y - n] = EnemyCheck;
                        EnemyCheck.name = "EnemyCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x, i + y - n);
                    }
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - (i + y - n) * 0.5f, i + y - n), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x, i + y - n);
                    }
                }
                else
                {
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - (i + y - n) * 0.5f, i + y - n), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x, i + y - n);
                    }
                }
            }

            if (i + 1 + y >= 0 && i + 1 + y < _height && !boardCheck[x, i + 1 + y])      // check up
            {
                wallCheckLineEnd = new Vector2(x, i + 1 + y + 3 * _height);
                if (onBoardEntities[x, i + 1 + y] && onBoardEntities[x, i + 1 + y].whatTeam() != turnSide)
                {
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                     Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x, i + 1 + y].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x, i + 1 + y].isCrouched()))))
                    {
                        GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x - (i + 1 + y) * 0.5f, i + 1 + y), Quaternion.identity);
                        boardCheck[x, i + 1 + y] = EnemyCheck;
                        EnemyCheck.name = "EnemyCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x, i + 1 + y);
                    }
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - (i + 1 + y) * 0.5f, i + 1 + y), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x, i + 1 + y);
                    }
                }
                else
                {
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - (i + 1 + y) * 0.5f, i + 1 + y), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x, i + 1 + y);
                    }
                }
            }

            if (i + x - n >= 0 && i + x - n < _width && !boardCheck[i + x - n, y])       // check left
            {
                wallCheckLineEnd = new Vector2(i + x - n, y + (_height + 16));
                if (onBoardEntities[i + x - n, y] && onBoardEntities[i + x - n, y].whatTeam() != turnSide)
                {
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                     Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[i + x - n, y].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[i + x - n, y].isCrouched()))))
                    {
                        GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(i + x - n - y * 0.5f, y), Quaternion.identity);
                        boardCheck[i + x - n, y] = EnemyCheck;
                        EnemyCheck.name = "EnemyCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(i + x - n, y);
                    }
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(i + x - n - y * 0.5f, y), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(i + x - n, y);
                    }
                }
                else
                {
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(i + x - n - y * 0.5f, y), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(i + x - n, y);
                    }
                }
            }

            if (i + x + 1 >= 0 && i + x + 1 < _width && !boardCheck[i + x + 1, y])       // check right
            {
                wallCheckLineEnd = new Vector2(i + x + 1, y + (_height + 16));
                if (onBoardEntities[i + x + 1, y] && onBoardEntities[i + x + 1, y].whatTeam() != turnSide)
                {
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                     Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[i + x + 1, y].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[i + x + 1, y].isCrouched()))))
                    {
                        GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(i + x + 1 - y * 0.5f, y), Quaternion.identity);
                        boardCheck[i + x + 1, y] = EnemyCheck;
                        EnemyCheck.name = "EnemyCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(i + x + 1, y);
                    }
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(i + x + 1 - y * 0.5f, y), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(i + x + 1, y);
                    }
                }
                else
                {
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(i + x + 1 - y * 0.5f, y), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(i + x + 1, y);
                    }
                }
            }

        }



        //          circle algorithm
        int m = y + n;
        for (int i = 1; i <= m - y; i++)
        {
            float pktE = (float)(Math.Sqrt(i * i + (m - y) * (m - y))), pktSE = (float)(Math.Sqrt(i * i + (m - y - 1) * (m - y - 1)));
            float ra = absDif(pktE, n), rb = absDif(pktSE, n);
            if (ra > rb)
            {
                m--;
            }

            // if conditions: up -> (y + j < _height); down -> (y + j >= 0); left -> (x + i >= 0); right -> (x + i < _width)

            for (int j = 1 + i; j <= m - y; j++)            // up, right, mid
            {
                if (y + j < _height && x + i < _width)
                {
                    wallCheckLineEnd = new Vector2(x + i, y + j + 3 * _height);
                    if (onBoardEntities[x + i, y + j] && onBoardEntities[x + i, y + j].whatTeam() != turnSide)
                    {

                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                         Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x + i, y + j].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x + i, y + j].isCrouched()))))
                        {
                            GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x + i - (y + j) * 0.5f, y + j), Quaternion.identity);
                            boardCheck[x + i, y + j] = EnemyCheck;
                            EnemyCheck.name = "EnemyCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + i, y + j);
                        }
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + i - (y + j) * 0.5f, y + j), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + i, y + j);
                        }
                    }
                    else
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + i - (y + j) * 0.5f, y + j), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + i, y + j);
                        }
                    }
                }


                if (y - j >= 0 && x + i < _width)            // down, right, mid
                {
                    wallCheckLineEnd = new Vector2(x + i, y - j + 3 * _height);
                    if (onBoardEntities[x + i, y - j] && onBoardEntities[x + i, y - j].whatTeam() != turnSide)
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                         Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x + i, y - j].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x + i, y - j].isCrouched()))))
                        {
                            GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x + i - (y - j) * 0.5f, y - j), Quaternion.identity);
                            boardCheck[x + i, y - j] = EnemyCheck;
                            EnemyCheck.name = "EnemyCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + i, y - j);
                        }
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + i - (y - j) * 0.5f, y - j), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + i, y - j);
                        }
                    }
                    else
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + i - (y - j) * 0.5f, y - j), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + i, y - j);
                        }
                    }
                }

                if (y + j < _height && x - i >= 0)        // up, left, mid
                {
                    wallCheckLineEnd = new Vector2(x - i, y + j + 3 * _height);
                    if (onBoardEntities[x - i, y + j] && onBoardEntities[x - i, y + j].whatTeam() != turnSide)
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                         Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x - i, y + j].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x - i, y + j].isCrouched()))))
                        {
                            GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x - i - (y + j) * 0.5f, y + j), Quaternion.identity);
                            boardCheck[x - i, y + j] = EnemyCheck;
                            EnemyCheck.name = "EnemyCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - i, y + j);
                        }
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - i - (y + j) * 0.5f, y + j), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - i, y + j);
                        }
                    }
                    else
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - i - (y + j) * 0.5f, y + j), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - i, y + j);
                        }
                    }
                }


                if (y - j >= 0 && x - i >= 0)            // down, left, mid
                {
                    wallCheckLineEnd = new Vector2(x - i, y - j + 3 * _height);
                    if (onBoardEntities[x - i, y - j] && onBoardEntities[x - i, y - j].whatTeam() != turnSide)
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                         Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x - i, y - j].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x - i, y - j].isCrouched()))))
                        {
                            GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x - i - (y - j) * 0.5f, y - j), Quaternion.identity);
                            boardCheck[x - i, y - j] = EnemyCheck;
                            EnemyCheck.name = "EnemyCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - i, y - j);
                        }
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - i - (y - j) * 0.5f, y - j), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - i, y - j);
                        }
                    }
                    else
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - i - (y - j) * 0.5f, y - j), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - i, y - j);
                        }
                    }
                }


                if (y - i >= 0 && x - j >= 0)            // down, left, side
                {
                    wallCheckLineEnd = new Vector2(x - j, y - i + 3 * _height);
                    if (onBoardEntities[x - j, y - i] && onBoardEntities[x - j, y - i].whatTeam() != turnSide)
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                         Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x - j, y - i].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x - j, y - i].isCrouched()))))
                        {
                            GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x - j - (y - i) * 0.5f, y - i), Quaternion.identity);
                            boardCheck[x - j, y - i] = EnemyCheck;
                            EnemyCheck.name = "EnemyCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - j, y - i);
                        }
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - j - (y - i) * 0.5f, y - i), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - j, y - i);
                        }
                    }
                    else
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - j - (y - i) * 0.5f, y - i), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - j, y - i);
                        }
                    }
                }


                if (y - i >= 0 && x + j < _width)            // down, right, side
                {
                    wallCheckLineEnd = new Vector2(x + j, y - i + 3 * _height);
                    if (onBoardEntities[x + j, y - i] && onBoardEntities[x + j, y - i].whatTeam() != turnSide)
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                         Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x + j, y - i].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x + j, y - i].isCrouched()))))
                        {
                            GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x + j - (y - i) * 0.5f, y - i), Quaternion.identity);
                            boardCheck[x + j, y - i] = EnemyCheck;
                            EnemyCheck.name = "EnemyCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + j, y - i);
                        }
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + j - (y - i) * 0.5f, y - i), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + j, y - i);
                        }
                    }
                    else
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + j - (y - i) * 0.5f, y - i), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + j, y - i);
                        }
                    }
                }


                if (y + i < _height && x - j >= 0)            // up, left, side
                {
                    wallCheckLineEnd = new Vector2(x - j, y + i + 3 * _height);
                    if (onBoardEntities[x - j, y + i] && onBoardEntities[x - j, y + i].whatTeam() != turnSide)
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                         Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x - j, y + i].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x - j, y + i].isCrouched()))))
                        {
                            GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x - j - (y + i) * 0.5f, y + i), Quaternion.identity);
                            boardCheck[x - j, y + i] = EnemyCheck;
                            EnemyCheck.name = "EnemyCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - j, y + i);
                        }
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - j - (y + i) * 0.5f, y + i), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - j, y + i);
                        }
                    }
                    else
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - j - (y + i) * 0.5f, y + i), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - j, y + i);
                        }
                    }
                }


                if (y + i < _height && x + j < _width)            // up, right, side
                {
                    wallCheckLineEnd = new Vector2(x + j, y + i + 3 * _height);
                    if (onBoardEntities[x + j, y + i] && onBoardEntities[x + j, y + i].whatTeam() != turnSide)
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                         Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x + j, y + i].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x + j, y + i].isCrouched()))))
                        {
                            GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x + j - (y + i) * 0.5f, y + i), Quaternion.identity);
                            boardCheck[x + j, y + i] = EnemyCheck;
                            EnemyCheck.name = "EnemyCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + j, y + i);
                        }
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + j - (y + i) * 0.5f, y + i), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + j, y + i);
                        }
                    }
                    else
                    {
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                        {
                            GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + j - (y + i) * 0.5f, y + i), Quaternion.identity);
                            EnemyCheck.name = "FOVCheck";
                            Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + j, y + i);
                        }
                    }
                }
            }
        }


            // best bevels
            m = n;
            float _diagonalsA = n * (float)(Math.Sqrt(2));
            float _diagonalsB = (n - 1) * (float)(Math.Sqrt(2));
            while (_diagonalsA > n && _diagonalsB > n)
            {
                m--;
                _diagonalsA = m * (float)(Math.Sqrt(2));
                _diagonalsB = (m - 1) * (float)(Math.Sqrt(2));
            }

            if( absDif(_diagonalsA , n) > absDif(_diagonalsB, n))
            {
                m--;
            }

        for (int i = 1; i < m; i++)
        {
            if (y + i < _height && x + i < _width)          // up, right
            {
                wallCheckLineEnd = new Vector2(x + i, y + i + 3 * _height);
                if (onBoardEntities[x + i, y + i] && onBoardEntities[x + i, y + i].whatTeam() != turnSide)
                {
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                      Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x + i, y + i].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x + i, y + i].isCrouched()))))
                    {
                        GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x + i - (y + i) * 0.5f, y + i), Quaternion.identity);
                        boardCheck[x + i, y + i] = EnemyCheck;
                        EnemyCheck.name = "EnemyCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x + i, y + i);
                    }
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + i - (y + i) * 0.5f, y + i), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x + i, y + i);
                    }
                }
                else
                {
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + i - (y + i) * 0.5f, y + i), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x + i, y + i);
                    }
                }
            }

            if (y - i >= 0 && x + i < _width)          // down, right
            {
                wallCheckLineEnd = new Vector2(x + i, y - i + 3 * _height);
                if (onBoardEntities[x + i, y - i] && onBoardEntities[x + i, y - i].whatTeam() != turnSide)
                {
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                      Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x + i, y - i].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x + i, y - i].isCrouched()))))
                    {
                        GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x + i - (y - i) * 0.5f, y - i), Quaternion.identity);
                        boardCheck[x + i, y - i] = EnemyCheck;
                        EnemyCheck.name = "EnemyCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x + i, y - i);
                    }
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + i - (y - i) * 0.5f, y - i), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x + i, y - i);
                    }
                }
                else
                {
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x + i - (y - i) * 0.5f, y - i), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x + i, y - i);
                    }
                }
            }

            if (y + i < _height && x - i >= 0)          // up, left
            {
                wallCheckLineEnd = new Vector2(x - i, y + i + 3 * _height);
                if (onBoardEntities[x - i, y + i] && onBoardEntities[x - i, y + i].whatTeam() != turnSide)
                {
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                      Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x - i, y + i].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x - i, y + i].isCrouched()))))
                    {
                        GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x - i - (y + i) * 0.5f, y + i), Quaternion.identity);
                        boardCheck[x - i, y + i] = EnemyCheck;
                        EnemyCheck.name = "EnemyCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x - i, y + i);
                    }
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - i - (y + i) * 0.5f, y + i), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x - i, y + i);
                    }
                }
                else
                {
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - i - (y + i) * 0.5f, y + i), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x - i, y + i);
                    }
                }
            }

            if (y - i >= 0 && x - i >= 0)          // down, left
            {
                wallCheckLineEnd = new Vector2(x - i, y - i + 3 * _height);
                if (onBoardEntities[x - i, y - i] && onBoardEntities[x - i, y - i].whatTeam() != turnSide)
                {
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.red, 10f);  //debug line
                      Debug.DrawLine(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), Color.magenta, 10f);  //debug line 
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && (((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()) && ((onBoardEntities[x - i, y - i].isCrouched() && !(Physics2D.Linecast(wallCheckLineEnd, halfWallCheckPoint(wallCheckLineStart, wallCheckLineEnd), halfWallMask)) || !onBoardEntities[x - i, y - i].isCrouched()))))
                    {
                        GameObject EnemyCheck = Instantiate(EnemyHighlight, new Vector3(x - i - (y - i) * 0.5f, y - i), Quaternion.identity);
                        boardCheck[x - i, y - i] = EnemyCheck;
                        EnemyCheck.name = "EnemyCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x - i, y - i);
                    }
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    else if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - i - (y - i) * 0.5f, y - i), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x - i, y - i);
                    }
                }
                else
                {
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, smokeMask) && !Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask)) && ((!Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()) || !onBoardEntities[x, y].isCrouched()))
                    {
                        GameObject EnemyCheck = Instantiate(FOVHighlight, new Vector3(x - i - (y - i) * 0.5f, y - i), Quaternion.identity);
                        EnemyCheck.name = "FOVCheck";
                        Highlight highlightScript = EnemyCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x - i, y - i);
                    }
                }
            }
        }
    }

    void HighlightGadgetUse(int x, int y, int n, bool halfWallBlocked, bool centerIncluded) // n - range
    {
        Vector2 wallCheckLineStart, wallCheckLineEnd;
        wallCheckLineStart = new Vector2(x, y + 3 * _height);
        LayerMask wallMask = LayerMask.GetMask("Wall");
        LayerMask halfWallMask = LayerMask.GetMask("HalfWall");

        //          center check
        if(centerIncluded)
        {
            GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x - y * 0.5f, y), Quaternion.identity);
            boardCheck[x, y] = GadgetCheck;
            GadgetCheck.name = "GadgetCheck";
            Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
            highlightScript.setCoordinates(x, y);
        }

        //          cross check
        for (int i = 0; i < n; i++)
        {
            if (i + y - n >= 0 && i + y - n < _height)      // check down
            {
                wallCheckLineEnd = new Vector2(x, i + y - n + 3 * _height);
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                    {
                        GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x - (i + y - n) * 0.5f, i + y - n), Quaternion.identity);
                        boardCheck[x, i + y - n] = GadgetCheck;
                        GadgetCheck.name = "GadgetCheck";
                        Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x, i + y - n);
                    }
            }

            if (i + 1 + y >= 0 && i + 1 + y < _height)      // check up
            {
                wallCheckLineEnd = new Vector2(x, i + 1 + y + 3 * _height);
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                    {
                        GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x - (i + 1 + y) * 0.5f, i + 1 + y), Quaternion.identity);
                        boardCheck[x, i + 1 + y] = GadgetCheck;
                        GadgetCheck.name = "GadgetCheck";
                        Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x, i + 1 + y);
                    }
            }

            if (i + x - n >= 0 && i + x - n < _width)       // check left
            {
                wallCheckLineEnd = new Vector2(i + x - n, y + (_height + 16));
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                    {
                        GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(i + x - n - y * 0.5f, y), Quaternion.identity);
                        boardCheck[i + x - n, y] = GadgetCheck;
                        GadgetCheck.name = "GadgetCheck";
                        Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(i + x - n, y);
                    }
            }

            if (i + x + 1 >= 0 && i + x + 1 < _width)       // check right
            {
                wallCheckLineEnd = new Vector2(i + x + 1, y + (_height + 16));
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                    {
                        GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(i + x + 1 - y * 0.5f, y), Quaternion.identity);
                        boardCheck[i + x + 1, y] = GadgetCheck;
                        GadgetCheck.name = "GadgetCheck";
                        Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(i + x + 1, y);
                    }
            }

        }



        //          circle algorithm
        int m = y + n;
        for (int i = 1; i <= m - y; i++)
        {
            float pktE = (float)(Math.Sqrt(i * i + (m - y) * (m - y))), pktSE = (float)(Math.Sqrt(i * i + (m - y - 1) * (m - y - 1)));
            float ra = absDif(pktE, n), rb = absDif(pktSE, n);
            if (ra > rb)
            {
                m--;
            }

            // if conditions: up -> (y + j < _height); down -> (y + j >= 0); left -> (x + i >= 0); right -> (x + i < _width)

            for (int j = 1 + i; j <= m - y; j++)            // up, right, mid
            {
                if (y + j < _height && x + i < _width)
                {
                    wallCheckLineEnd = new Vector2(x + i, y + j + 3 * _height);
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                        {
                            GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x + i - (y + j) * 0.5f, y + j), Quaternion.identity);
                            boardCheck[x + i, y + j] = GadgetCheck;
                            GadgetCheck.name = "GadgetCheck";
                            Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + i, y + j);
                        }
                }


                if (y - j >= 0 && x + i < _width)            // down, right, mid
                {
                    wallCheckLineEnd = new Vector2(x + i, y - j + 3 * _height);
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                        {
                            GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x + i - (y - j) * 0.5f, y - j), Quaternion.identity);
                            boardCheck[x + i, y - j] = GadgetCheck;
                            GadgetCheck.name = "GadgetCheck";
                            Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + i, y - j);
                        }
                }

                if (y + j < _height && x - i >= 0)        // up, left, mid
                {
                    wallCheckLineEnd = new Vector2(x - i, y + j + 3 * _height);
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                        {
                            GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x - i - (y + j) * 0.5f, y + j), Quaternion.identity);
                            boardCheck[x - i, y + j] = GadgetCheck;
                            GadgetCheck.name = "GadgetCheck";
                            Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - i, y + j);
                        }
                }


                if (y - j >= 0 && x - i >= 0)            // down, left, mid
                {
                    wallCheckLineEnd = new Vector2(x - i, y - j + 3 * _height);
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                        {
                            GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x - i - (y - j) * 0.5f, y - j), Quaternion.identity);
                            boardCheck[x - i, y - j] = GadgetCheck;
                            GadgetCheck.name = "GadgetCheck";
                            Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - i, y - j);
                        }
                }


                if (y - i >= 0 && x - j >= 0)            // down, left, side
                {
                    wallCheckLineEnd = new Vector2(x - j, y - i + 3 * _height);
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                        {
                            GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x - j - (y - i) * 0.5f, y - i), Quaternion.identity);
                            boardCheck[x - j, y - i] = GadgetCheck;
                            GadgetCheck.name = "GadgetCheck";
                            Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - j, y - i);
                        }
                }


                if (y - i >= 0 && x + j < _width)            // down, right, side
                {
                    wallCheckLineEnd = new Vector2(x + j, y - i + 3 * _height);
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                        {
                            GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x + j - (y - i) * 0.5f, y - i), Quaternion.identity);
                            boardCheck[x + j, y - i] = GadgetCheck;
                            GadgetCheck.name = "GadgetCheck";
                            Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + j, y - i);
                        }
                }


                if (y + i < _height && x - j >= 0)            // up, left, side
                {
                    wallCheckLineEnd = new Vector2(x - j, y + i + 3 * _height);
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                        {
                            GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x - j - (y + i) * 0.5f, y + i), Quaternion.identity);
                            boardCheck[x - j, y + i] = GadgetCheck;
                            GadgetCheck.name = "GadgetCheck";
                            Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x - j, y + i);
                        }
                }


                if (y + i < _height && x + j < _width)            // up, right, side
                {
                    wallCheckLineEnd = new Vector2(x + j, y + i + 3 * _height);
                        // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                        if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                        {
                            GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x + j - (y + i) * 0.5f, y + i), Quaternion.identity);
                            boardCheck[x + j, y + i] = GadgetCheck;
                            GadgetCheck.name = "GadgetCheck";
                            Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                            highlightScript.setCoordinates(x + j, y + i);
                        }
                }
            }
        }


        // best bevels
        m = n;
        float _diagonalsA = n * (float)(Math.Sqrt(2));
        float _diagonalsB = (n - 1) * (float)(Math.Sqrt(2));
        while (_diagonalsA > n && _diagonalsB > n)
        {
            m--;
            _diagonalsA = m * (float)(Math.Sqrt(2));
            _diagonalsB = (m - 1) * (float)(Math.Sqrt(2));
        }

        if (absDif(_diagonalsA, n) > absDif(_diagonalsB, n))
        {
            m--;
        }

        for (int i = 1; i < m; i++)
        {
            if (y + i < _height && x + i < _width)          // up, right
            {
                wallCheckLineEnd = new Vector2(x + i, y + i + 3 * _height);
                    // Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                    {
                        GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x + i - (y + i) * 0.5f, y + i), Quaternion.identity);
                        boardCheck[x + i, y + i] = GadgetCheck;
                        GadgetCheck.name = "GadgetCheck";
                        Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x + i, y + i);
                    }
            }

            if (y - i >= 0 && x + i < _width)          // down, right
            {
                wallCheckLineEnd = new Vector2(x + i, y - i + 3 * _height);
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                    {
                        GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x + i - (y - i) * 0.5f, y - i), Quaternion.identity);
                        boardCheck[x + i, y - i] = GadgetCheck;
                        GadgetCheck.name = "GadgetCheck";
                        Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x + i, y - i);
                    }
            }

            if (y + i < _height && x - i >= 0)          // up, left
            {
                wallCheckLineEnd = new Vector2(x - i, y + i + 3 * _height);
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                    {
                        GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x - i - (y + i) * 0.5f, y + i), Quaternion.identity);
                        boardCheck[x - i, y + i] = GadgetCheck;
                        GadgetCheck.name = "GadgetCheck";
                        Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x - i, y + i);
                    }
            }

            if (y - i >= 0 && x - i >= 0)          // down, left
            {
                wallCheckLineEnd = new Vector2(x - i, y - i + 3 * _height);
                    //  Debug.DrawLine(wallCheckLineStart, wallCheckLineEnd, Color.yellow, 10f);  //debug line
                    if (!(Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, wallMask) || (halfWallBlocked && (Physics2D.Linecast(wallCheckLineStart, wallCheckLineEnd, halfWallMask) && onBoardEntities[x, y].isCrouched()))))
                    {
                        GameObject GadgetCheck = Instantiate(GadgetHighlight, new Vector3(x - i - (y - i) * 0.5f, y - i), Quaternion.identity);
                        boardCheck[x - i, y - i] = GadgetCheck;
                        GadgetCheck.name = "GadgetCheck";
                        Highlight highlightScript = GadgetCheck.GetComponent<Highlight>();
                        highlightScript.setCoordinates(x - i, y - i);
                    }
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
            onBoardEntities[activeX, activeY].ShootAt(passiveX - passiveY * 0.5f, passiveY+1);
            int DamageDealt = 50;   // for constant damage to improve later
            onBoardEntities[passiveX, passiveY].takeDamage(DamageDealt);
            UpdateMovesCount();
        }
    }

    void Move()
    {
        bool _directional = (passiveX != activeX);
        bool _moveSide = (passiveX > activeX);

        onBoardEntities[activeX, activeY].SetPosition(passiveY, passiveX);
        onBoardEntities[activeX, activeY].Walk(_moveSide, _directional, new Vector3(passiveX - passiveY * 0.5f, passiveY));

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

    void SetUnitInfoDisplay()
    {
        Unit.UnitInfo _info = selectedUnit.GetInfoToDisplay();
        UnitControlsHP.showMessage(_info.hp.ToString());
        UnitControlsAmmo.showMessage(_info.mag.ToString());
        UnitControlsNumber.showMessage(_info.number.ToString());
        UnitControlsSymbol.ChangeIcon(_info.role, _info.team);
        UnitControlsMoves.showMessage(_info.movesCount.ToString());
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
                SetUnitInfoDisplay();
                UnitControlsUI.SetActive(true);
                if (!moveHighlightsOn)
                {
                    if (!selectedUnit.isGadgetActive())
                    {
                        HighlightPossibleMoves();
                        HighlightPossibleAttacks(activeX, activeY, onBoardEntities[activeX, activeY].whatRange());
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
                    if(boardCheck[passiveX,passiveY] && onBoardEntities[activeX, activeY].CanMove())
                    {
                        if(boardCheck[passiveX, passiveY].name == "MoveCheck")
                        {
                            Move();
                            if (unitIsSelected && !cameraZoomed)
                            {
                                cameraController.MoveCamera(activeX - activeY * 0.5f, activeY, onBoardEntities[activeX, activeY].whatRange());
                                cameraZoomed = true;
                            }
                        }
                        else if (boardCheck[passiveX, passiveY].name == "EnemyCheck")
                        {
                            Attack();
                        }
                        else if (boardCheck[passiveX, passiveY].name == "GadgetCheck")
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

