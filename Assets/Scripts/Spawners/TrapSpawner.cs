using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the trap spawning logic
/// </summary>
public class TrapSpawner : MonoBehaviour
{

    [SerializeField] private GameObject trapPrefab;

    [SerializeField] private int trapCount = 10;

    //[Header("Spawn Locations")]
    //public Transform[] spawnPoints;

    /// <summary>
    /// Creates new traps at valid spawn points
    /// </summary>
    /// <param name="validPickupSpawnLocations"></param>
    /// <param name="occupiedSpawnLocation"></param>
    public void SpawnTraps(List<Vector3> validPickupSpawnLocations, List<Vector3> occupiedSpawnLocation)
    {
        if (validPickupSpawnLocations == null)
        {
            Debug.LogError("validPickupSpawnLocations instance not found!");
            return;
        }

        List<Vector3> validSpawnLocations = validPickupSpawnLocations;

        foreach (Vector3 occupied in occupiedSpawnLocation)
        {
            validSpawnLocations.Remove(occupied);
        }

        List<int> usedIndexes = new List<int>();
        int spawnLocationCount = validSpawnLocations.Count;

        if (spawnLocationCount == 0)
        {
            Debug.LogWarning("No valid spawn locations available!");
            return;
        }

        int spawnCounter = 0;
        while (spawnCounter < trapCount && validSpawnLocations.Count > 0)
        {
            // Pick a random valid spawn point
            int randomIndex = UnityEngine.Random.Range(0, validSpawnLocations.Count);
            Vector3 spawnLocation = validSpawnLocations[randomIndex];

            // Spawn the coin
            Instantiate(trapPrefab, spawnLocation, Quaternion.identity);

            validSpawnLocations.RemoveAt(randomIndex);

            spawnCounter++;
        }
    }
}
