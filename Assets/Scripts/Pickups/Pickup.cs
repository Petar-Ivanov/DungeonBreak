using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Base class for all pickups.
/// Defines pickup effects and sounds, object destruction, and pickup collision
/// </summary>
public class Pickup : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private AudioSource pickupSound;

    [SerializeField] private Light2D pickupLight;

    [Header("Light Settings")]
    [SerializeField] private float maxLightIntensity = 10f;

    [SerializeField] private float flashDuration = 0.1f;

    private const string PlayerTag = "Player";

    protected virtual void Start()
    {
        if (pickupSound == null)
        {
            pickupSound = GetComponent<AudioSource>();
        }

        pickupLight = GetComponent<Light2D>();
        if (pickupLight != null)
        {
            pickupLight.enabled = false;
        }
    }

    /// <summary>
    /// Detects collision with the player and initializes specific pickup item behavior, flash, pickup sound and coin destruction
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(PlayerTag))
        {
            OnPickup();

            PlayPickupSound();
            StartCoroutine(LightFlash());
            StartCoroutine(DestroyPickup());
        }
    }

    /// <summary>
    /// Produces a flash at the location of the coin
    /// </summary>
    /// <returns></returns>
    private IEnumerator LightFlash()
    {
        if (pickupLight != null)
        {
            pickupLight.enabled = true;

            for (float t = 0; t < 1; t += Time.deltaTime / flashDuration)
            {
                pickupLight.intensity = Mathf.Lerp(0, maxLightIntensity, t);
                yield return null;
            }

            for (float t = 0; t < 1; t += Time.deltaTime / flashDuration)
            {
                pickupLight.intensity = Mathf.Lerp(maxLightIntensity, 0, t);
                yield return null;
            }

            pickupLight.enabled = false;
        }
    }

    /// <summary>
    /// Plays a pickup sound
    /// </summary>
    private void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            pickupSound.Play();
            Debug.Log("Pickup sound played!");
        }
        else
        {
            Debug.LogWarning("Pickup sound not available!");
        }
    }

    /// <summary>
    /// Destroys the pickup item
    /// </summary>
    /// <returns></returns>
    private IEnumerator DestroyPickup()
    {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    /// <summary>
    /// Virtual method used to define specific pickup item behaviour
    /// </summary>
    protected virtual void OnPickup()
    {

    }
}
