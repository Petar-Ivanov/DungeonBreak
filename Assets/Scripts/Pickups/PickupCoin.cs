using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Defines a coin pickup object and its behaviour
/// </summary>
public class PickupCoin : Pickup
{
    /// <summary>
    /// Coin pickup specific behavior
    /// </summary>
    protected override void OnPickup()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin();
            Debug.Log($"Coin added. Total: {GameManager.Instance.CoinsCollected}");
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
    }

    
}
