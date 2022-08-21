using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    public GameObject fogTilePrefab;

    public GameObject[, ] fogTiles;

    public bool fogTilesEnabled = false;
    public bool hiddenLevelObjectsFogEnabled = true;

    /// <summary>
    /// Keeps track of whats revealed on the map by macrochips or other special effects.
    /// </summary>
    private Dictionary<IRevealable, bool> revealedObjects;

    /// <summary>
    /// Keeps track of what IRevealable objects has each revealer revealed. 
    /// </summary>
    private List<RevealEffect> activeRevealEffects;

    public void Init(List<Cell> allCells, int levelWidth, int levelLength)
    {
        if (fogTilesEnabled)
        {
            InitFogTiles(allCells, levelWidth, levelLength);
        }

        if (hiddenLevelObjectsFogEnabled)
        {
            InitHiddenLevelObjectsFog(allCells, levelWidth, levelLength);
        }

        revealedObjects = new Dictionary<IRevealable, bool>();
        activeRevealEffects = new List<RevealEffect>();

        GameMap.Instance.cellGrid.TurnEnded += OnTurnEnded;
        GameMaster.Instance.LevelLoading += OnLevelEnd;
    }

    public bool IsElementRevealed(IRevealable element)
    {
        if (revealedObjects.ContainsKey(element))
        {
            return revealedObjects[element];
        }
        else
        {
            return false;
        }
    }

    public void SetFogOfWar(List<Cell> unFogged, List<Cell> allCells)
    {
        if (fogTilesEnabled)
        {
            SetFogTiles(unFogged, allCells);
        }

        if (hiddenLevelObjectsFogEnabled)
        {
            SetHiddenLevelObjectsFog(unFogged, allCells);
        }
    }

    private void SetHiddenLevelObjectsFog(List<Cell> unFogged, List<Cell> allCells)
    {
        int hiddenLayer = CameraOgre.Instance.GetHiddenLevelObjectsLayer();
        int visibleLayer = CameraOgre.Instance.GetVisibleLevelObjectsLayer();
        int visibleCharactersLayer = CameraOgre.Instance.GetVisibleCharactersLayer();

        // Hide all not revealed objects
        foreach (Cell cell in allCells)
        {
            if (unFogged.Contains(cell))
            {
                continue;
            }
            // if (!revealedObjects[IRevealable]) means if the IRevealable object is not revealed, (then we can hide it normally)
            if (!revealedObjects.ContainsKey(cell) || !revealedObjects[cell])
            {
                SetLayerInTransformHierarchy(cell.gameObject, hiddenLayer);
            }
            else // is revealed, so set visible
            {
                SetLayerInTransformHierarchy(cell.gameObject, visibleLayer);
            }

            Obstacle obstacle = ObstacleManager.Instance.allObstacles.Find(o => o.cell == cell);
            if (obstacle != null)
            {
                if (!revealedObjects.ContainsKey(obstacle) || !revealedObjects[obstacle])
                {
                    SetLayerInTransformHierarchy(obstacle.gameObject, hiddenLayer);
                }
                else // is revealed, so set visible
                {
                    SetLayerInTransformHierarchy(obstacle.gameObject, visibleLayer);
                }
            }

            Cover cover = CoverManager.Instance.allCovers.Find(o => o.cell == cell);
            if (cover != null)
            {
                if (!revealedObjects.ContainsKey(cover) || !revealedObjects[cover])
                {
                    SetLayerInTransformHierarchy(cover.gameObject, hiddenLayer);
                }
                else // is revealed, so set visible
                {
                    SetLayerInTransformHierarchy(cover.gameObject, visibleLayer);
                }
            }

            InteractablePoint point = InteractablePointManager.Instance.allInteractablePoints.Find(o => o.cell == cell);
            if (point != null)
            {
                if (!revealedObjects.ContainsKey(point) || !revealedObjects[point])
                {
                    SetLayerInTransformHierarchy(point.gameObject, hiddenLayer);
                }
                else // is revealed, so set visible
                {
                    SetLayerInTransformHierarchy(point.gameObject, visibleLayer);
                }
            }

            Unit unit = UnitManager.Instance.allUnits.Find(o => o.Cell == cell);
            if (unit != null)
            {
                if (!revealedObjects.ContainsKey(unit) || !revealedObjects[unit])
                {
                    SetLayerInTransformHierarchy(unit.gameObject, hiddenLayer);
                }
                else // is revealed, so set visible
                {
                    SetLayerInTransformHierarchy(unit.gameObject, visibleCharactersLayer);
                }
            }
        }

        // Reveal visible objects
        foreach (Cell cell in unFogged)
        {
            SetLayerInTransformHierarchy(cell.gameObject, visibleLayer);
            Obstacle obstacle = ObstacleManager.Instance.allObstacles.Find(o => o.cell == cell);
            if (obstacle != null) SetLayerInTransformHierarchy(obstacle.gameObject, visibleLayer);
            Cover cover = CoverManager.Instance.allCovers.Find(o => o.cell == cell);
            if (cover != null) SetLayerInTransformHierarchy(cover.gameObject, visibleLayer);
            InteractablePoint point = InteractablePointManager.Instance.allInteractablePoints.Find(o => o.cell == cell);
            if (point != null) SetLayerInTransformHierarchy(point.gameObject, visibleLayer);
            Unit unit = UnitManager.Instance.allUnits.Find(o => o.Cell == cell);
            if (unit != null) SetLayerInTransformHierarchy(unit.gameObject, visibleCharactersLayer);
        }
    }

    private void SetLayerInTransformHierarchy(GameObject target, int layerNumber)
    {
        if (target == null) return;

        foreach (Transform child in target.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = layerNumber;
        }
    }

    private void InitHiddenLevelObjectsFog(List<Cell> allCells, int levelWidth, int levelLength)
    {

    }

    private void InitFogTiles(List<Cell> allCells, int levelWidth, int levelLength)
    {
        fogTiles = new GameObject[levelWidth, levelLength];
        for (int i = 0; i < allCells.Count; i++)
        {
            GameObject fogTile = Instantiate(fogTilePrefab, transform);
            fogTile.SetActive(true);
            fogTiles[(int) allCells[i].OffsetCoord.x, (int) allCells[i].OffsetCoord.y] = fogTile;
            fogTile.transform.position = allCells[i].transform.position;
        }
    }

    private void SetFogTiles(List<Cell> unFogged, List<Cell> allCells)
    {
        foreach (Cell cell in allCells)
        {
            if (unFogged.Contains(cell))
            {
                fogTiles[(int) cell.OffsetCoord.x, (int) cell.OffsetCoord.y].SetActive(false);
            }
            else
            {
                fogTiles[(int) cell.OffsetCoord.x, (int) cell.OffsetCoord.y].SetActive(true);
            }
        }
    }

    private void OnLevelEnd()
    {
        foreach (RevealEffect effect in new List<RevealEffect>(activeRevealEffects))
        {
            RemoveRevealEffect(effect);
        }

        activeRevealEffects.Clear();
    }

    private void OnTurnEnded(object sender, EventArgs e)
    {
        DecrementRevealEffectDurations((sender as CellGrid).CurrentPlayerNumber);
    }

    /// <summary>
    /// Goes through active reveal effects, decrements their durations, then checks if duration reached zero, if so remove effect.
    /// </summary>
    /// <param name="currentPlayerNumber"></param>
    private void DecrementRevealEffectDurations(int currentPlayerNumber)
    {
        List<RevealEffect> toRemove = new List<RevealEffect>();
        foreach (RevealEffect effect in activeRevealEffects)
        {
            if (effect.activatorPlayerNumber == currentPlayerNumber)
            {
                effect.duration--;
                if (effect.duration == 0)
                {
                    toRemove.Add(effect);
                }
            }
        }

        foreach (RevealEffect effect in toRemove)
        {
            RemoveRevealEffect(effect);
        }

        if (toRemove.Count > 0)
        {
            GameMap.Instance.UpdateFogOfWar(Squad.Instance.GetAllAliveUnits());
        }
    }

    private void RemoveRevealEffect(RevealEffect effect)
    {
        foreach (IRevealable revealed in effect.revealed)
        {
            bool revealedByOtherEffect = false;
            foreach (RevealEffect otherEffect in activeRevealEffects)
            {
                if (otherEffect != effect && otherEffect.revealed.Contains(revealed))
                {
                    revealedByOtherEffect = true;
                    break;
                }
            }

            if (!revealedByOtherEffect)
            {
                if (!revealedObjects.ContainsKey(revealed))
                {
                    Debug.LogWarning("Revealed object not found when trying to remove.");
                    continue;
                }
                revealedObjects[revealed] = false;
            }
        }

        activeRevealEffects.Remove(effect);
    }

    /// <summary>
    /// Activates a new reveal effect and keeps track of it. Reveals all objects in the effects reveal list.
    /// </summary>
    /// <param name="revealEffect"></param>
    public void ActivateRevealEffect(RevealEffect revealEffect)
    {
        foreach (IRevealable revealable in revealEffect.revealed)
        {
            if (revealedObjects.ContainsKey(revealable))
            {
                revealedObjects[revealable] = true;
            }
            else
            {
                revealedObjects.Add(revealable, true);
            }
        }

        activeRevealEffects.Add(revealEffect);

        GameMap.Instance.UpdateFogOfWar(Squad.Instance.GetAllAliveUnits());
    }

    /// <summary>
    /// Deactivates a reveal effect and removes it. Only for special cases, most reveal effects should expire by duration.
    /// </summary>
    /// <param name="revealEffect"></param>
    public void DeactivateRevealEffect(RevealEffect revealEffect)
    {
        RemoveRevealEffect(revealEffect);
        GameMap.Instance.UpdateFogOfWar(Squad.Instance.GetAllAliveUnits());
    }

    public static List<IRevealable> GetRevealTargetsOfType(RevealableType targetType)
    {
        List<IRevealable> revealables = new List<IRevealable>();
        List<InteractablePoint> points = null;
        List<Cell> cells = null;
        switch (targetType)
        {
            case RevealableType.Loot:
                points = InteractablePointManager.Instance.GetAllLootPoints();
                revealables.AddRange(points);
                break;
            case RevealableType.Shop:
                points = InteractablePointManager.Instance.GetAllShopPoints();
                revealables.AddRange(points);
                break;
            case RevealableType.HealthStation:
                points = InteractablePointManager.Instance.GetAllChargePointsOfType(ChargePoint.ChargeType.Health);
                revealables.AddRange(points);
                break;
            case RevealableType.ArmorStation:
                points = InteractablePointManager.Instance.GetAllChargePointsOfType(ChargePoint.ChargeType.Armor);
                revealables.AddRange(points);
                break;
            case RevealableType.EnergyStation:
                points = InteractablePointManager.Instance.GetAllChargePointsOfType(ChargePoint.ChargeType.Energy);
                revealables.AddRange(points);
                break;
            case RevealableType.Elevator:
                cells = GameMaster.Instance.currentLevelElevator.endElevator;
                revealables.AddRange(cells);
                break;
            case RevealableType.Enemies:
                revealables.AddRange(UnitManager.Instance.allUnits);
                break;
            case RevealableType.BareMap:
                revealables.AddRange(GameMap.Instance.cellGrid.Cells);
                revealables.AddRange(CoverManager.Instance.allCovers);
                revealables.AddRange(ObstacleManager.Instance.allObstacles);
                break;
            case RevealableType.FullMap:
                revealables.AddRange(GameMap.Instance.cellGrid.Cells);

                revealables.AddRange(UnitManager.Instance.allUnits);

                revealables.AddRange(CoverManager.Instance.allCovers);
                revealables.AddRange(ObstacleManager.Instance.allObstacles);
                revealables.AddRange(InteractablePointManager.Instance.allInteractablePoints);
                break;
        }

        return revealables;
    }

    public static RevealEffect CreateRevealEffect(object revealer, RevealableType targets, int duration, int activatorPlayerNumber)
    {
        return new RevealEffect(revealer, GetRevealTargetsOfType(targets), duration, activatorPlayerNumber);
    }

    public enum RevealableType
    {
        Loot,
        Shop,
        HealthStation,
        ArmorStation,
        EnergyStation,
        Elevator,
        Enemies,
        BareMap,
        FullMap
    }

    public class RevealEffect
    {
        public object revealer;
        public List<IRevealable> revealed;
        public int duration;
        public int activatorPlayerNumber;

        public RevealEffect(object revealer, List<IRevealable> revealed, int duration, int activatorPlayerNumber)
        {
            this.revealer = revealer;
            this.revealed = revealed;
            this.duration = duration;
            this.activatorPlayerNumber = activatorPlayerNumber;
        }
    }

}
