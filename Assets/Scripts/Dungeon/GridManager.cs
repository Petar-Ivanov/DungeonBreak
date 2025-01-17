using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Enumerators;

/// <summary>
/// Manages the navigation, path finding and detection of the dungeon.
/// Has its own simple normalised coordinate system for easier management of operations.
/// </summary>
public class GridManager : MonoBehaviour
{
    /// <summary>
    /// Grid of wall tiles and empty walkable spaces between them
    /// </summary>
    private WallTile[,] grid;
    
    public int gridWidth => grid.GetLength(0);
    public int gridHeight => grid.GetLength(1);

    private Vector3 gridOrigin;
    public Vector2 cellSize;

    private Vector2Int playerLocation = Vector2Int.zero;
    public Vector2Int doorLocation = Vector2Int.zero;

    /// <summary>
    /// Initializes the GridManager.
    /// Loads the grid, its coordinates in the world and the size of the cells
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="gridOrigin"></param>
    /// <param name="cellSize"></param>
    public void SetGrid(WallTile[,] grid, Vector3 gridOrigin, Vector2 cellSize)
    {
        this.grid = grid;
        this.gridOrigin = gridOrigin;
        this.cellSize = cellSize;
    }

    /// <summary>
    /// Sets the player position in the grid based on its world coordinates
    /// </summary>
    /// <param name="playerWorldPosition"></param>
    public void SetPlayerLocation(Vector2Int playerWorldPosition)
    {
        this.playerLocation = GetGridCoordinates(playerWorldPosition);
    }

    /// <summary>
    /// Checks whether the player is no more than 1 tile away from a door
    /// </summary>
    /// <returns></returns>
    public bool IsPlayerNearDoor()
    {
        if(playerLocation != Vector2Int.zero && doorLocation != Vector2Int.zero && Math.Abs(playerLocation.x - doorLocation.x) <= 1 && Math.Abs(playerLocation.y - doorLocation.y) <= 1) return true;
        return false;
    }

    /// <summary>
    /// Gets a pair of world coordinates and returns a pair of corresponding grid coordinates
    /// </summary>
    /// <param name="worldPosition"></param>
    /// <returns></returns>
    public Vector2Int GetGridCoordinates(Vector2Int worldPosition)
    {
        // Calculate the cell coordinates
        int cellX = Mathf.CeilToInt((worldPosition.x - gridOrigin.x) / cellSize.x);
        int cellY = Mathf.FloorToInt((worldPosition.y - gridOrigin.y) / cellSize.y);

        Vector2Int cellPosition = new Vector2Int(cellX, cellY);

        return cellPosition;
    }

    /// <summary>
    /// Gets a pair of grid coordinates and returns a pair of corresponding world coordinates
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
    public Vector2Int GetWorldCoordinates(Vector2Int gridPosition)
    {
        int worldX = Mathf.FloorToInt(gridPosition.x * cellSize.x + gridOrigin.x);
        int worldY = Mathf.FloorToInt(gridPosition.y * cellSize.y + gridOrigin.y);

        Vector2Int worldPosition = new Vector2Int(worldX, worldY);

        return worldPosition;
    }

    /// <summary>
    /// Returns the player's grid coordinates
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetPlayerLocation()
    {
        return this.playerLocation;
    }

    /// <summary>
    /// Returns the player's world coordinates
    /// </summary>
    /// <returns></returns>
    public Vector2Int GetPlayerWorldLocation()
    {
        return GetWorldCoordinates(this.playerLocation);
    }

    /// <summary>
    /// Determines whether the player is within the line of sight of a specific world position.
    /// Converts the world position into a tile.
    /// Performs a raycast trying to find a player that collides with the rays.
    /// </summary>
    /// <param name="startWorldPosition"></param>
    /// <returns></returns>
    public bool DetectPlayer(Vector2Int startWorldPosition)
    {
        if (playerLocation == Vector2Int.zero)
        {
            return false;
        }

        Vector2Int startCoordinates = GetGridCoordinates(startWorldPosition);
        int x = startCoordinates.x;
        int y = startCoordinates.y;

        bool playerDetected = false;

        for (int i = 0; i < 4; i++)
        {
            playerDetected = Raycast(x, y, (Directions)i);
            if (playerDetected) return true;
        }

        return false;
    }

