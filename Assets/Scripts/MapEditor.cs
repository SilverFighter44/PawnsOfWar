using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class MapEditor : MonoBehaviour
{
    public static MapEditor Instance;

    public event EventHandler resetPreview;

    [SerializeField] private Toggle spawnsToggle, hideWallsToggle, bucketToggle;
    [SerializeField] private TMP_InputField widthInput, heightInput, spawnTypeInput, mapNameInput, movesLimitInput, teamSizeInput;
    [SerializeField] private GameObject NewMapWindow, ConfirmDeleteWindow, tilePrefab, currentSubMenu, tilesSubMenu, wallsSubMenu, spawnsSubMenu, gameplayMenu, optionsMenu, saveMenu, WallX, WallY, WindowFrameX, WindowFrameY, DoorFrameX, DoorFrameY, HalfWallX, HalfWallY, SpawnMarker, flagB, flagR;
    private List<string> mapFiles = new List<string>();

    [SerializeField] private GridTools.Map currentMap;
    [SerializeField] private GridTools.MapPreview currentPreview;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private TMP_Dropdown editingModeDropdown, tileCategoryDropdown, wallDirectionDropdown, wallTypeDropdown, frontFaceDropdown, backFaceDropdown, insideDropdown, spawnsDropdown, gameplayDropdown, gamemodeDropdown, mapFilesDropdown;

    [SerializeField] private GridTools.tileType currentType;
    public event EventHandler clearMap;

    private bool checkSpawnsSet()
    {
        bool f = true;
        for (int i = 0; i < currentMap.teamSize.Value; i++)
        {
            if (!currentMap.spawnsBlue[i].HasValue)
            {
                    f = false;
                    break;
            }
            if (!currentMap.spawnsRed[i].HasValue)
            {
                    f = false;
                    break;
            }
        }
        return f;
    }

    public void DeleteMap()
    {
        ConfirmDeleteWindow.SetActive(true);
    }

    public void ConfirmYes()
    {
        string filePath = Application.persistentDataPath + "/map_" + mapFilesDropdown.captionText.text + ".json";
        System.IO.File.Delete(filePath);
        CheckForMapFiles();
        ConfirmDeleteWindow.SetActive(false);
    }

    public void ConfirmNo()
    {
        ConfirmDeleteWindow.SetActive(false);
    }

    public void SaveMap()
    {
        string mapName = mapNameInput.text; //might need to check input
        if(checkSpawnsSet())
        {
            if(currentMap.gameplayObjects[0].HasValue)
            {
                currentMap.modeCompatibility[0] = true;
            }
            if (currentMap.gameplayObjects[1].HasValue)
            {
                currentMap.modeCompatibility[1] = true;
            }
            currentMap.movesLimit = Int32.Parse(movesLimitInput.text);
            GridTools.MapIntermediate mapIntermediate = GridTools.translateMapToIntermediate(currentMap);            
            string mapJSON = JsonUtility.ToJson(mapIntermediate);
            string filePath = Application.persistentDataPath + "/map_" + mapName + ".json";
            Debug.Log(filePath);
            System.IO.File.WriteAllText(filePath, mapJSON);
            Debug.Log("Saving finished");
            CheckForMapFiles();
        }
    }

    public void LoadMap()
    {
        string filePath = Application.persistentDataPath + "/map_" + mapFilesDropdown.captionText.text + ".json";
        string mapData = System.IO.File.ReadAllText(filePath);
        GridTools.MapIntermediate mapIntermediate = JsonUtility.FromJson<GridTools.MapIntermediate>(mapData);
        currentMap = GridTools.translateMapFromIntermediate(mapIntermediate);
        Debug.Log("Loading finished");
        ResetPreview();
        currentPreview = new GridTools.MapPreview(currentMap.width, currentMap.height, currentMap.teamSize.Value);
        for (int x = 0; x < currentMap.width; x++)
        {
            for (int y = 0; y < currentMap.height; y++)
            {
                GameObject spawnedTile = Instantiate(tilePrefab, new Vector3(x - y * 0.5f, y), Quaternion.identity);
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
                spawnedTile.GetComponent<EditorTile>().Init(isOffset, x, y);
            }
        }
        cameraController.SetDefaultPosition(((float)currentMap.width - ((float)currentMap.height - 1) * 0.5f) / 2 - 0.5f, (float)currentMap.width / 2 - 0.5f);
        cameraController.SetBorders(currentMap.width, currentMap.height);
        for(int x = 0; x < currentMap.width + 1; x++)
        {
            for (int y = 0; y < currentMap.height; y++)
            {
                if (currentMap.verticalWalls[x,y].HasValue)
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
                }
            }
        }
        for (int i = 0; i < currentMap.teamSize.Value; i++)
        {
            if (currentMap.spawnsBlue[i].HasValue)
            {
                GameObject currentMarker = Instantiate(SpawnMarker, new Vector3(currentMap.spawnsBlue[i].Value.x - currentMap.spawnsBlue[i].Value.y * 0.5f, currentMap.spawnsBlue[i].Value.y), Quaternion.identity);
                currentMarker.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(currentMarker.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), "Blue");
                currentMarker.GetComponentInChildren<TMP_Text>().text = (i+1).ToString();
                currentPreview.blueSpawns[i] = currentMarker;
            }
            if (currentMap.spawnsRed[i].HasValue)
            {
                GameObject currentMarker = Instantiate(SpawnMarker, new Vector3(currentMap.spawnsRed[i].Value.x - currentMap.spawnsRed[i].Value.y * 0.5f, currentMap.spawnsRed[i].Value.y), Quaternion.identity);
                currentMarker.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(currentMarker.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), "Red");
                currentMarker.GetComponentInChildren<TMP_Text>().text = (i + 1).ToString();
                currentPreview.redSpawns[i] = currentMarker;
            }
        }
        if (currentMap.gameplayObjects[0].HasValue)
        {
            GameObject currentFlag = Instantiate(flagB, new Vector3(currentMap.gameplayObjects[0].Value.x - currentMap.gameplayObjects[0].Value.y * 0.5f - 0.25f, currentMap.gameplayObjects[0].Value.y - 0.5f), Quaternion.identity);
            currentPreview.blueFlag = currentFlag;
        }
        if (currentMap.gameplayObjects[1].HasValue)
        {
            GameObject currentFlag = Instantiate(flagR, new Vector3(currentMap.gameplayObjects[1].Value.x - currentMap.gameplayObjects[1].Value.y * 0.5f - 0.25f, currentMap.gameplayObjects[1].Value.y - 0.5f), Quaternion.identity);
            currentPreview.redFlag = currentFlag;
        }
    }

    private void Awake()
    {
        Instance = this;
        NewMapWindow.SetActive(false);
        ConfirmDeleteWindow.SetActive(false);
        tilesSubMenu.SetActive(false);
        wallsSubMenu.SetActive(false);
        spawnsSubMenu.SetActive(false);
        gameplayMenu.SetActive(false);
        optionsMenu.SetActive(false);
        saveMenu.SetActive(false);
        ChooseEditingMode();
        spawnTypeInput.onValueChanged.AddListener(delegate { spawnTypeValueCheck(); });
        spawnTypeInput.characterLimit = 2;
        spawnTypeInput.onValidateInput = (string text, int charIndex, char addedChar) =>
        {
            return ValidateChar("1234567890", addedChar);
        };
        widthInput.characterLimit = 2;
        widthInput.onValidateInput = (string text, int charIndex, char addedChar) =>
        {
            return ValidateChar("1234567890", addedChar);
        };
        heightInput.characterLimit = 2;
        heightInput.onValidateInput = (string text, int charIndex, char addedChar) =>
        {
            return ValidateChar("1234567890", addedChar);
        };
        movesLimitInput.onDeselect.AddListener(delegate { movesLimitValueCheck(); });
        movesLimitInput.characterLimit = 2;
        movesLimitInput.onValidateInput = (string text, int charIndex, char addedChar) =>
        {
            return ValidateChar("1234567890", addedChar);
        };
        teamSizeInput.onDeselect.AddListener(delegate { teamSizeValueCheck(); });
        teamSizeInput.characterLimit = 1;
        teamSizeInput.onValidateInput = (string text, int charIndex, char addedChar) =>
        {
            return ValidateChar("1234567890", addedChar);
        };
        currentPreview = new GridTools.MapPreview(0, 0, 0);
        CheckForMapFiles();
    }

    public void CheckForMapFiles()
    {
        mapFiles = new List<string>();
        string[] filesInDirectory = Directory.GetFiles(Application.persistentDataPath);
        for (int i = 0; i < filesInDirectory.Length; i++)
        {
            filesInDirectory[i] = filesInDirectory[i].Remove(0, Application.persistentDataPath.Length + 1);
            if (filesInDirectory[i].StartsWith("map_") && filesInDirectory[i].EndsWith(".json"))
            {
                filesInDirectory[i] = filesInDirectory[i].Remove(0, "map_".Length);
                filesInDirectory[i] = filesInDirectory[i].Remove(filesInDirectory[i].Length - ".json".Length, ".json".Length);
                mapFiles.Add(filesInDirectory[i]);
            }
        }
        mapFilesDropdown.ClearOptions();
        mapFilesDropdown.AddOptions(mapFiles);
    }

    public void SpawnsPreviewVisibility()
    {
        if(spawnsToggle.isOn)
        {
            for(int i = 0; i < currentMap.teamSize.Value; i++)
            {
                Helper.setObjetsVisibility(currentPreview.blueSpawns[i], true);
                Helper.setObjetsVisibility(currentPreview.redSpawns[i], true);
            }
        }
        else
        {
            for (int i = 0; i < currentMap.teamSize.Value; i++)
            {
                Helper.setObjetsVisibility(currentPreview.blueSpawns[i], false);
                Helper.setObjetsVisibility(currentPreview.redSpawns[i], false);
            }
        }
    }

    public void WallsVisibility()
    {
        for (int x = 0; x < currentMap.width + 1; x++)
        {
            for (int y = 0; y < currentMap.height; y++)
            {
                if (currentPreview.verticalWalls[x, y])
                {
                    if(hideWallsToggle.isOn)
                    {
                        currentPreview.verticalWalls[x, y].GetComponent<Wall>().hide();
                    }
                    else
                    {
                        currentPreview.verticalWalls[x, y].GetComponent<Wall>().appear();
                    }
                }
            }
        }
        for (int x = 0; x < currentMap.width; x++)
        {
            for (int y = 0; y < currentMap.height + 1; y++)
            {
                if (currentPreview.horizontalWalls[x, y])
                {
                    if (hideWallsToggle.isOn)
                    {
                        currentPreview.horizontalWalls[x, y].GetComponent<Wall>().hide();
                    }
                    else
                    {
                        currentPreview.horizontalWalls[x, y].GetComponent<Wall>().appear();
                    }
                }
            }
        }
    }

    public void GameplayObjectsVisibility()
    {
        switch (gamemodeDropdown.value)
        {
            case 0:
                Helper.setObjetsVisibility(currentPreview.blueFlag, true);
                Helper.setObjetsVisibility(currentPreview.redFlag, true);
                break;
            case 1:
                Helper.setObjetsVisibility(currentPreview.blueFlag, false);
                Helper.setObjetsVisibility(currentPreview.redFlag, false);
                break;
            case 2:
                Helper.setObjetsVisibility(currentPreview.blueFlag, true);
                Helper.setObjetsVisibility(currentPreview.redFlag, false);
                break;
            case 3:
                Helper.setObjetsVisibility(currentPreview.blueFlag, false);
                Helper.setObjetsVisibility(currentPreview.redFlag, true);
                break;
            case 4:
                Helper.setObjetsVisibility(currentPreview.blueFlag, true);
                Helper.setObjetsVisibility(currentPreview.redFlag, true);
                break;
            default:
                break;
        }
    }

    public void DeleteGameplayObject()
    {
        switch(gameplayDropdown.value)
        {
            case 0:
                Destroy(currentPreview.blueFlag);
                if(currentMap.gameplayObjects[0].HasValue)
                {
                    currentMap.gameplayObjects[0] = null;
                }
                break;
            case 1:
                Destroy(currentPreview.redFlag);
                if (currentMap.gameplayObjects[1].HasValue)
                {
                    currentMap.gameplayObjects[1] = null;
                }
                break;
            default:
                break;
        }
    }

    public void teamSizeValueCheck()
    {
        string inputText = teamSizeInput.text;
        if (inputText == string.Empty)
        {
            teamSizeInput.text = GridTools.teamSizeMin.ToString();
        }
        else
        {
            if (Int32.Parse(teamSizeInput.text) < GridTools.teamSizeMin || Int32.Parse(teamSizeInput.text) > GridTools.teamSizeMax) // 8 and 100 are limits
            {
                teamSizeInput.text = GridTools.teamSizeMin.ToString();
            }
        }

        for (int i = 0; currentMap.teamSize > i; i++)
        {
            if (currentPreview.redSpawns[i])
            {
                Destroy(currentPreview.redSpawns[i]);
            }
            if (currentPreview.blueSpawns[i])
            {
                Destroy(currentPreview.blueSpawns[i]);
            }
        }
        currentMap.teamSize = Int32.Parse(teamSizeInput.text);
        currentMap.spawnsBlue = new GridTools.TileCoordinates?[currentMap.teamSize.Value];
        currentMap.spawnsRed = new GridTools.TileCoordinates?[currentMap.teamSize.Value];
        currentPreview.blueSpawns = new GameObject[currentMap.teamSize.Value];
        currentPreview.redSpawns = new GameObject[currentMap.teamSize.Value];

    }

    public void movesLimitValueCheck()
    {
        string inputText = movesLimitInput.text;
        if (inputText == string.Empty)
        {
            movesLimitInput.text = "0";
        }
        else
        {
            if (inputText[0] == '0' && GridTools.movesLimitDown != 0)
            {
                movesLimitInput.text = GridTools.movesLimitDown.ToString();
            }
            if (Int32.Parse(movesLimitInput.text) > GridTools.movesLimitUp || Int32.Parse(movesLimitInput.text) < GridTools.movesLimitDown) // 8 and 100 are limits
            {
                movesLimitInput.text = GridTools.movesLimitDown.ToString();
            }
        }
    }

    public void spawnTypeValueCheck()
    {
        string inputText = spawnTypeInput.text;
        if(inputText == string.Empty)
        {
            spawnTypeInput.text = "0";
        }
        else
        {
            if(inputText[0] == '0' && inputText != "0")
            {
                spawnTypeInput.text = "0";
            }
            if(currentMap.teamSize.HasValue)
            {
                if(Int32.Parse(spawnTypeInput.text) > currentMap.teamSize.Value || Int32.Parse(spawnTypeInput.text) < 0)
                {
                    spawnTypeInput.text = "0";
                } 
            }
            else
            {
                spawnTypeInput.text = "0";
            }
        }
    }

    public void ResetPreview()
    {
        if (resetPreview != null)
        {
            resetPreview(this, EventArgs.Empty);
        }
        if(currentPreview.blueFlag)
        {
            Destroy(currentPreview.blueFlag);
        }
        if (currentPreview.redFlag)
        {
            Destroy(currentPreview.redFlag);
        }
        if (currentMap.teamSize.HasValue)
        {
            for (int i = 0; i < currentPreview.blueSpawns.Length; i++)
            {
                if (currentPreview.blueSpawns[i])
                {
                    Destroy(currentPreview.blueSpawns[i]);
                }
                if (currentPreview.redSpawns[i])
                {
                    Destroy(currentPreview.redSpawns[i]);
                }
            }
        }
    }

    public void ChooseEditingMode()
    {
        if(currentSubMenu)
        {
            currentSubMenu.SetActive(false);
        }
        switch (editingModeDropdown.value)
        {
            case 0:
                currentSubMenu = tilesSubMenu;
                currentSubMenu.SetActive(true);
                break;
            case 1:
                currentSubMenu = wallsSubMenu;
                currentSubMenu.SetActive(true);
                break;
            case 2:
                currentSubMenu = spawnsSubMenu;
                currentSubMenu.SetActive(true);
                break;
            case 3:
                currentSubMenu = gameplayMenu;
                currentSubMenu.SetActive(true);
                break;
            case 4:
                currentSubMenu = optionsMenu;
                currentSubMenu.SetActive(true);
                break;
            case 5:
                currentSubMenu = saveMenu;
                currentSubMenu.SetActive(true);
                break;
            default:
                break;
        }
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
            if (raycastResultList[i].gameObject.GetComponent<BlockMouse>() == null)
            {
                raycastResultList.RemoveAt(i);
                i--;
            }
        }
        return raycastResultList.Count > 0;
    }

    public void ShowNewMapWindow()
    {
        NewMapWindow.SetActive(true);
    }

    public void HideNewMapWindow()
    {
        NewMapWindow.SetActive(false);
    }

    public void CreateNewMap()
    {
        
        if ((widthInput.text != null && heightInput.text != null) && ((Int32.Parse(widthInput.text) >= 8 && Int32.Parse(widthInput.text) <= 50) && (Int32.Parse(heightInput.text) >= 8 && Int32.Parse(heightInput.text) <= 50)))
        {
            ResetPreview();
            currentMap = new GridTools.Map(Int32.Parse(widthInput.text), Int32.Parse(heightInput.text));
            currentPreview = new GridTools.MapPreview(Int32.Parse(widthInput.text), Int32.Parse(heightInput.text), currentMap.teamSize.Value);
            HideNewMapWindow();
            GenerateNewMap();
        }
    }

    public void GenerateNewMap()
    {
        for (int x = 0; x < currentMap.width; x++)
        {
            for (int y = 0; y < currentMap.height; y++)
            {
                GameObject spawnedTile = Instantiate(tilePrefab, new Vector3(x - y * 0.5f, y), Quaternion.identity);
                spawnedTile.GetComponent<EditorTile>().name = $"Tile {x} {y}";

                currentMap.tileGrid[x,y] = new GridTools.TileInfo(x, y, GridTools.tileType.blank);
                currentPreview.gridPreview[x, y] = spawnedTile.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>();

                var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.GetComponent<EditorTile>().Init(isOffset, x, y);

            }
        }
        cameraController.SetDefaultPosition(((float)currentMap.width - ((float)currentMap.height - 1) * 0.5f) / 2 - 0.5f, (float)currentMap.width / 2 - 0.5f);
        cameraController.SetBorders(currentMap.width, currentMap.height);
    }

    private char ValidateChar(string validCharacters, char addedChar)
    {
        if(validCharacters.IndexOf(addedChar) != -1)
        {
            return addedChar;
        }
        else
        {
            return '\0';
        }
    }

    private bool validateSpawnCoords(int x, int y)
    {
        bool f = true;
        for(int i = 0; i < currentMap.teamSize.Value; i++)
        {
            if (currentMap.spawnsBlue[i].HasValue)
            {
                if (currentMap.spawnsBlue[i].Value.x   == x && currentMap.spawnsBlue[i].Value.y == y)
                {
                    f = false;
                    break;
                }
            }
            if (currentMap.spawnsRed[i].HasValue)
            {
                if (currentMap.spawnsRed[i].Value.x == x && currentMap.spawnsRed[i].Value.y == y)
                {
                    f = false;
                    break;
                }
            }
        }
        return f;
    }

    private int findSpawnNumberWithCoordinates(int x, int y, bool blue)
    {
        for (int i = 0; i < currentMap.teamSize.Value; i++)
        {
            if(currentMap.spawnsBlue[i].HasValue)
            {
                if ((currentMap.spawnsBlue[i].Value.x == x && currentMap.spawnsBlue[i].Value.y == y) && blue)
                { 
                    return i + 1;
                }
            }
            if (currentMap.spawnsRed[i].HasValue)
            {
                if ((currentMap.spawnsRed[i].Value.x == x && currentMap.spawnsRed[i].Value.y == y) && !blue)
                {
                    return i + 1;
                }
            }
        }
        return 0;
    }

    public void bucketFill(int x, int y, GridTools.tileType newColor, string categoryName)
    {
        GridTools.tileType oldColor = currentMap.tileGrid[x, y].tileType;
        if (oldColor == newColor)
        {
            return;
        }
        Queue<GridTools.TileCoordinates> tilesQueue = new Queue<GridTools.TileCoordinates>();
        tilesQueue.Enqueue(new GridTools.TileCoordinates(x, y));
        while(tilesQueue.Count != 0)
        {
            GridTools.TileCoordinates currentTile = tilesQueue.Dequeue();
            currentMap.tileGrid[currentTile.x, currentTile.y] = new GridTools.TileInfo(currentTile.x, currentTile.y, newColor);
            currentPreview.gridPreview[currentTile.x, currentTile.y].SetCategoryAndLabel(currentPreview.gridPreview[currentTile.x, currentTile.y].GetCategory(), categoryName);
            if(currentTile.x > 0)
            {
                if(currentMap.tileGrid[currentTile.x - 1, currentTile.y].tileType == oldColor && !currentMap.verticalWalls[currentTile.x,currentTile.y].HasValue)
                {
                    tilesQueue.Enqueue(new GridTools.TileCoordinates(currentTile.x - 1, currentTile.y));
                }
            }
            if (currentTile.y > 0)
            {
                if (currentMap.tileGrid[currentTile.x, currentTile.y - 1].tileType == oldColor && !currentMap.horizontalWalls[currentTile.x, currentTile.y].HasValue)
                {
                    tilesQueue.Enqueue(new GridTools.TileCoordinates(currentTile.x, currentTile.y - 1));
                }
            }
            if (currentTile.x < currentMap.width - 1)
            {
                if (currentMap.tileGrid[currentTile.x + 1, currentTile.y].tileType == oldColor && !currentMap.verticalWalls[currentTile.x + 1, currentTile.y].HasValue)
                {
                    tilesQueue.Enqueue(new GridTools.TileCoordinates(currentTile.x + 1, currentTile.y));
                }
            }
            if (currentTile.y < currentMap.height - 1)
            {
                if (currentMap.tileGrid[currentTile.x, currentTile.y + 1].tileType == oldColor && !currentMap.horizontalWalls[currentTile.x, currentTile.y + 1].HasValue)
                {
                    tilesQueue.Enqueue(new GridTools.TileCoordinates(currentTile.x, currentTile.y + 1));
                }
            }
        }
    }

    public void OnTileClick(int x, int y)
    {
        Debug.Log(x + " " + y);
        switch (editingModeDropdown.value)
        {
            case 0:
                GridTools.tileType newTileType;
                string newCategoryName;
                switch (tileCategoryDropdown.value)
                {
                    case 1:
                        newTileType = GridTools.tileType.cobblestone;
                        newCategoryName = "Cobblestone";
                        break;
                    case 2:
                        newTileType = GridTools.tileType.sand;
                        newCategoryName = "Sand";
                        break;
                    case 3:
                        newTileType = GridTools.tileType.sandRoad;
                        newCategoryName = "SandRoad";
                        break;
                    case 4:
                        newTileType = GridTools.tileType.woodenFloor1;
                        newCategoryName = "WoodFloor1";
                        break;
                    default:
                        newTileType = GridTools.tileType.blank;
                        newCategoryName = "Blank";
                        break;
                }
                if (!bucketToggle.isOn)
                {
                    currentMap.tileGrid[x, y] = new GridTools.TileInfo(x, y, newTileType);
                    currentPreview.gridPreview[x, y].SetCategoryAndLabel(currentPreview.gridPreview[x, y].GetCategory(), newCategoryName);

                }
                else
                { 
                    bucketFill(x, y, newTileType, newCategoryName);
                }
                break;
            case 1:
                GridTools.WallInfo newWall = new GridTools.WallInfo();
                newWall.wallType = (Wall.wallType) wallTypeDropdown.value;
                if(newWall.wallType != 0)
                {
                    newWall.wallFront = (Wall.wallTexture) frontFaceDropdown.value;
                    newWall.wallBack = (Wall.wallTexture) backFaceDropdown.value;
                    newWall.wallInside = (Wall.wallInsideTexture) insideDropdown.value;
                    switch (wallDirectionDropdown.value)
                    {
                        case 0:
                            newWall.isFacingOutside = false;
                            newWall.isVertical = false;
                            newWall.x = x;
                            newWall.y = y + 1;
                            currentMap.horizontalWalls[newWall.x, newWall.y] = newWall;
                            break;
                        case 1:
                            newWall.isFacingOutside = true;
                            newWall.isVertical = false;
                            newWall.x = x;
                            newWall.y = y;
                            currentMap.horizontalWalls[newWall.x, newWall.y] = newWall;
                            break;
                        case 2:
                            newWall.isFacingOutside = false;
                            newWall.isVertical = true;
                            newWall.x = x;
                            newWall.y = y;
                            currentMap.verticalWalls[newWall.x, newWall.y] = newWall;
                            break;
                        default:
                            newWall.isFacingOutside = true;
                            newWall.isVertical = true;
                            newWall.x = x + 1;
                            newWall.y = y;
                            currentMap.verticalWalls[newWall.x, newWall.y] = newWall;
                            break;
                    }
                    
                    bool alreadyExist = false;
                    if(!newWall.isVertical)
                    {
                        if(currentPreview.horizontalWalls[newWall.x, newWall.y])
                        {
                            if(currentPreview.horizontalWalls[newWall.x, newWall.y].getWallType() == newWall.wallType)
                            {
                                alreadyExist = true;
                            }
                            else
                            {
                                currentPreview.horizontalWalls[newWall.x, newWall.y].destroyWall();
                            }
                        }
                    }
                    else
                    {
                        if (currentPreview.verticalWalls[newWall.x, newWall.y])
                        {
                            if (currentPreview.verticalWalls[newWall.x, newWall.y].getWallType() == newWall.wallType)
                            {
                                alreadyExist = true;
                            }
                            else
                            {
                                currentPreview.verticalWalls[newWall.x, newWall.y].destroyWall();
                            }
                        }
                    }

                    if(! alreadyExist)
                    {
                        GameObject choosedWallPrefab;// wall type prefabs
                        switch(newWall.wallType)
                        {
                            case Wall.wallType.frame:
                                if(newWall.isVertical)
                                {
                                    choosedWallPrefab = DoorFrameY;
                                }
                                else
                                {
                                    choosedWallPrefab = DoorFrameX;
                                }
                                break;
                            case Wall.wallType.window:
                                if (newWall.isVertical)
                                {
                                    choosedWallPrefab = WindowFrameY;
                                }
                                else
                                {
                                    choosedWallPrefab = WindowFrameX;
                                }
                                break;
                            case Wall.wallType.wall:
                                if (newWall.isVertical)
                                {
                                    choosedWallPrefab = WallY;
                                }
                                else
                                {
                                    choosedWallPrefab = WallX;
                                }
                                break;
                            case Wall.wallType.halfwall:
                                if (newWall.isVertical)
                                {
                                    choosedWallPrefab = HalfWallY;
                                }
                                else
                                {
                                    choosedWallPrefab = HalfWallX;
                                }
                                break;
                            default:
                                choosedWallPrefab = WallX;
                                break;
                        }
                        GameObject spawnedWall;
                        if (newWall.isVertical)
                        {
                            spawnedWall = Instantiate(choosedWallPrefab, new Vector3(newWall.x - newWall.y * 0.5f - 0.5f, newWall.y + 0.5f), Quaternion.identity);
                            currentPreview.verticalWalls[newWall.x, newWall.y] = spawnedWall.GetComponent<Wall>();
                            currentPreview.verticalWalls[newWall.x, newWall.y].setWallInfo(newWall);
                            currentPreview.verticalWalls[newWall.x, newWall.y].setLayer(currentMap.width, currentMap.height);
                        }
                        else
                        {
                            spawnedWall = Instantiate(choosedWallPrefab, new Vector3(newWall.x - newWall.y * 0.5f + 0.25f, newWall.y), Quaternion.identity);
                            currentPreview.horizontalWalls[newWall.x, newWall.y] = spawnedWall.GetComponent<Wall>();
                            currentPreview.horizontalWalls[newWall.x, newWall.y].setWallInfo(newWall);
                            currentPreview.horizontalWalls[newWall.x, newWall.y].setLayer(currentMap.width, currentMap.height);
                        }
                    }
                    else
                    {
                        if (newWall.isVertical)
                        {
                            currentPreview.verticalWalls[newWall.x, newWall.y].setWallInfo(newWall);
                        }
                        else
                        {
                            currentPreview.horizontalWalls[newWall.x, newWall.y].setWallInfo(newWall);
                        }
                    }
                }
                else
                {
                    int _x, _y;
                    bool _horizontal;
                    switch (wallDirectionDropdown.value)
                    {
                        case 0:
                            _horizontal = true;
                            _x = x;
                            _y = y + 1;
                            break;
                        case 1:
                            _horizontal = true;
                            _x = x;
                            _y = y;
                            break;
                        case 2:
                            _horizontal = false;
                            _x = x;
                            _y = y;
                            break;
                        default:
                            _horizontal = false;
                            _x = x + 1;
                            _y = y;
                            break;
                    }
                    if(_horizontal)
                    {
                        if (currentMap.horizontalWalls[_x, _y].HasValue)
                        {
                            currentMap.horizontalWalls[_x, _y] = null;
                            currentPreview.horizontalWalls[_x, _y].destroyWall();
                        }
                    }
                    else
                    {
                        if (currentMap.verticalWalls[_x, _y].HasValue)
                        {
                            currentMap.verticalWalls[_x, _y] = null;
                            currentPreview.verticalWalls[_x, _y].destroyWall();
                        }
                    }
                }
                WallsVisibility();
                break;
            case 2:
                switch(spawnsDropdown.value)
                {
                    case 0:
                        if(Int32.Parse(spawnTypeInput.text) == 0)
                        {
                            int numberOfCurrentSpawn = findSpawnNumberWithCoordinates(x, y, true);
                            if(numberOfCurrentSpawn != 0)
                            {
                                if (currentMap.spawnsBlue[numberOfCurrentSpawn - 1].HasValue)
                                {
                                    currentMap.spawnsBlue[numberOfCurrentSpawn - 1] = null;
                                    Destroy(currentPreview.blueSpawns[numberOfCurrentSpawn - 1]);
                                }
                            }
                        }
                        else
                        {
                            if(validateSpawnCoords(x, y))
                            {
                                if(currentPreview.blueSpawns[Int32.Parse(spawnTypeInput.text) - 1])
                                {
                                    Destroy(currentPreview.blueSpawns[Int32.Parse(spawnTypeInput.text) - 1]);
                                }
                                currentMap.spawnsBlue[Int32.Parse(spawnTypeInput.text) - 1] = null;
                                currentMap.spawnsBlue[Int32.Parse(spawnTypeInput.text) - 1] = new GridTools.TileCoordinates(x, y);
                                GameObject currentMarker = Instantiate(SpawnMarker, new Vector3(x - y * 0.5f, y), Quaternion.identity);
                                currentMarker.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(currentMarker.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), "Blue");
                                currentMarker.GetComponentInChildren<TMP_Text>().text = spawnTypeInput.text;
                                currentPreview.blueSpawns[Int32.Parse(spawnTypeInput.text) - 1] = currentMarker;
                            }
                        }
                        break;
                    case 1:
                        if (Int32.Parse(spawnTypeInput.text) == 0)
                        {
                            int numberOfCurrentSpawn = findSpawnNumberWithCoordinates(x, y, false);
                            if(numberOfCurrentSpawn != 0)
                            {
                                if (currentMap.spawnsRed[numberOfCurrentSpawn - 1].HasValue)
                                {
                                    currentMap.spawnsRed[numberOfCurrentSpawn - 1] = null;
                                    Destroy(currentPreview.redSpawns[numberOfCurrentSpawn - 1]);
                                }
                            }
                        }
                        else
                        {
                            if (validateSpawnCoords(x, y))
                            {
                                if (currentPreview.redSpawns[Int32.Parse(spawnTypeInput.text) - 1])
                                {
                                    Destroy(currentPreview.redSpawns[Int32.Parse(spawnTypeInput.text) - 1]);
                                }
                                currentMap.spawnsRed[Int32.Parse(spawnTypeInput.text) - 1] = null;
                                currentMap.spawnsRed[Int32.Parse(spawnTypeInput.text) - 1] = new GridTools.TileCoordinates(x, y);
                                GameObject currentMarker = Instantiate(SpawnMarker, new Vector3(x - y * 0.5f, y), Quaternion.identity);
                                currentMarker.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().SetCategoryAndLabel(currentMarker.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>().GetCategory(), "Red");
                                currentMarker.GetComponentInChildren<TMP_Text>().text = spawnTypeInput.text;
                                currentPreview.redSpawns[Int32.Parse(spawnTypeInput.text) - 1] = currentMarker;
                            }
                        }
                        break;
                    default:
                        break;
                }
                break;
            case 3:
                switch (gameplayDropdown.value)
                {
                    case 0:
                        if(x > 0 && y > 0)
                        {
                            if (currentPreview.blueFlag)
                            {
                                Destroy(currentPreview.blueFlag);
                            }
                            { 
                                currentMap.gameplayObjects[0] = null;
                                currentMap.gameplayObjects[0] = new GridTools.TileCoordinates(x, y);
                                GameObject currentFlag = Instantiate(flagB, new Vector3(x - y * 0.5f - 0.25f, y - 0.5f), Quaternion.identity);
                                currentPreview.blueFlag = currentFlag;
                                currentFlag.GetComponent<Flag>().setLayer(x, y, currentMap.height, currentMap.width);
                            }
                        }
                        break;
                    case 1:
                        if (x > 0 && y > 0)
                        {
                            if (currentPreview.redFlag)
                            {
                                Destroy(currentPreview.redFlag);
                            }
                            {
                                currentMap.gameplayObjects[1] = null;
                                currentMap.gameplayObjects[1] = new GridTools.TileCoordinates(x, y);
                                GameObject currentFlag = Instantiate(flagR, new Vector3(x - y * 0.5f - 0.25f, y - 0.5f), Quaternion.identity);
                                currentPreview.redFlag = currentFlag;
                                currentFlag.GetComponent<Flag>().setLayer(x, y, currentMap.height, currentMap.width);
                            }
                        }
                        break;
                    case 4:

                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

    }
}
