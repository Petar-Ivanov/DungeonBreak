using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the npc spawning logic
/// </summary>
public class NPCSpawner : MonoBehaviour
{

    GridManager gridManager;

    [SerializeField] private List<GameObject> npcPrefabs = new List<GameObject>();

    //public Transform[] spawnPoints;

    [SerializeField] private int npcCount = 3;

    /// <summary>
    /// The navigation grid which the spawned npcs are going to use
    /// </summary>
    public GridManager GridManager
    {
        //get { return this.gridManager; }
        set { this.gridManager = value; }
    }

    /// <summary>
    /// Creates new npcs at valid spawn points
    /// </summary>
    /// <param name="validKeySpawnLocations"></param>
    /// <returns></returns>
    public List<Vector3> SpawnNPCs(List<Vector3> validKeySpawnLocations)
    {
        if (validKeySpawnLocations == null)
        {
            Debug.LogError("validKeySpawnLocations instance not found!");
            return null;
        }

        List<Vector3> validSpawnLocations = validKeySpawnLocations;
        int spawnLocationCount = validSpawnLocations.Count;
        int npcTypeCount = npcPrefabs.Count;

        if (spawnLocationCount == 0)
        {
            Debug.LogWarning("No valid spawn locations available!");
            return null;
        }

        List<Vector3> spawnLocations = new List<Vector3>();

        for (int i = 0; i < npcCount; i++)
        {
            // Pick a random valid spawn point
            int randomIndex = Random.Range(0, spawnLocationCount);
            Vector3 spawnLocation = validSpawnLocations[randomIndex];
            spawnLocations.Add(spawnLocation);

            int randomNPCType = Random.Range(0, npcTypeCount);

            GameObject spawnedNPC = Instantiate(npcPrefabs[randomNPCType], spawnLocation, Quaternion.identity);
            GhostController npcController = spawnedNPC.GetComponent<GhostController>();
            npcController.GridManager = gridManager;
            
        }

        return spawnLocations;
    }
}