    /// <summary>
    /// Explores the subsequent tiles from a specific starting point in the grid in all 4 cardinal directions until it reaches a wall tile.
    /// If a ray lands on a tile with coordinates equal to those of the player before reaching a wall the player is detected.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    private bool Raycast(int x, int y, Directions direction)
    {
        while (true)
        {
            if (x < 1 || y < 1 || x >= grid.GetLength(0) - 1 || y >= grid.GetLength(1) - 1)
                break;
            if (grid[x, y] == WallTile.Wall)
                break;
            if (x == playerLocation.x && y == playerLocation.y)
                return true;
            switch (direction)
            {
                case Directions.Left:
                    x--;
                    break;
                case Directions.Right:
                    x++;
                    break;
                case Directions.Up:
                    y++;
                    break;
                case Directions.Down:
                    y--;
                    break;
            }
        }

        return false;
    }

    /// <summary>
    /// An object representing a position within the grid during the A* search.
    /// Has coordinates and a parent node.
    /// G - cost of the path from the start node to the current node.
    /// H - approximation of the cost for moving from the current node to the end node using Manhattan distance formula.
    /// </summary>
    private class PathNode
    {
        public Vector2Int Position;
        public PathNode Parent;
        public int G, H;
        public int F => G + H;

        public PathNode(Vector2Int pos, PathNode parent, int g, int h)
        {
            Position = pos;
            Parent = parent;
            G = g;
            H = h;
        }
    }

    /// <summary>
    /// Returns an ordered list of tiles that is the shortest path between two positions within the grid.
    /// Performs A* search.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        if (!IsPositionValid(start) || !IsPositionValid(end))
            return new List<Vector2Int>();

        // Check if start or end is a wall
        if (grid[start.x, start.y] == WallTile.Wall || grid[end.x, end.y] == WallTile.Wall)
            return new List<Vector2Int>();

        var openSet = new List<PathNode>();
        var closedSet = new HashSet<Vector2Int>();
        openSet.Add(new PathNode(start, null, 0, ManhattanDistance(start, end)));

        while (openSet.Count > 0)
        {
            var current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
                if (openSet[i].F < current.F)
                    current = openSet[i];

            if (current.Position == end)
                return ReconstructPath(current);

            openSet.Remove(current);
            closedSet.Add(current.Position);

            foreach (var neighbor in GetWalkableNeighbors(current.Position))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                int newG = current.G + 1;

                PathNode neighborNode = openSet.Find(n => n.Position == neighbor);
                if (neighborNode == null)
                {
                    neighborNode = new PathNode(neighbor, current, newG, ManhattanDistance(neighbor, end));
                    openSet.Add(neighborNode);
                }
                else if (newG < neighborNode.G)
                {
                    neighborNode.Parent = current;
                    neighborNode.G = newG;
                }
            }
        }

        return new List<Vector2Int>().Skip(0).ToList(); // No path found
    }

    /// <summary>
    /// Checks wether a given pair of coordinates fit within the boundaries of the grid
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private bool IsPositionValid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) &&
               pos.y >= 0 && pos.y < grid.GetLength(1);
    }

    /// <summary>
    /// Returns which neighbors of the current tile are walkable.
    /// Walkable are all tiles that fit within the constraints of the grid and are not walls
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    private List<Vector2Int> GetWalkableNeighbors(Vector2Int pos)
    {
        var neighbors = new List<Vector2Int>();
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (var dir in directions)
        {
            Vector2Int newPos = pos + dir;
            if (IsPositionValid(newPos) && grid[newPos.x, newPos.y] != WallTile.Wall)
            {
                neighbors.Add(newPos);
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Evaluates how far away is vector "a" from vector "b" (coordinates of 2 tiles within the grid) in the sense of a direct pact between the two.
    /// Uses the Manhattan distance formula.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// Backtracks from the end position of the A* search, reconstructing the path as a list of ordered coordinate pairs (tiles)
    /// </summary>
    /// <param name="endNode"></param>
    /// <returns></returns>
    private List<Vector2Int> ReconstructPath(PathNode endNode)
    {
        var path = new List<Vector2Int>();
        var current = endNode;
        while (current != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }

}
