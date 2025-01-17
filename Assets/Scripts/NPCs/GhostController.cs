using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles npc navigation, movement, animation, states, ai behavior
/// </summary>
public class GhostController : MonoBehaviour
{
    GridManager gridManager;

    private Rigidbody2D _rb;
    private Animator _animator;

    [Header("NPC Stats")]
    [SerializeField] private float _moveSpeed = 4.5f;
    private Vector2 _movement;

    /// <summary>
    /// Behavior states of the npc.
    /// Roaming - randomly traversing the navigation grid.
    /// Chasing - following the player
    /// </summary>
    private enum State
    {
        Roaming = 0,
        Chasing = 1
    }

    private State state = State.Roaming;

    // The current path that the npc follows
    private List<Vector2Int> path = new List<Vector2Int>();

    // NPC player detection timer
    [Header("GhostNPC AI")]
    [SerializeField] private float playerDetectionInterval = 0.55f;
    private float timeSinceLastDetection = 0f;

    // Chase duration after loosing visibility of the player 
    [SerializeField] private float chaseTime = 3;
    private float timeSinceChaseStart = 0;

    private const string _horizontal = "Horizontal";
    private const string _vertical = "Vertical";
    private const string _isChasing = "IsChasing";

    /// <summary>
    /// The navigation grid that the npc uses
    /// </summary>
    public GridManager GridManager
    {
        set { this.gridManager = value; }
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GenerateNewPath();
    }

    /// <summary>
    /// Defines the overall npc behaviour and sets the corresponding animation.
    /// Looks for the player in all cardinal directions.
    /// If player found transitions into chasing state.
    /// When chasing - follows player while the player is visible and then 3 seconds after that.
    /// When roaming - explores random paths.
    /// </summary>
    void Update()
    {
        //FindPlayer();
        timeSinceLastDetection += Time.deltaTime;

        // Check if it's time to detect the player again
        if (timeSinceLastDetection >= playerDetectionInterval)
        {
            FindPlayer();
            timeSinceLastDetection = 0f;
        }

        if (state == State.Roaming)
        {
            if (path.Count > 0)
            {
                MoveAlongPath();
            }
            else if (state == State.Roaming)
            {
                GenerateNewPath();
            }
        }
        else if (state == State.Chasing)
        {
            if (path.Count > 0)
            {
                MoveAlongPath();
            }
            else if (state == State.Chasing)
            {
                ChasePlayer();

            }
        }

        timeSinceChaseStart += Time.deltaTime;

        if (timeSinceChaseStart >= chaseTime)
        {
            state = State.Roaming;
        }

        SetupAnimation();
    }

    /// <summary>
    /// Handles the npc animation based on its movement and state
    /// </summary>
    private void SetupAnimation()
    {
        _animator.SetFloat(_horizontal, _movement.x);
        _animator.SetFloat(_vertical, _movement.y);
        _animator.SetBool(_isChasing, state == State.Chasing);
    }

    /// <summary>
    /// Moves the npc to the center of the closest tile on its path, then removes the tile from the path
    /// </summary>
    private void MoveAlongPath()
    {
        Vector2 target = GetCellCenter(path[0]);
        Vector2 currentPosition = transform.position;

        _movement = (target - currentPosition).normalized;
        _rb.velocity = _movement * _moveSpeed;

        // Check if close enough to the target
        if (Vector2.Distance(currentPosition, target) < (state == State.Roaming? 0.05f: 0.35f))
        {
            path.RemoveAt(0);
        }

    }

    /// <summary>
    /// Picks a random walkable tile from the navigation grid and gets the path from its position to that point
    /// </summary>
    private void GenerateNewPath()
    {
        Vector3 npcPosition = transform.position;
        Vector2Int npcGridPosition = gridManager.GetGridCoordinates(new Vector2Int(Mathf.FloorToInt(npcPosition.x), Mathf.FloorToInt(npcPosition.y)));

        int randomX = Random.Range(1, gridManager.gridWidth);
        int randomY = Random.Range(1, gridManager.gridHeight);

        path = gridManager.FindPath(npcGridPosition, new Vector2Int(randomX, randomY));
    }

    /// <summary>
    /// Gets the player location in the navigation grid and then gets the path from its position to the player
    /// </summary>
    private void ChasePlayer()
    {
        Vector2Int playerLocation = gridManager.GetPlayerLocation();

        Vector3 npcPosition = transform.position;

        Vector2Int npcGridPosition = gridManager.GetGridCoordinates(new Vector2Int(Mathf.FloorToInt(npcPosition.x), Mathf.FloorToInt(npcPosition.y)));

        path = gridManager.FindPath(npcGridPosition, playerLocation);
    }

    /// <summary>
    /// Initializes a chase state and action
    /// </summary>
    private void StartChase()
    {
        state = State.Chasing;
        timeSinceChaseStart = 0;
        ChasePlayer();
    }

    /// <summary>
    /// Ends a chase.
    /// Transitions to roaming
    /// </summary>
    private void EndChase()
    {
        state = State.Roaming;
        timeSinceChaseStart = 0;
    }

    /// <summary>
    /// Checks whether the player is within the sights of the npc
    /// </summary>
    private void FindPlayer()
    {
        Vector3 npcPosition = transform.position;

        bool playerFound = gridManager.DetectPlayer(new Vector2Int(Mathf.FloorToInt(npcPosition.x), Mathf.FloorToInt(npcPosition.y)));

        if (playerFound)
        {
            //Debug.Log("Player found!");
            state = State.Chasing;
            path.Clear();
            timeSinceChaseStart = 0;
        }
    }

    /// <summary>
    /// Gets the coordinates of the center of a cell from the navigation grid
    /// </summary>
    /// <param name="gridPosition"></param>
    /// <returns></returns>
    private Vector2 GetCellCenter(Vector2Int gridPosition)
    {
        Vector2 cellCenter = new Vector2(gridPosition.x + gridManager.cellSize.x / 2f,
                                         gridPosition.y + gridManager.cellSize.y / 2f);
        return cellCenter;
    }
}
