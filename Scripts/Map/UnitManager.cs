using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    public static UnitManager Instance = null;

    public Transform UnitsParent;
    public Transform CellsParent;

    public List<Unit> allUnits;
    public List<UberUnit> allUberUnits
    {
        get
        {
            List<UberUnit> uberUnits = new List<UberUnit>();
            foreach(Unit unit in allUnits)
            {
                if (unit is UberUnit)
                {
                    uberUnits.Add((UberUnit) unit);
                }
            }
            return uberUnits;
        }
    }

    private List<Unit> addedUnits;
    private List<Unit> NPCs;

    public UberUnit GetActiveUnitOnCell(Cell cell)
    {
        Unit unit = allUnits.Find(o => o.Cell == cell);
        if (unit != null && unit is UberUnit)
        {
            UberUnit uberUnit = (UberUnit)unit;
            if (uberUnit.IsAlive())
            {
                return uberUnit;
            }
        }

        return null;
    }

    public List<UberUnit> GetActiveUnitsOnCell(Cell cell)
    {
        List<UberUnit> units = new List<UberUnit>();
        foreach(Unit unit in allUnits.FindAll(o => o.Cell == cell))
        {
            if (unit != null && unit is UberUnit)
            {
                UberUnit uberUnit = (UberUnit)unit;
                if (uberUnit.IsAlive())
                {
                    units.Add(uberUnit);
                }
            }
        }

        return units;
    }

    public void ResetNonPlayerUnits()
    {
        if (NPCs == null || NPCs.Count == 0)
        {
            return;
        }

        foreach (Unit unit in NPCs)
        {
            if (unit != null)
            {
                if (allUnits != null)
                {
                    allUnits.Remove(unit);
                }
                Destroy(unit.gameObject);
            }
        }

        NPCs.Clear();
    }

    public void AssignUnitsToCells(List<PlayerUnit> playerUnits, List<Cell> cells)
    {
        allUnits = UnitsParent.GetComponentsInChildren<Unit>().ToList();
        foreach (Unit unit in new List<Unit>(allUnits))
        {
            if (unit != null && !playerUnits.Contains(unit))
            {
                var cell = cells.OrderBy(h => Mathf.Abs((h.transform.position - unit.transform.position).magnitude)).First();
                if (!cell.BlocksMovement)
                {
                    cell.BlocksMovement = true;
                    unit.Cell = cell;
                    Vector3 offset = new Vector3(0, cell.GetCellDimensions().y, 0);
                    unit.transform.position = cell.transform.position + offset;
                    unit.Initialize();
                }
                else
                {
                    Debug.LogWarning("Unit spawned on blocked cell and destroyed. Cell: " + cell.OffsetCoord);
                    allUnits.Remove(unit);
                    Destroy(unit.gameObject);
                }
            }
        }

        foreach (PlayerUnit unit in playerUnits)
        {
            unit.Initialize();
        }

        foreach (Unit unit in allUnits)
        {
            if (addedUnits.Contains(unit))
            {
                continue;
            }
            addedUnits.Add(unit);
            unit.UnitDestroyed += OnUnitDestroyed;
        }

        NPCs = allUnits.FindAll(u => !(u is PlayerUnit));
    }

    private void OnUnitDestroyed(object sender, AttackEventArgs e)
    {
        allUnits.Remove(sender as Unit);
    }

    public void PlacePlayerUnitsInLimbo(List<PlayerUnit> playerUnits)
    {
        foreach (PlayerUnit playerUnit in playerUnits)
        {
            playerUnit.PlaceInLimbo(GameMap.Instance.limboCell);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
            Destroy(this);
            return;
        }

        addedUnits = new List<Unit>();
    }

    public void SetPlayerUnitsToStartElevator(List<PlayerUnit> playerUnits, List<Cell> startElevator)
    {
        if (startElevator.FindAll(c => !c.BlocksMovement).ToList().Count < playerUnits.Count)
        {
            Debug.LogError("Not enough space in start elevator for all player units");
            foreach (PlayerUnit playerUnit in playerUnits)
            {
                if (!playerUnit.IsAlive())
                {
                    continue;
                }
                if (startElevator.Count > 0)
                {
                    playerUnit.ForceMove(startElevator[0]);
                }
            }
            return;
        }

        foreach (PlayerUnit playerUnit in playerUnits)
        {
            if (!playerUnit.IsAlive())
            {
                continue;
            }
            List<Cell> elevatorCells = startElevator.FindAll(c => !c.BlocksMovement);
            Cell randomCell = elevatorCells[UnityEngine.Random.Range(0, elevatorCells.Count)];
            playerUnit.ForceMove(startElevator.Find(c => !c.BlocksMovement));

            Vector3 randomRotation = UnityEngine.Random.insideUnitSphere;
            randomRotation.y = 0f;
            playerUnit.ForceRotate(randomRotation);
        }
    }
}
