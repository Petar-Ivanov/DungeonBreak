using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using static Enumerators;

/// <summary>
/// A class responsible for procedurally generating and rendering dungeons, finding suitable spawn locations for the player, the npcs, the traps and the pickups 
/// </summary>
public class DungeonGenerator : MonoBehaviour
{

    public Tilemap WallTilemap;
    public Tilemap GroundTilemap;
    [SerializeField] private TileBase doorTile;
    [SerializeField] private TileBase horizontalWallTile;
    [SerializeField] private TileBase verticalWallTile;
    [SerializeField] private TileBase floorTile;
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 20;

    [HideInInspector]
    public List<Vector3> validPlayerSpawnLocations = new List<Vector3>();

    [HideInInspector]
    public List<Vector3> validKeySpawnLocations = new List<Vector3>();

    [HideInInspector]
    public List<Vector3> validPickupSpawnLocations = new List<Vector3>();

    public Vector2Int doorLocation;

    private WallTile[,] wallGrid;
    private GroundTile[,] groundGrid;

    /// <summary>
    /// A struct used to represent a tile during the Hunt and Kill algorithm
    /// </summary>
    private struct Cell
    {
        public bool visited;
        public bool linkUp, linkDown, linkLeft, linkRight;
    }

    /// <summary>
    /// A struct used to store the connections generated during the Hunt and Kill algorithm
    /// </summary>
    private struct Link
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;

        public Link(int x1, int y1, int x2, int y2)
        {
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
    }

    /// <summary>
    /// Generates a dungeon, renders it and then returns the grid of walls to be passed to the GridManager for pathfinding and navigation
    /// </summary>
    /// <returns></returns>
    public WallTile[,] SpawnDungeon()
    {
        GenerateDungeon();
        RenderDungeon();

        return wallGrid;
    }

    /// <summary>
    /// Generates the entire dungeon layout and finds all types of valid spawn points
    /// </summary>
    void GenerateDungeon()
    {
        
        GenerateGround();
        GenerateWalls();

        FindValidItemSpawnPoints();
    }

    /// <summary>
    /// Generates the wall tileset of the dungeon.
    /// Assigns the tiles from the links generated during Hunt and Kill as empty tiles. 
    /// Connects the tiles of every link pair by turning the tile between them into an empty tile as well.
    /// All other tiles are made into walls.
    /// </summary>
    void GenerateWalls()
    {
        List<Link> links = HuntAndKill(width, height);

        wallGrid = new WallTile[width, height];

        foreach (Link link in links)
        {
            wallGrid[link.x1, link.y1] = WallTile.None;
            wallGrid[link.x2, link.y2] = WallTile.None;

            if (link.x1 == link.x2)
            {
                wallGrid[link.x1, (link.y1 + link.y2) / 2] = WallTile.None;
            }
            else
            {
                wallGrid[(link.x1 + link.x2) / 2, link.y1] = WallTile.None;
            }
        }

        wallGrid = AddDoor(wallGrid);
    }

    /// <summary>
    /// Generates the ground tileset of the dungeon
    /// </summary>
    void GenerateGround()
    {
        groundGrid = new GroundTile[width, height];
    }

    /// <summary>
    /// The Hunt and Kill algorithm is a simplified version of Depth First Search that is used to generate maze-like dungeon paths
    /// It first creates its own smaller tilemap, then connects all tiles in a random way and returns a list of links
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    List<Link> HuntAndKill(int width, int height)
    {
        int widthIn = Mathf.FloorToInt(width / 2);
        int heightIn = Mathf.FloorToInt(height / 2);

        Cell[,] tilemap = new Cell[widthIn, heightIn];
        List<Link> links = new List<Link>();

        int x = UnityEngine.Random.Range(0, widthIn);
        int y = UnityEngine.Random.Range(0, heightIn);
        int visitedCount = 0;

        int safetyCounter = 0;
        int maxIterations = widthIn * heightIn * 10;

        while (visitedCount < widthIn * heightIn)
        {
            if (safetyCounter++ > maxIterations)
            {

                Debug.LogError("Infinite loop detected in HuntAndKill. Terminating...");
                break;
            }

            Cell currentCell = tilemap[x, y];

            if (!currentCell.visited)
            {
                currentCell.visited = true;
                visitedCount++;

                tilemap[x, y] = currentCell;
                FindValidPlayerKeySpawnPoints(x, y, visitedCount, widthIn, heightIn);
            }


            if (HasAvailableDirection(x, y, tilemap))
            {

                Directions[] directions = { Directions.Up, Directions.Down, Directions.Left, Directions.Right };
                directions = directions.OrderBy(x => UnityEngine.Random.value).ToArray();
                bool directionFound = false;

                foreach (Directions direction in directions)
                {
                    if (TryLinkCell(direction, ref x, ref y, tilemap, links))
                    {
                        directionFound = true;
                        break;
                    }
                }

                if (directionFound)
                    continue;
            }
            else
            {

                if (visitedCount < widthIn * heightIn)
                {
                    Hunt(ref x, ref y, widthIn, heightIn, tilemap);
                }
                else
                {
                    break;
                }
            }
        }

        links = AdjustIndexes(links);
        return links;
    }

