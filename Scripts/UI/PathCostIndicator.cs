using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PathCostIndicator : MonoBehaviour
{
    private static PathCostIndicator _Instance = null;
    public static PathCostIndicator Instance
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<PathCostIndicator>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find singleton Instance of type PathCostIndicator.");
                }
                else
                {
                    _Instance.Init();
                }
                return _Instance;
            }
        }
    }

    private bool initialized = false;
    
    public GameObject pathCostCanvas;
    public TextMeshProUGUI pathCostText;

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
        if (initialized) return;
    }

    public void ShowPathCost(int pathCost, Cell cell)
    {
        pathCostCanvas.SetActive(true);
        pathCostCanvas.transform.position = cell.transform.position;
        pathCostText.SetText(pathCost.ToString());
    }

    public void HidePathCost()
    {
        pathCostCanvas.SetActive(false);
    }
}
