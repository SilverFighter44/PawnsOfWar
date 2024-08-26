using System.Collections.Generic;
using UnityEngine;


public static class GridTools
    {

    [SerializeField] public static int gameplayObjectsCount = 2;

    public enum tileType { blank, cobblestone, sand, sandRoad, woodenFloor1 };

    [SerializeField] private static int layerMultiplier = 50;


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

    [System.Serializable]
    public struct MapPreview
    {
        public UnityEngine.U2D.Animation.SpriteResolver[,] gridPreview;
        public GameObject[] blueSpawns, redSpawns;
        public GameObject blueFlag, redFlag;
        public Wall[,] verticalWalls, horizontalWalls;
        public int height, width;
        public MapPreview(int width, int height, int teamSize)
        {
            this.width = width;
            this.height = height;
            gridPreview = new UnityEngine.U2D.Animation.SpriteResolver[width, height];
            verticalWalls = new Wall[width + 1, height];
            horizontalWalls = new Wall[width, height + 1];
            blueSpawns = new GameObject[teamSize];
            redSpawns = new GameObject[teamSize];
            blueFlag = null;
            redFlag = null;
        }
    }

    [System.Serializable]
    public struct Map
    {
        public bool[] modeCompatibility;
        public int width, height;
        public int? teamSize;
        public TileCoordinates?[] spawnsBlue, spawnsRed, gameplayObjects;
        public TileInfo[,] tileGrid;
        public WallInfo?[,] verticalWalls, horizontalWalls;
        public Map(int width, int height)
        {
            this.width = width;
            this.height = height;
            tileGrid = new TileInfo[width, height];
            verticalWalls = new WallInfo?[width + 1, height];
            horizontalWalls = new WallInfo?[width, height + 1];
            gameplayObjects = new TileCoordinates?[gameplayObjectsCount];
            teamSize = 4;   //default size
            spawnsBlue = new TileCoordinates?[teamSize.Value];
            spawnsRed = new TileCoordinates?[teamSize.Value];
            modeCompatibility = new bool[gameplayObjectsCount];
            for (int i = 0; i < modeCompatibility.Length; i++)
            {
                modeCompatibility[i] = false;
            }
        }
    }

    [System.Serializable]
    public struct MapIntermediate
    {
        public bool[] modeCompatibility;
        public int width, height;
        public int teamSize;
        public List<TileCoordinates> GameplayObjects;
        public List<TileInfo> Tiles;
        public List<WallInfo> Walls;
        public MapIntermediate(int width, int height, int teamSize)
        {
            this.width = width;
            this.height = height;
            Tiles = new List<TileInfo>();
            Walls = new List<WallInfo>();
            this.teamSize = teamSize;
            modeCompatibility = new bool[gameplayObjectsCount];
            for (int i = 0; i < modeCompatibility.Length; i++)
            {
                modeCompatibility[i] = false;
            }
            GameplayObjects = new List<TileCoordinates>();
        }
    }

    [System.Serializable]
    public struct TileInfo
    {
        public int x, y;
        public tileType tileType;
        public TileInfo(int x, int y, tileType tileType)
        {
            this.x = x;
            this.y = y;
            this.tileType = tileType;
        }
    }

    [System.Serializable]
    public struct WallInfo
    {
        public Wall.wallType wallType;
        public Wall.wallTexture wallFront, wallBack;
        public Wall.wallInsideTexture wallInside;
        public int x, y;
        public bool isVertical;
        public bool isFacingOutside;
        public WallInfo(Wall.wallType wallType, Wall.wallTexture wallFront, Wall.wallTexture wallBack, Wall.wallInsideTexture wallInside, int x, int y, bool isVertical, bool isFacingOutside)
        {
            this.wallType = wallType;
            this.wallFront = wallFront;
            this.wallBack = wallBack;
            this.wallInside = wallInside;
            this.x = x;
            this.y = y;
            this.isVertical = isVertical;
            this.isFacingOutside = isFacingOutside;
        }
    }

    public static int getLayerMultiplier()
    {
        return layerMultiplier;
    }

    public static int OnGridObjectLayer(int width, int height, int x, int y)
    {
        return (width + 1) * (height - y) + ((2 * width + 1) * layerMultiplier) * (height - 1 - y) + (2 * (width - x) - 2) * layerMultiplier;
        //return  (width + 1 + (2 * width + 1) * layerMultiplier) * (height - 1 - y) + width + 1 + ((1 + width - x) * 2) * layerMultiplier; //* (((MaxHeight - unitOnGridPositionX - 1) * MaxWidth + MaxWidth - unitOnGridPositionY - 2) + 2);
    }
    // object layer = (_width + 1 + (2 * _width + 1) * layerMultiplier) * (_height - 1 - y) + _width + 1 + ((1 + _width - x) * 2) * layerMultiplier

    public static MapIntermediate translateMapToIntermediate(Map map)
    {
        MapIntermediate mapIntermediate = new MapIntermediate(map.width, map.height, map.teamSize.Value);
        mapIntermediate.modeCompatibility = map.modeCompatibility;
        for (int i = 0; i < gameplayObjectsCount; i++)
        {
            if (mapIntermediate.modeCompatibility[i])
            {
                mapIntermediate.GameplayObjects.Add(map.gameplayObjects[i].Value);
            }
        }
        for (int i = 0; i < map.teamSize.Value; i++)
        {
            mapIntermediate.GameplayObjects.Add(map.spawnsBlue[i].Value);
        }
        for (int i = 0; i < map.teamSize.Value; i++)
        {
            mapIntermediate.GameplayObjects.Add(map.spawnsRed[i].Value);
        }
        for (int i = 0; i < mapIntermediate.width; i++)
        {
            for (int j = 0; j < mapIntermediate.height; j++)
            {
                mapIntermediate.Tiles.Add(map.tileGrid[i, j]);
            }
        }
        for (int i = 0; i < mapIntermediate.width + 1; i++)
        {
            for (int j = 0; j < mapIntermediate.height; j++)
            {
                if (map.verticalWalls[i, j].HasValue)
                {
                    mapIntermediate.Walls.Add(map.verticalWalls[i, j].Value);
                }
            }
        }
        for (int i = 0; i < mapIntermediate.width; i++)
        {
            for (int j = 0; j < mapIntermediate.height + 1; j++)
            {
                if (map.horizontalWalls[i, j].HasValue)
                {
                    mapIntermediate.Walls.Add(map.horizontalWalls[i, j].Value);
                }
            }
        }
        return mapIntermediate;
    }

    public static Map translateMapFromIntermediate(MapIntermediate mapIntermediate)
    {
        Map map = new Map(mapIntermediate.width, mapIntermediate.height);
        map.modeCompatibility = mapIntermediate.modeCompatibility;
        for (int i = 0; i < map.modeCompatibility.Length; i++)
        {
            if (map.modeCompatibility[i])
            {
                map.gameplayObjects[i] = mapIntermediate.GameplayObjects[0];
                mapIntermediate.GameplayObjects.RemoveAt(0);
            }
        }
        for (int i = 0; i < map.teamSize; i++)
        {
            map.spawnsBlue[i] = mapIntermediate.GameplayObjects[i];
            map.spawnsRed[i] = mapIntermediate.GameplayObjects[i + map.teamSize.Value];
        }
        for (int i = 0; i < mapIntermediate.Tiles.Count; i++)
        {
            int x = mapIntermediate.Tiles[i].x, y = mapIntermediate.Tiles[i].y;
            map.tileGrid[x, y].x = x;
            map.tileGrid[x, y].y = y;
            map.tileGrid[x, y].tileType = mapIntermediate.Tiles[i].tileType;
        }
        for (int i = 0; i < mapIntermediate.Walls.Count; i++)
        {
            int x = mapIntermediate.Walls[i].x, y = mapIntermediate.Walls[i].y;
            if (mapIntermediate.Walls[i].isVertical)
            {
                map.verticalWalls[x, y] = new WallInfo(mapIntermediate.Walls[i].wallType, mapIntermediate.Walls[i].wallFront, mapIntermediate.Walls[i].wallBack, mapIntermediate.Walls[i].wallInside, x, y, mapIntermediate.Walls[i].isVertical, mapIntermediate.Walls[i].isFacingOutside);
            }
            else
            {
                map.horizontalWalls[x, y] = new WallInfo(mapIntermediate.Walls[i].wallType, mapIntermediate.Walls[i].wallFront, mapIntermediate.Walls[i].wallBack, mapIntermediate.Walls[i].wallInside, x, y, mapIntermediate.Walls[i].isVertical, mapIntermediate.Walls[i].isFacingOutside);
            }

        }
        return map;
    }
}