using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the coin spawning logic
/// </summary>
public class CoinSpawner : MonoBehaviour
{

    [SerializeField] private GameObject coinPrefab;

    //private Transform[] spawnPoints;

    [SerializeField] private int coinCount = 10;


    /// <summary>
    /// Creates new coins at valid spawn points that are not already occupied
    /// </summary>
    /// <param name="validPickupSpawnLocations"></param>
    /// <param name="occupiedSpawnLocation"></param>
    public void SpawnCoins(List<Vector3> validPickupSpawnLocations, List<Vector3> occupiedSpawnLocation)
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
        while (spawnCounter < coinCount && validSpawnLocations.Count > 0)
        {
            // Pick a random valid spawn point
            int randomIndex = UnityEngine.Random.Range(0, validSpawnLocations.Count);
            Vector3 spawnLocation = validSpawnLocations[randomIndex];

            // Spawn the coin
            Instantiate(coinPrefab, spawnLocation, Quaternion.identity);

            validSpawnLocations.RemoveAt(randomIndex);

            spawnCounter++;
        }
    }
}
