using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using static Enumerators;

/// <summary>
/// Manages the UI of the game results screen
/// </summary>
public class GameResultUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI coinCountText;
    [SerializeField] private TextMeshProUGUI keyCollectedText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI totalText;
    [SerializeField] private TextMeshProUGUI statusText;

    /// <summary>
    /// Sets the behaviour of the play button
    /// </summary>
    public void PlayGame()
    {
        GameManager.Instance.StartGame();
    }

    /// <summary>
    /// Sets the behaviour of the open scoreboard button
    /// </summary>
    public void OpenScoreboard()
    {
        GameManager.Instance.LoadScoreboard();
    }

    /// <summary>
    /// Populates the game summary stats fields with data of the last played game from the GameManager
    /// </summary>
    /// <param name="didWin"></param>
    /// <param name="coinCount"></param>
    /// <param name="keyCollected"></param>
    /// <param name="timer"></param>
    /// <param name="rank"></param>
    /// <param name="total"></param>
    /// <param name="causeOfDeath"></param>
    public void SetStatValues(bool didWin,int coinCount, bool keyCollected, float timer, int rank, int total, CauseOfDeath causeOfDeath)
    {
        coinCountText.text = coinCount.ToString();
        keyCollectedText.text = keyCollected ? "yes" : "no";
        timerText.text = FormatTime(timer);
        rankText.text = rank.ToString();
        totalText.text = total.ToString() + " pts";
        if(didWin)
        {
            title.text = "Dungeon Completed"; 
            statusText.text = "";
        }
        else
        {
            title.text = "Game Over";
            statusText.text = GetRandomDeathMessage(causeOfDeath);
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

    private Dictionary<string, List<string>> deathMessages;

    /// <summary>
    /// Gets a random death message based on the cause of death of the player
    /// </summary>
    /// <param name="cause"></param>
    /// <returns></returns>
    private string GetRandomDeathMessage(CauseOfDeath cause)
    {
        if (deathMessages == null)
        {
            LoadDeathMessages();
        } 

        string causeKey = cause.ToString(); 

        if (deathMessages.ContainsKey(causeKey))
        {
            List<string> messages = deathMessages[Random.Range(0, 10) != 0 ? causeKey: "General"];
            return messages[Random.Range(0, messages.Count)];
        }

        return "Unknown cause of death.";
    }

    /// <summary>
    /// Loads the death messages from the JSON file where they are stored
    /// </summary>
    private void LoadDeathMessages()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "deathMessages.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            deathMessages = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json);
            Debug.Log($"Death Messages Loaded: {deathMessages.Count} entries.");
        }
        else
        {
            Debug.LogError("Death messages JSON file not found!");
            deathMessages = new Dictionary<string, List<string>>();
        }
    } 
}