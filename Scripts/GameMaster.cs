using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    private static GameMaster _Instance = null;
    public static GameMaster Instance
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<GameMaster>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find singleton Instance of type GameMaster.");
                }
                return _Instance;
            }
        }
    }

    public delegate void GameEvent();

    /// <summary>
    /// Event invoked before level is loaded
    /// </summary>
    public event GameEvent LevelLoading;
    /// <summary>
    /// Event invoked when level is loaded
    /// </summary>
    public event GameEvent LevelLoadingDone;
    /// <summary>
    /// Event is invoked when a new game starts
    /// </summary>
    public event GameEvent GameStarting;
    /// <summary>
    /// Event is invoked when a game ends
    /// </summary>
    public event GameEvent GameEnded;
    /// <summary>
    /// Event is invoked when a level starts
    /// </summary>
    public event GameEvent LevelStarted;

    public int startCredits;

    [HideInInspector]
    public Elevator currentLevelElevator;

    [HideInInspector]
    public int currentLevel;

    private bool mainMenuReload = false;

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else if (_Instance != this)
        {
            Debug.Log("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Init();
    }

    private void Init()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += SceneLoaded;

        if (MainMenu.Instance == null)
        {
            mainMenuReload = true;
            SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Additive);
        }
        else
        {
            MainMenu.Instance.Show();
        }
    }

    public void GameOver(bool victory)
    {
        OnGameEnded();

        string title = (victory) ? "Victory!" : "Defeat!";
        string message = "";
        if (victory)
        {
            message += "The megacorporation is no more!";
        }
        else
        {
            message += "Failure will not slow down the oppressive megacorporation.";
        }
        
        // TODO: add game end stats
        // message += 
        ConfirmationWindow.Instance.ShowConfirmation(title, message, GameOverConfirmed, null, true, false);
    }

    private void GameOverConfirmed()
    {
        currentLevel = 0;
        currentLevelElevator = null;
        mainMenuReload = false;

        ReturnToMainMenu();
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("MainMenuScene", LoadSceneMode.Single);
        MainMenu.Instance.Show();
    }

    public void StartNewGame()
    {
        MainMenu.Instance.ShowLoadingScreen();
        SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Single);
    }

    public virtual void OnLevelLoading()
    {
        if (LevelLoading != null)
        {
            LevelLoading.Invoke();
        }
    }

    public virtual void OnLevelLoadingDone()
    {
        if (LevelLoadingDone != null)
        {
            LevelLoadingDone.Invoke();
        }
    }

    public virtual void OnGameStarting()
    {
        if (GameStarting != null)
        {
            GameStarting.Invoke();
        }
    }

    public virtual void OnGameEnded()
    {
        if (GameEnded != null)
        {
            GameEnded.Invoke();
        }
    }

    public virtual void OnLevelStarted()
    {
        if (LevelStarted != null)
        {
            LevelStarted.Invoke();
        }
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mainMenuReload && scene.name == "MainMenuScene")
        {
            mainMenuReload = false;
            SceneManager.UnloadSceneAsync("MainMenuScene");
        }

        if (scene.isLoaded && scene.name != "MainMenuScene")
        {
            StartCoroutine(StartNewGameSequence());
        }
    }

    public IEnumerator StartNewGameSequence()
    {
        CellGrid cellGrid = GameMap.Instance.cellGrid;
        currentLevel = 0;

        cellGrid.Initialize();
        LevelGenerator.Instance.ResetLevel();
        yield return 0;

        // cellGrid.InitLevel();
        // UnitManager.Instance.AssignUnitsToCells(Squad.Instance.GetAllUnits(), cellGrid.Cells);

        yield return StartCoroutine(NewLevel());

        OnGameStarting();

        // give starting money
        Squad.Instance.AddCredits(startCredits, false);
        // open start shop
        if (currentLevelElevator != null && currentLevelElevator.startElevator != null && currentLevelElevator.startElevator.Count > 0)
        {
            Cell shopCell = currentLevelElevator.startElevator[0];
            StartShop.Instance.OpenStartShop(shopCell);
        }
        else
        {
            Debug.LogError("Null elevator");
        }

        MainMenu.Instance.Hide();

        OnLevelStarted();
    }

    public void ElevatorActivated(bool cheat = false)
    {
        if (!cheat)
        {
            // Kill any scoundrels not in end elevator
            List<PlayerUnit> unitsNotInElevator = Squad.Instance.GetAllAliveUnits().Except(currentLevelElevator.unitsInEndElevator).ToList();

            foreach(PlayerUnit unit in unitsNotInElevator)
            {
                unit.DestroyUnit();
            }
        }

        StartCoroutine(ElevatorActivatedSequence());
    }

    private IEnumerator ElevatorActivatedSequence()
    {
        if (LevelGenerator.Instance.levelProgression.winLevel == currentLevel)
        {
            GameOver(true);
            yield break;
        }

        yield return StartCoroutine(NewLevel());

        Squad.Instance.ChargeScoundrelsEnergy(GameGlobals.ELEVATOR_ENERGY_CHARGE);

        OnLevelStarted();
    }

    /// <summary>
    /// Creates new level and tells cellgrid to start game
    /// </summary>
    /// <returns></returns>
    private IEnumerator NewLevel()
    {
        CellGrid cellGrid = GameMap.Instance.cellGrid;
        currentLevel++;
        GameUI.Instance.UpdateCurrentLevel(currentLevel);
        
        GameMap.Instance.HideLevel();

        OnLevelLoading();

        UnitManager.Instance.ResetNonPlayerUnits();
        // wait for frame for unity to actually destroy object marked for destruction
        yield return 0;
        UnitManager.Instance.PlacePlayerUnitsInLimbo(Squad.Instance.GetAllUnits());

        LevelGenerator.Instance.ResetLevel();
        yield return 0;
        currentLevelElevator = LevelGenerator.Instance.CreateNewLevel(Squad.Instance.GetAllUnits(), currentLevel);
        yield return 0;

        UnitManager.Instance.SetPlayerUnitsToStartElevator(Squad.Instance.GetAllUnits(), currentLevelElevator.startElevator);
        ObstacleManager.Instance.AssignToCells(null);
        CoverManager.Instance.AssignToCells(null);
        InteractablePointManager.Instance.AssignToCells(null);

        GameMap.Instance.MapNewLevel();

        EnemySpawner.Instance.SpawnEnemies(LevelGenerator.Instance.levelProgression.GetActiveEnemyProgress(currentLevel), cellGrid.Cells);
        UnitManager.Instance.AssignUnitsToCells(Squad.Instance.GetAllUnits(), cellGrid.Cells);

        cellGrid.InitLevel();

        cellGrid.StartGame();

        OnLevelLoadingDone();
    }
}
