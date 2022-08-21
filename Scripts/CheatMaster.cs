using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatMaster : MonoBehaviour
{
    private static CheatMaster _Instance = null;
    public static CheatMaster Instance
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<CheatMaster>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find singleton Instance of type CheatMaster.");
                }
                return _Instance;
            }
        }
    }

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

        DontDestroyOnLoad(this);
    }

    private bool mapRevealActive = false;
    private FogOfWar.RevealEffect mapRevealEffect;

    private void Start()
    {
        GameMaster.Instance.LevelStarted += Init;
    }

    public void Init()
    {
        mapRevealEffect = FogOfWar.CreateRevealEffect(this, FogOfWar.RevealableType.FullMap, -1, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMapReveal();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            RegenerateMap();
        }
    }

    private void ToggleMapReveal()
    {
        if (mapRevealActive)
        {
            GameMap.Instance.fogOfWar.DeactivateRevealEffect(mapRevealEffect);
            mapRevealActive = false;
        }
        else
        {
            GameMap.Instance.fogOfWar.ActivateRevealEffect(mapRevealEffect);
            mapRevealActive = true;
        }
    }

    private void RegenerateMap()
    {
        GameMaster.Instance.ElevatorActivated(true);
    }
}
