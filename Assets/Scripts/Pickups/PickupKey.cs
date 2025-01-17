using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Defines a key pickup object and its behaviour
/// </summary>
public class PickupKey : Pickup
{
    /// <summary>
    /// Key pickup specific behavior
    /// </summary>
    protected override void OnPickup()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectKey();
            Debug.Log($"Key Collected: {GameManager.Instance.KeyCollected}");
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
    }
}
