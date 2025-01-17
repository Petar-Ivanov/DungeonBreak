using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player input.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static Vector2 Movement;
    public static bool UnlockKeyPressed;

    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private InputAction _unlockAction;

    /// <summary>
    /// Setup 
    /// </summary>
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        _moveAction = _playerInput.actions["Move"];

        _unlockAction = _playerInput.actions["Unlock"];
        _unlockAction.performed += OnUnlockPerformed;
    }

    /// <summary>
    /// Detects movement input
    /// </summary>
    void Update()
    {
        Movement = _moveAction.ReadValue<Vector2>();
    }

    /// <summary>
    /// Unsubscribes from the unlock button press event listener on destroy
    /// </summary>
    private void OnDestroy()
    {
        _unlockAction.performed -= OnUnlockPerformed;
    }

    /// <summary>
    /// Flags that the unlock keybind is pressed
    /// </summary>
    /// <param name="context"></param>
    private void OnUnlockPerformed(InputAction.CallbackContext context)
    {
        UnlockKeyPressed = true;
        StartCoroutine(ResetUnlockKeyPressed());
    }

    /// <summary>
    /// Resets the unlock flag to false after 1 second of it going up
    /// </summary>
    /// <returns></returns>
    private IEnumerator ResetUnlockKeyPressed()
    {
        yield return new WaitForSeconds(1f);
        UnlockKeyPressed = false;
    }
}
