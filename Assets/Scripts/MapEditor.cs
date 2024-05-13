using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using TMPro;

public class MapEditor : MonoBehaviour
{
    public static MapEditor Instance;

    [SerializeField] private TMP_InputField widthInput, heightInput;
    [SerializeField] private GameObject NewMapWindow ,tilePrefab, currentSubMenu, tilesSubMenu, wallsSubMenu;
    [SerializeField] private GridManager.tileType currentType;
    private Map currentMap;
    private MapPreview currentPreviev;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private TMP_Dropdown editingModeDropdown, tileCategoryDropdown, wallDirectionDropdown, wallTypeDropdown, frontFaceDropdown, backFaceDropdown;

    public event EventHandler clearMap;

    private void Awake()
    {
        Instance = this;
        NewMapWindow.SetActive(false);
        tilesSubMenu.SetActive(false);
        wallsSubMenu.SetActive(false);
        ChooseEditingMode();
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
            default:
                break;
        }

    }

    public void ShowNewMapWindow()
    {
        NewMapWindow.SetActive(true);

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
    }

    public void CreateNewMap()
    {
        if((widthInput.text != null && heightInput.text != null) && ((Int32.Parse(widthInput.text) >= 8 && Int32.Parse(widthInput.text) <= 50) && (Int32.Parse(heightInput.text) >= 8 && Int32.Parse(heightInput.text) <= 50)))
        {
            ResetMap();
            currentMap = new Map(Int32.Parse(widthInput.text), Int32.Parse(heightInput.text));
            currentPreviev = new MapPreview(Int32.Parse(widthInput.text), Int32.Parse(heightInput.text));
            NewMapWindow.SetActive(false);
            GenerateNewMap();
        }
    }


    public void ResetMap()
    {
        if (clearMap != null)
        {
            clearMap(this, EventArgs.Empty);
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

                currentMap.tileGrid[x,y] = new Tile(x, y, GridManager.tileType.blank);
                currentPreviev.gridPreview[x, y] = spawnedTile.GetComponent<UnityEngine.U2D.Animation.SpriteResolver>();

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

    public void OnTileClick(int x, int y)
    {
        Debug.Log(x + " " + y);
        switch (editingModeDropdown.value)
        {
            case 0:
                switch(tileCategoryDropdown.value)
                {
                    case 0:
                        currentMap.tileGrid[x, y] = new Tile(x, y, GridManager.tileType.blank);
                        currentPreviev.gridPreview[x, y].SetCategoryAndLabel(currentPreviev.gridPreview[x, y].GetCategory(), "Blank");
                        break;
                    case 1:
                        currentMap.tileGrid[x, y] = new Tile(x, y, GridManager.tileType.cobblestone);
                        currentPreviev.gridPreview[x, y].SetCategoryAndLabel(currentPreviev.gridPreview[x, y].GetCategory(), "Cobblestone");
                        break;
                    case 2:
                        currentMap.tileGrid[x, y] = new Tile(x, y, GridManager.tileType.sand);
                        currentPreviev.gridPreview[x, y].SetCategoryAndLabel(currentPreviev.gridPreview[x, y].GetCategory(), "Sand");
                        break;
                    case 3:
                        currentMap.tileGrid[x, y] = new Tile(x, y, GridManager.tileType.sandRoad);
                        currentPreviev.gridPreview[x, y].SetCategoryAndLabel(currentPreviev.gridPreview[x, y].GetCategory(), "SandRoad");
                        break;
                    case 4:
                        currentMap.tileGrid[x, y] = new Tile(x, y, GridManager.tileType.woodenFloor1);
                        currentPreviev.gridPreview[x, y].SetCategoryAndLabel(currentPreviev.gridPreview[x, y].GetCategory(), "WoodFloor1");
                        break;
                    default:
                        break;
                }
                break;
            case 1:
                Wall newWall = new Wall();
                newWall.wallType = (GridManager.wallType) wallTypeDropdown.value;
                newWall.wallFront = (GridManager.wallTexture) frontFaceDropdown.value;
                newWall.wallBack = (GridManager.wallTexture) backFaceDropdown.value;
                switch (wallDirectionDropdown.value)
                {
                    case 0:
                        newWall.isFacingOutside = false;
                        currentMap.horizontalWalls[x, y + 1] = newWall;
                        break;
                    case 1:
                        newWall.isFacingOutside = true;
                        currentMap.horizontalWalls[x, y] = newWall;
                        break;
                    case 2:
                        newWall.isFacingOutside = false;
                        currentMap.verticalWalls[x + 1, y] = newWall;
                        break;
                    case 3:
                        newWall.isFacingOutside = true;
                        currentMap.verticalWalls[x, y] = newWall;
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

    }

    private struct MapPreview
    {
        public UnityEngine.U2D.Animation.SpriteResolver[,] gridPreview;
        public Wall[,] verticalWalls, horizontalWalls;
        public int height, width;
        public MapPreview(int width, int height)
        {
            this.width = width;
            this.height = height;
            gridPreview = new UnityEngine.U2D.Animation.SpriteResolver[width, height];
            verticalWalls = new Wall[width + 1, height];
            horizontalWalls = new Wall[width, height + 1];
        }
    }

    public struct Map
    {
        public int width, height;
        //public GridManager.TileCoordinates spawnB, spawnR, flagB, flagR;
        public Tile[,] tileGrid;
        public Wall[,] verticalWalls, horizontalWalls;
        public Map(int width, int height)
        {
            this.width = width;
            this.height = height;
            tileGrid = new Tile[width, height];
            verticalWalls = new Wall[width + 1, height];
            horizontalWalls = new Wall[width, height + 1];
        }
    }

    public struct Tile
    {
        public int x, y;
        public GridManager.tileType tileType;
        public Tile (int x, int y, GridManager.tileType tileType)
        {
            this.x = x;
            this.y = y;
            this.tileType = tileType;
        }
    }

    public struct Wall
    {
        public GridManager.wallType wallType;
        public GridManager.wallTexture wallFront, wallBack;
        public int x, y;
        //bool isVertical;
        public bool isFacingOutside;
    }
}
