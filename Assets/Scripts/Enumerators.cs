using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains enumerators that are being used across multiple classes
/// </summary>
public class Enumerators : MonoBehaviour
{
    public enum CauseOfDeath
    {
        Ghost = 0,
        SpikeTrap = 1
    }

    public enum GameState
    {
        StartMenu = 0,
        Scoreboard = 1,
        GameOver = 2,
        Playing = 3
    }

    public enum Directions
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3
    }

    public enum WallTile
    {
        Wall = 0,
        None = 1,
        Door = 2
    }

    public enum GroundTile
    {
        Floor = 0
    }
}