    /// <summary>
    /// Converts an index from the Hunt and Kill algorithm into a grid index
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    int AdjustIndex(int index)
    {
        return 2 * index + 1;
    }

    /// <summary>
    /// Converts the coordinates of 2 tiles from the Hunt and Kill algorithm into grid coordinates
    /// </summary>
    /// <param name="links"></param>
    /// <returns></returns>
    List<Link> AdjustIndexes(List<Link> links)
    {
        for(int i = 0; i < links.Count; i++)
        {
            Link temp = links[i];

            temp.x1 = AdjustIndex(temp.x1);
            temp.y1 = AdjustIndex(temp.y1);

            temp.x2 = AdjustIndex(temp.x2);
            temp.y2 = AdjustIndex(temp.y2);

            links[i] = temp;
        }
        return links;
    }

    /// <summary>
    /// Randomly turns a reachable wall tile somewhere along the edge of the dungeon into a door
    /// </summary>
    /// <param name="tilemap"></param>
    /// <returns></returns>
    WallTile[,] AddDoor(WallTile[,] tilemap)
    {
        Directions[] directions = { Directions.Up, Directions.Down, Directions.Left, Directions.Right };
        Directions direction = directions.OrderBy(x => UnityEngine.Random.value).ToArray()[0];
        int index;

        switch (direction)
        {
            case Directions.Up:
                do
                {
                    index = UnityEngine.Random.Range(0, width);
                }
                while (tilemap[index, 1] != WallTile.None);
                tilemap[index, 0] = WallTile.Door;
                doorLocation = new Vector2Int(index, 0);
                break;
            case Directions.Down:
                do
                {
                    index = UnityEngine.Random.Range(0, width);
                }
                while (tilemap[index, height - 2] != WallTile.None);
                tilemap[index, height - 1] = WallTile.Door;
                doorLocation = new Vector2Int(index, height - 1);
                break;
            case Directions.Left:
                do
                {
                    index = UnityEngine.Random.Range(0, height);
                }
                while (tilemap[1, index] != WallTile.None);
                tilemap[0, index] = WallTile.Door;
                doorLocation = new Vector2Int(0, index);
                break;
            case Directions.Right:
                do
                {
                    index = UnityEngine.Random.Range(0, height);
                }
                while (tilemap[width - 2, index] != WallTile.None);
                tilemap[width - 1, index] = WallTile.Door;
                doorLocation = new Vector2Int(width - 1, index);
                break;
        }
            
        return tilemap;
    }

    /// <summary>
    /// Checks if there are any unlinked directions for the current cell
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tilemap"></param>
    /// <returns></returns>
    bool HasAvailableDirection(int x, int y, Cell[,] tilemap)
    {
        var currentCell = tilemap[x, y];
        return (x > 0 && !currentCell.linkLeft && !tilemap[x - 1, y].visited) || (x < tilemap.GetLength(0) - 1 && !currentCell.linkRight && !tilemap[x + 1, y].visited) ||
               (y > 0 && !currentCell.linkUp && !tilemap[x, y - 1].visited) || (y < tilemap.GetLength(1) - 1 && !currentCell.linkDown && !tilemap[x, y + 1].visited);
    }

    /// <summary>
    /// Attempts to link the current cell to a neighboring one
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="tilemap"></param>
    /// <param name="links"></param>
    /// <returns></returns>
    bool TryLinkCell(Directions direction, ref int x, ref int y, Cell[,] tilemap, List<Link> links)
    {
        Cell currentCell = tilemap[x, y];
        Cell neighborCell;

        switch (direction)
        {
            case Directions.Up:
                if (y > 0 && !currentCell.linkUp && !tilemap[x, y - 1].visited)
                {
                    currentCell.linkUp = true;
                    tilemap[x, y] = currentCell;

                    neighborCell = tilemap[x, y - 1];
                    neighborCell.linkDown = true;
                    tilemap[x, y - 1] = neighborCell;

                    links.Add(new Link(x, y, x, y - 1));
                    y--;
                    return true;
                }
                break;

            case Directions.Down:
                if (y < tilemap.GetLength(1) - 1 && !currentCell.linkDown && !tilemap[x, y + 1].visited)
                {
                    currentCell.linkDown = true;
                    tilemap[x, y] = currentCell;

                    neighborCell = tilemap[x, y + 1];
                    neighborCell.linkUp = true;
                    tilemap[x, y + 1] = neighborCell;

                    links.Add(new Link(x, y, x, y + 1));
                    y++;
                    return true;
                }
                break;

            case Directions.Left:
                if (x > 0 && !currentCell.linkLeft && !tilemap[x - 1, y].visited)
                {
                    currentCell.linkLeft = true;
                    tilemap[x, y] = currentCell;

                    neighborCell = tilemap[x - 1, y];
                    neighborCell.linkRight = true;
                    tilemap[x - 1, y] = neighborCell;

                    links.Add(new Link(x, y, x - 1, y));
                    x--;
                    return true;
                }
                break;

            case Directions.Right:
                if (x < tilemap.GetLength(0) - 1 && !currentCell.linkRight && !tilemap[x + 1, y].visited)
                {
                    currentCell.linkRight = true;
                    tilemap[x, y] = currentCell;

                    neighborCell = tilemap[x + 1, y];
                    neighborCell.linkLeft = true;
                    tilemap[x + 1, y] = neighborCell;

                    links.Add(new Link(x, y, x + 1, y));
                    x++;
                    return true;
                }
                break;
        }

        return false;
    }

