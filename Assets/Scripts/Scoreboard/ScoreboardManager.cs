using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Used to handle the data of the scoreboard.
/// Stores and ranks the data of each run into the scoreboard.
/// Uses JSON.
/// </summary>
public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager Instance;

    [SerializeField] 
    private ScoreboardUIManager ScoreboardUIManager;

    /// <summary>
    /// Current working list of runs
    /// </summary>
    private List<RunData> runs = new List<RunData>();

    private bool runsLoaded = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load existing runs into memory
            LoadRuns();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Entity for storing the data of each run
    /// </summary>
    [System.Serializable]
    public class RunData
    {
        public bool didWin;
        public int coinCount;
        public bool keyCollected;
        public float time;
        public int score;

        public RunData(bool didWin, int coinCount, bool keyCollected, float time, int score)
        {
            this.didWin = didWin;
            this.coinCount = coinCount;
            this.keyCollected = keyCollected;
            this.time = time;
            this.score = score;
        }
    }

    /// <summary>
    /// Adds a new run to the list
    /// </summary>
    /// <param name="didWin"></param>
    /// <param name="coinCount"></param>
    /// <param name="keyCollected"></param>
    /// <param name="time"></param>
    /// <param name="score"></param>
    public void AddRun(bool didWin, int coinCount, bool keyCollected, float time, int score)
    {
        // Ensures no data loss
        LoadRuns();

        RunData newRun = new RunData(didWin, coinCount, keyCollected, time, score);
        runs.Add(newRun);

        SaveRuns();
    }

    /// <summary>
    /// Saves the current list of runs to the JSON file
    /// </summary>
    private void SaveRuns()
    {
        string json = JsonUtility.ToJson(new SaveData(runs));
        File.WriteAllText(Application.persistentDataPath + "/scoreboard.json", json);
    }

    /// <summary>
    /// Used for extracting data from the JSON file
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        public List<RunData> runs;

        public SaveData(List<RunData> runs)
        {
            this.runs = runs;
        }
    }

    /// <summary>
    /// Populates the list of runs from the JSON file that stores the runs persistently
    /// </summary>
    public void LoadRuns()
    {
        if (runsLoaded) return; // Prevent reloading
        string path = Application.persistentDataPath + "/scoreboard.json";
        Debug.Log("Persistent Data Path: " + Application.persistentDataPath);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            runs = data.runs ?? new List<RunData>();
        }
        else
        {
            runs = new List<RunData>();
        }

        runsLoaded = true; // Mark as loaded
    }

    /// <summary>
    /// Passes all run data to the scoreboard UI
    /// </summary>
    public void LoadScoreboard()
    {
        LoadRuns();

        if (ScoreboardUIManager == null)
        {
            ScoreboardUIManager = FindObjectOfType<ScoreboardUIManager>();
        }

        ScoreboardUIManager.ShowScoreboard(runs);
    }
}