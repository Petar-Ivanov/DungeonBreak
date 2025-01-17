using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the player spawning logic
/// </summary>
public class PlayerSpawner : MonoBehaviour
{

    GridManager gridManager;

    [SerializeField] private GameObject playerPrefab;

    //[Header("Spawn Locations")]
    //public Transform[] spawnPoints;

    /// <summary>
    /// The navigation grid which the spawned player is going to use
    /// </summary>
    public GridManager GridManager
    {
        //get { return this.gridManager; }
        set { this.gridManager = value; }
    }

    /// <summary>
    /// Creates a new player at a valid spawn point
    /// </summary>
    /// <param name="validPlayerSpawnLocations"></param>
    /// <returns></returns>
    public Vector3 SpawnPlayer(List<Vector3> validPlayerSpawnLocations)
    {
        if (validPlayerSpawnLocations == null)
        {
            Debug.LogError("validPlayerSpawnLocations instance not found!");
            return Vector3.zero; ;
        }

        List<Vector3> validSpawnLocations = validPlayerSpawnLocations;
        int spawnLocationCount = validSpawnLocations.Count;

        if (spawnLocationCount == 0)
        {
            Debug.LogWarning("No valid spawn locations available!");
            return Vector3.zero; ;
        }

        // Pick a random valid spawn point
        int randomIndex = Random.Range(0, spawnLocationCount);
        Vector3 spawnLocation = validSpawnLocations[randomIndex];
         
        GameObject spawnedPlayer = Instantiate(playerPrefab, spawnLocation, Quaternion.identity); 
        PlayerController playerController = spawnedPlayer.GetComponent<PlayerController>();
        playerController.GridManager = gridManager;

        return spawnLocation;
    }
}
