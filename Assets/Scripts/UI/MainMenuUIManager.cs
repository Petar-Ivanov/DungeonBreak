using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages the UI of the main menu
/// </summary>
public class MainMenuUIManager : MonoBehaviour
{
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
}
