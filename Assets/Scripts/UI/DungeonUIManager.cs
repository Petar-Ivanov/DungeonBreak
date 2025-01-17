using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the in-game UI
/// </summary>
public class DungeonUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinCounterText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image keyIcon;
    [SerializeField] private Image unlockImage;
    [SerializeField] private TextMeshProUGUI unlockPrompt;

    private int coinCount = 0;
    private float timer = 0;

    void Update()
    { 
        timerText.text = FormatTime(timer);
    }

    /// <summary>
    /// Synchronises the timer of the UI with the timer of the GameManager
    /// </summary>
    /// <param name="time"></param>
    public void UpdateTimer(float time)
    {
        timer = time;
    }

    /// <summary>
    /// Synchronises coin count of the UI with the coin count of the GameManager
    /// </summary>
    /// <param name="newCount"></param>
    public void UpdateCoinCount(int newCount)
    {
        coinCount = newCount;
        coinCounterText.text = "x " + coinCount;
    }

    /// <summary>
    /// Sets the key icon oppacity based on wheather it is collected or not
    /// </summary>
    /// <param name="isCollected"></param>
    public void SetKeyCollected(bool isCollected)
    {
        Color iconColor = keyIcon.color;
        iconColor.a = isCollected ? 1f : 0.1f;
        keyIcon.color = iconColor;
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
    /// Sets the controll prompt for unlocking the door to either visible or invisible by adjusting its oppacity
    /// </summary>
    /// <param name="makeVisible"></param>
    public void SetPromptVisibility(bool makeVisible)
    { 
        Color color = unlockImage.color;
        color.a = makeVisible ? 1f : 0f;
        unlockImage.color = color;

        color = unlockPrompt.color;
        color.a = makeVisible ? 1f : 0f;
        unlockPrompt.color = color;
    }
}