    /// <summary>
    /// Hunt function that finds a visited cell with unlinked directions
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="widthIn"></param>
    /// <param name="heightIn"></param>
    /// <param name="tilemap"></param>
    void Hunt(ref int x, ref int y, int widthIn, int heightIn, Cell[,] tilemap)
    {
        for (int newX = 0; newX < widthIn; newX++)
        {
            for (int newY = 0; newY < heightIn; newY++)
            {
                if (tilemap[newX, newY].visited && HasAvailableDirection(newX, newY, tilemap))
                {
                    x = newX;
                    y = newY;
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Locates all points within the generated grid that are suitable for spawning pickup items and traps
    /// </summary>
    public void FindValidItemSpawnPoints()
    {
        validPickupSpawnLocations.Clear();

        for (int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if (wallGrid[x, y] == WallTile.None && groundGrid[x, y] == GroundTile.Floor)
                {
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);
                    Vector3 worldPosition = GroundTilemap.GetCellCenterWorld(tilePosition);

                    validPickupSpawnLocations.Add(worldPosition);
                }
            }
        }
    }

    /// <summary>
    /// Locates all points within the generated grid that are suitable for player spawn and all that are suitable for key spawn.
    /// Player spawn points - all points explored early in the Hunt and Kill generation algorithm.
    /// Key spawn points - all points explored late in the Hunt and Kill generation algorithm.
    /// Ensures that the player and the key are always spawned far enough from each other to keep the game challenging.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="iteration"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    void FindValidPlayerKeySpawnPoints(int x, int y, int iteration, int width, int height)
    {
        if (iteration <= 0)
        {
            validPlayerSpawnLocations.Clear();
            validKeySpawnLocations.Clear();
        }
        else if (iteration >= width * height * 0.8)
        {
            Vector3Int tilePosition = new Vector3Int(AdjustIndex(x), AdjustIndex(y), 0);
            Vector3 worldPosition = GroundTilemap.GetCellCenterWorld(tilePosition);

            validKeySpawnLocations.Add(worldPosition);
        }
        else if (iteration <= width * height * 0.4)
        {
            Vector3Int tilePosition = new Vector3Int(AdjustIndex(x), AdjustIndex(y), 0);
            Vector3 worldPosition = GroundTilemap.GetCellCenterWorld(tilePosition);

            validPlayerSpawnLocations.Add(worldPosition);
        }
    }

    /// <summary>
    /// Renders the entire dungeon
    /// </summary>
    void RenderDungeon()
    {
        RenderWalls();
        RenderGround();
    }

    /// <summary>
    /// Renders all wall tiles.
    /// Decides on the rotation of the wall (horizontal or vertical) based on its neighbors and on its own position.
    /// </summary>
    void RenderWalls()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                switch (wallGrid[x, y])
                {
                    case WallTile.Wall:
                        if (y == 0 || y == height - 1)
                        {
                            if(y == height - 1 && wallGrid[x, height - 2] == 0) WallTilemap.SetTile(tilePosition, verticalWallTile);
                            else WallTilemap.SetTile(tilePosition, horizontalWallTile);
                        }
                        else if (x == 0 || x == width - 1)
                        {
                            WallTilemap.SetTile(tilePosition, verticalWallTile);
                        }
                        else
                        {
                            if (
                                ((wallGrid[x + 1, y] == 0 || wallGrid[x - 1, y] == 0) && wallGrid[x, y - 1] != 0)
                                || (wallGrid[x + 1, y] != 0 && wallGrid[x - 1, y] != 0 && wallGrid[x, y + 1] == 0 && wallGrid[x, y - 1] != 0)
                                )
                            {
                                WallTilemap.SetTile(tilePosition, horizontalWallTile);
                            }
                            else
                            {
                                WallTilemap.SetTile(tilePosition, verticalWallTile);
                            }
                        }
                        break;
                    case WallTile.Door:
                        WallTilemap.SetTile(tilePosition, doorTile);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Renders all floor tiles
    /// </summary>
    void RenderGround()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                switch (groundGrid[x, y])
                { 
                    case GroundTile.Floor: 
                        GroundTilemap.SetTile(tilePosition, floorTile);
                        break;
                }
            }
        }
    }
}
