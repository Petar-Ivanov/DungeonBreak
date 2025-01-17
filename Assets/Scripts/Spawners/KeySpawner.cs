using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the key spawning logic
/// </summary>
public class KeySpawner : MonoBehaviour
{

    [SerializeField] private GameObject keyPrefab;

    //private Transform[] spawnPoints;

    /// <summary>
    /// Creates a key at a valid spawn point
    /// </summary>
    /// <param name="validKeySpawnLocations"></param>
    /// <returns></returns>
    public Vector3 SpawnKey(List<Vector3> validKeySpawnLocations)
    {
        if (validKeySpawnLocations == null)
        {
            Debug.LogError("validKeySpawnLocations instance not found!");
            return Vector3.zero;
        }

        List<Vector3> validSpawnLocations = validKeySpawnLocations;
        int spawnLocationCount = validSpawnLocations.Count;

        if (spawnLocationCount == 0)
        {
            Debug.LogWarning("No valid spawn locations available!");
            return Vector3.zero;
        }

        // Pick a random valid spawn point
        int randomIndex = Random.Range(0, spawnLocationCount);
        Vector3 spawnLocation = validSpawnLocations[randomIndex];

        // Spawn the coin
        Instantiate(keyPrefab, spawnLocation, Quaternion.identity);

        return spawnLocation;
    }
}
