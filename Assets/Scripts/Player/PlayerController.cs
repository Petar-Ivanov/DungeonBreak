using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Enumerators;

/// <summary>
/// Handles player animation, movement, collision and death
/// </summary>
public class PlayerController : MonoBehaviour
{
    private GridManager gridManager;

    private Rigidbody2D _rb;
    private Animator _animator;

    [Header("Player Stats")]
    [SerializeField] private float _moveSpeed = 7f;
    private Vector2 _movement;

    private bool isAlive = true;

    private const string _horizontal = "Horizontal";
    private const string _vertical = "Vertical";
    private const string _lastHorizontal = "LastHorizontal";
    private const string _lastVertical = "LastVertical";

    /// <summary>
    /// The navigation grid that the player uses
    /// </summary>
    public GridManager GridManager
    {
        set { this.gridManager = value; }
    }

    public bool IsAlive
    {
        get { return this.isAlive; }
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Handles player movement, input control and animation
    /// </summary>
    void Update()
    {
        SetupMovement();
        SetupAnimation();

        gridManager.SetPlayerLocation(new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y)));
    }

    /// <summary>
    /// Handles the player movement
    /// </summary>
    private void SetupMovement()
    {
        _movement.Set(InputManager.Movement.x, InputManager.Movement.y);
        _rb.velocity = _movement * _moveSpeed;
    }

    /// <summary>
    /// Handles the player animation based on its movement
    /// </summary>
    private void SetupAnimation()
    {
        _animator.SetFloat(_horizontal, _movement.x);
        _animator.SetFloat(_vertical, _movement.y);

        if (_movement != Vector2.zero)
        {
            _animator.SetFloat(_lastHorizontal, _movement.x);
            _animator.SetFloat(_lastVertical, _movement.y);
        }
    }

    /// <summary>
    /// Detects player collision with npcs and traps and activates player death
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay2D(Collider2D other)
    {
        if (isAlive)
        {
            if (other.CompareTag("NPC"))
            {
                isAlive = false;
                Debug.Log("Collided with NPC");
                GameManager.Instance.PlayerKilled(CauseOfDeath.Ghost);
            }
            else if (other.CompareTag("Trap"))
            {
                TrapController trap = other.GetComponent<TrapController>();
                if (trap != null && trap.IsTrapActive)
                {
                    isAlive = false;
                    Debug.Log("Collided with a trap");
                    GameManager.Instance.PlayerKilled(CauseOfDeath.SpikeTrap);
                }
            }
        }
    }
}
