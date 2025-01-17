using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ScoreboardManager;

/// <summary>
/// Manages the UI of the scoreboard
/// </summary>
public class ScoreboardUIManager : MonoBehaviour
{
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private Transform scoreboardParent;

    /// <summary>
    /// Loads all runs into the scoreboard UI element
    /// </summary>
    /// <param name="runs"></param>
    public void ShowScoreboard(List<RunData> runs)
    {
        foreach (Transform child in scoreboardParent)
        {
            Destroy(child.gameObject);
        }

        int runIndex = 1;

        foreach (var run in runs.OrderByDescending(x=>x.score))
        {
            GameObject entry = Instantiate(entryPrefab, scoreboardParent);

            TMP_Text entryText = entry.GetComponentInChildren<TMP_Text>();
            if (entryText == null)
            {
                Debug.LogError("Entry Prefab does not have a TMP_Text component.");
                continue;
            }

            string gameStatusText = run.didWin ? "WIN" : "DEFEAT";
            string keyStatusText = run.keyCollected ? "YES" : "NO";
            entryText.text = $"# {runIndex++} - Score: {run.score} - Status: {gameStatusText} - Coins: {run.coinCount} - KEY: {keyStatusText} - Time: {FormatTime(run.time)}";
        }
    }

    /// <summary>
    /// Formats the GameManager timer as follows: 00:00
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Sets the behaviour of the play button
    /// </summary>
    public void PlayGame()
    { 
        GameManager.Instance.StartGame();
    }
}