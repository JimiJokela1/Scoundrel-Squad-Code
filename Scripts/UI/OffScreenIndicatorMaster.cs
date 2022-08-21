using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OffScreenIndicatorMaster : MonoBehaviour
{
    private static OffScreenIndicatorMaster _Instance = null;
    public static OffScreenIndicatorMaster Instance
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<OffScreenIndicatorMaster>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find singleton Instance of type OffScreenIndicatorMaster.");
                }
                else
                {
                    _Instance.Init();
                }
                return _Instance;
            }
        }
    }

    public GameObject enemyUnitIndicatorPrefab;
    public GameObject playerUnitIndicatorPrefab;

    public CanvasScaler canvasScaler;

    private bool initialized = false;

    [Tooltip("Off screen indicators for each unit")]
    private Dictionary<UberUnit, RectTransform> unitIndicators;

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

        unitIndicators = new Dictionary<UberUnit, RectTransform>();

        GameMaster.Instance.LevelStarted += CreateUnitIndicators;
    }

    private void OnDestroy()
    {
        // GameMaster.Instance.LevelStarted -= CreateUnitIndicators;
    }

    private void Update()
    {
        UpdateIndicatorVisibility();
        UpdateIndicatorPositions();
    }

    private void CreateUnitIndicators()
    {
        foreach (var indicator in unitIndicators)
        {
            Destroy(indicator.Value.gameObject);
        }

        unitIndicators.Clear();

        foreach (UberUnit unit in UnitManager.Instance.allUberUnits)
        {
            CreateIndicator(unit);
        }
    }

    private void CreateIndicator(UberUnit unit)
    {
        if (unitIndicators.ContainsKey(unit))
        {
            return;
        }

        if (unit is PlayerUnit)
        {
            GameObject indicator = Instantiate(playerUnitIndicatorPrefab, transform);
            if (indicator.GetComponent<RectTransform>())
            {
                unitIndicators.Add(unit, indicator.GetComponent<RectTransform>());
            }
            else
            {
                Debug.LogWarning("Invalid off screen indicator prefab");
                Destroy(indicator);
            }
        }
        else if (unit is EnemyUnit)
        {
            GameObject indicator = Instantiate(enemyUnitIndicatorPrefab, transform);
            if (indicator.GetComponent<RectTransform>())
            {
                unitIndicators.Add(unit, indicator.GetComponent<RectTransform>());
            }
            else
            {
                Debug.LogWarning("Invalid off screen indicator prefab");
                Destroy(indicator);
            }
        }
    }

    private void UpdateIndicatorVisibility()
    {
        foreach (var unitIndicator in unitIndicators)
        {
            if (GameUI.Instance.IsFullScreenWindowOpen())
            {
                unitIndicator.Value.gameObject.SetActive(false);
            }
            else if (GameMap.Instance.IsElementRevealed(unitIndicator.Key))
            {
                // Vector3 screenPos = Camera.main.WorldToScreenPoint(unitIndicator.Key.transform.position);

                if (!unitIndicator.Key.IsVisible())
                {
                    unitIndicator.Value.gameObject.SetActive(true);
                }
                else
                {
                    unitIndicator.Value.gameObject.SetActive(false);
                }
            }
            else
            {
                unitIndicator.Value.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateIndicatorPositions()
    {
        float canvasWidth = canvasScaler.referenceResolution.x;
        float canvasHeight = canvasScaler.referenceResolution.y;

        foreach (var unitIndicator in unitIndicators)
        {
            if (!unitIndicator.Value.gameObject.activeSelf)
            {
                continue;
            }

            Vector3 viewportPos = Camera.main.WorldToViewportPoint(unitIndicator.Key.transform.position);
            // Vector3 screenPos = Camera.main.WorldToScreenPoint(unitIndicator.Key.transform.position);
            Vector3 screenPos = new Vector3(viewportPos.x * canvasWidth, viewportPos.y * canvasHeight);
            Vector2 indicatorPosition = screenPos;

            if (screenPos.x > canvasWidth)
            {
                indicatorPosition.x = canvasWidth - unitIndicator.Value.sizeDelta.x;
                unitIndicator.Value.rotation = Quaternion.Euler(0f, 0f, -90f);
            }

            if (screenPos.y > canvasHeight)
            {
                indicatorPosition.y = canvasHeight - unitIndicator.Value.sizeDelta.y;
                unitIndicator.Value.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            if (screenPos.x < 0)
            {
                indicatorPosition.x = unitIndicator.Value.sizeDelta.x;
                unitIndicator.Value.rotation = Quaternion.Euler(0f, 0f, 90f);
            }

            if (screenPos.y < 0)
            {
                indicatorPosition.y = unitIndicator.Value.sizeDelta.y;
                unitIndicator.Value.rotation = Quaternion.Euler(0f, 0f, 180f);
            }

            unitIndicator.Value.anchoredPosition = indicatorPosition;
        }
    }
}
