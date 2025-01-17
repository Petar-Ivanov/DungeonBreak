using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Enumerators;

/// <summary>
/// A centrall class that manages the overall game logic connecting all components and keeping track of meta data
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private DungeonUIManager DungeonUIManager;
    [SerializeField] private GridManager GridManager;
    [SerializeField] private DungeonGenerator DungeonGenerator;
    [SerializeField] private CoinSpawner CoinSpawner;
    [SerializeField] private KeySpawner KeySpawner;
    [SerializeField] private PlayerSpawner PlayerSpawner;
    [SerializeField] private NPCSpawner NPCSpawner;
    [SerializeField] private TrapSpawner TrapSpawner;

    [SerializeField] private int coinScoreValue = 10;
    [SerializeField] private int timeScoreMax = 50;
    [SerializeField] private int decayInterval = 30;
    [SerializeField] private int decayScore = 5;

    private GameState gameState = GameState.StartMenu;
    private CauseOfDeath causeOfPlayerDeath = 0;

    private int coinsCollected = 0;
    private float timer = 0;
    
    private bool doorDetectionActivated = false;
    private bool keyCollected = false;
    private bool didWin = false;

    private List<Vector3> occupiedSpawnLocation;

    public bool KeyCollected
    {
        get { return keyCollected; }
    }

    public int CoinsCollected
    {
        get { return coinsCollected; }
    } 

    public void AddCoin()
    {
        coinsCollected++;
        DungeonUIManager.UpdateCoinCount(coinsCollected);
    }

    public void CollectKey() 
    { 
        keyCollected = true;
        DungeonUIManager.SetKeyCollected(true);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    

    void Update()
    {
        if (gameState == GameState.Playing && DungeonUIManager != null) UpdateTimer();
        DetectDoorUnlock();
    }

    /// <summary>
    /// Increments the timer over time
    /// </summary>
    private void UpdateTimer()
    {
        timer += Time.deltaTime;
        DungeonUIManager.UpdateTimer(timer);
    }

    /// <summary>
    /// Detects whether the conditions for unlocking the door are satisfied and if so displays a prompt that tells the player what buttons to use.
    /// If the player uses the unlock button while satisfying all conditions the door is unlocked and the game is won.
    /// </summary>
    void DetectDoorUnlock()
    {
        if (gameState != GameState.Playing || doorDetectionActivated == false) return;

        if (GridManager != null && GridManager.IsPlayerNearDoor() && keyCollected)
        {
            DungeonUIManager.SetPromptVisibility(true);

            if(InputManager.UnlockKeyPressed)
            {
                InputManager.UnlockKeyPressed = false;
                EndGame(true);
            }
        }
        else DungeonUIManager.SetPromptVisibility(false);
    }

    /// <summary>
    /// Gets references to all classes used to generate, render and populate the dungeon.
    /// </summary>
    private void InitializeReferences()
    {
        DungeonGenerator = FindObjectOfType<DungeonGenerator>();
        DungeonUIManager = FindObjectOfType<DungeonUIManager>();
        GridManager = FindObjectOfType<GridManager>();
        CoinSpawner = FindObjectOfType<CoinSpawner>();
        KeySpawner = FindObjectOfType<KeySpawner>();
        PlayerSpawner = FindObjectOfType<PlayerSpawner>();
        NPCSpawner = FindObjectOfType<NPCSpawner>();
        TrapSpawner = FindObjectOfType<TrapSpawner>();
    }

    /// <summary>
    /// Resets all metadata
    /// </summary>
    private void ResetStats()
    {
        keyCollected = false;
        coinsCollected = 0;
        timer = 0;
        causeOfPlayerDeath = 0;
        didWin = false;
        doorDetectionActivated = false;
    }

    /// <summary>
    /// Generates, renders and populates a new dungeon with npcs, pickup items, traps, the key and the player
    /// </summary>
    private void InitializeNewLevel()
    {
        InitializeReferences();

        ResetStats();
        
        GridManager.SetGrid(DungeonGenerator.SpawnDungeon(),
            DungeonGenerator.transform.position,
            DungeonGenerator.WallTilemap.cellSize
        );

        GridManager.doorLocation = DungeonGenerator.doorLocation;

        List<Vector3> validPlayerSpawnLocations = DungeonGenerator.validPlayerSpawnLocations;
        List<Vector3> validKeySpawnLocations = DungeonGenerator.validKeySpawnLocations;
        List<Vector3> validPickupSpawnLocations = DungeonGenerator.validPickupSpawnLocations;

        occupiedSpawnLocation = new List<Vector3>();

        PlayerSpawner.GridManager = GridManager;
        occupiedSpawnLocation.Add(PlayerSpawner.SpawnPlayer(validPlayerSpawnLocations));

        occupiedSpawnLocation.Add(KeySpawner.SpawnKey(validKeySpawnLocations));

        NPCSpawner.GridManager = GridManager;
        foreach (Vector3 occupied in NPCSpawner.SpawnNPCs(validKeySpawnLocations))
        {
            occupiedSpawnLocation.Add(occupied);
        }
         
        TrapSpawner.SpawnTraps(validPickupSpawnLocations, occupiedSpawnLocation);
        CoinSpawner.SpawnCoins(validPickupSpawnLocations, occupiedSpawnLocation); 
    }

    /// <summary>
    /// Gets the player's cause of death and calls EndGame
    /// </summary>
    /// <param name="causeOfDeath"></param>
    public void PlayerKilled(CauseOfDeath causeOfDeath)
    {
        if(gameState == GameState.Playing)
        {
            this.causeOfPlayerDeath = causeOfDeath;
            EndGame(false);
        }
    }

    /// <summary>
    /// Sets the game state to GameOver, sends the metadata of the run to the ScoreboardManager to be saved into memory and switches to the Game Results scene.
    /// </summary>
    /// <param name="didWin"></param>
    public void EndGame(bool didWin)
    {
        this.didWin = didWin;
        if (gameState == GameState.Playing)
        {
            gameState = GameState.GameOver;
            Debug.Log("Game over!");

            ScoreboardManager.Instance.AddRun(this.didWin, coinsCollected, keyCollected, timer, CalculateScore());
             
            SceneManager.sceneLoaded += OnGameResultSceneLoaded;
             
             
            SceneManager.LoadSceneAsync("Game Result");
        }
    }

    /// <summary>
    /// Passes the metadata of the last played run from the GameManager to the GameResults UI
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnGameResultSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        doorDetectionActivated = false;

        if (scene.name == "Game Result")
        {
            GameResultUIManager GameResultUIManager = FindObjectOfType<GameResultUIManager>();
            GameResultUIManager.SetStatValues(this.didWin, coinsCollected, keyCollected, timer, 1, CalculateScore(), this.causeOfPlayerDeath);

            SceneManager.sceneLoaded -= OnDungeonSceneLoaded;
        }
    }

    /// <summary>
    /// Calculates the total score of the run based on the collected coin count and the time for completion.
    /// If the player looses the game all points are taken away.
    /// </summary>
    /// <returns></returns>
    private int CalculateScore()
    {
        if(didWin == false) return 0;

        int total;
        int timeBonus = timeScoreMax - (int)(timer / decayInterval * decayScore); 
        if(timeBonus < 0) timeBonus = 0;

        total = (coinsCollected * coinScoreValue) + timeBonus;

        return total;
    }

    /// <summary>
    /// Switches to the scoreboard state and scene
    /// </summary>
    public void LoadScoreboard()
    {
        if (gameState == GameState.StartMenu || gameState == GameState.GameOver)
        {
            gameState = GameState.Scoreboard;

            //Debug.Log("Scoreboard!"); 

            SceneManager.sceneLoaded += OnScoreboardSceneLoaded;
            SceneManager.LoadSceneAsync("Scoreboard");
        }
    }

    /// <summary>
    /// Loads the scoreboard scene
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    private void OnScoreboardSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        doorDetectionActivated = false;

        if (scene.name == "Scoreboard")
        { 
            ScoreboardManager.Instance.LoadScoreboard();

            SceneManager.sceneLoaded -= OnDungeonSceneLoaded;
        }
    }

    /// <summary>
    /// Loads the dungeon scene and switches to a Playing state, initialising the game
    /// </summary>
    public void StartGame()
    {
        if (gameState == GameState.StartMenu || gameState == GameState.GameOver || gameState == GameState.Scoreboard)
        {
            gameState = GameState.Playing;
             
            SceneManager.sceneLoaded += OnDungeonSceneLoaded;
             
            SceneManager.LoadSceneAsync("Dungeon");
        }
    }

    private void OnDungeonSceneLoaded(Scene scene, LoadSceneMode mode)
    { 
        if (scene.name == "Dungeon")
        { 
            InitializeNewLevel();
            doorDetectionActivated = true;
             
            SceneManager.sceneLoaded -= OnDungeonSceneLoaded;
        }
    }
}
