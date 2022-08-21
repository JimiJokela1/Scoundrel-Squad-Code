using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellGridStateUnitActionMode : CellGridState
{
    private UberUnit _unit;
    private HashSet<Cell> _possibleTargetCells;
    private List<Unit> _unitsInRange;

    private Cell _unitCell;

    private List<Cell> _currentActionPath;

    public CellGridStateUnitActionMode(CellGrid cellGrid, Unit unit) : base(cellGrid)
    {
        if (unit is UberUnit)
        {
            _unit = (UberUnit) unit;
        }
        else
        {
            Debug.LogError("Wrong unit type");
        }

        _possibleTargetCells = new HashSet<Cell>();
        _unitsInRange = new List<Unit>();
        _currentActionPath = new List<Cell>();
    }

    public override bool IsPlayerTurn()
    {
        return true;
    }

    public override bool OnUnitEquipmentClicked(PlayerUnit unit, Equipment equipment)
    {
        if (unit == _unit)
        {
            if (unit.activeEquipment == equipment)
            {
                if (unit.TakeAction())
                {
                    if (unit.ActionPoints > 0)
                    {
                        _cellGrid.CellGridState = new CellGridStateUnitActionMode(_cellGrid, unit);
                    }
                    else
                    {
                        _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, unit);
                    }
                    return true;
                }
            }

            if (unit.OnEquipmentSelected(equipment))
            {
                if (unit.TryReload(equipment))
                {
                    if (unit.ActionPoints > 0)
                    {
                        _cellGrid.CellGridState = new CellGridStateUnitActionMode(_cellGrid, unit);
                    }
                    else
                    {
                        _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, unit);
                    }
                }
                else
                {
                    _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, unit);
                }

                return true;
            }
        }

        return false;
    }

    public override void OnCellClicked(Cell cell)
    {
        if (_unit.isMoving)
            return;
        if (cell.BlocksLineOfSight || !_possibleTargetCells.Contains(cell))
        {
            return;
        }

        if (!_unit.TakeAction(cell))
        {
            _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, _unit);
            return;
        }

        if (_unit.ActionPoints > 0)
        {
            _cellGrid.CellGridState = new CellGridStateUnitActionMode(_cellGrid, _unit);
        }
        else if (_unit.MovementPoints > 0)
        {
            _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, _unit);
        }
        else
        {
            _cellGrid.CellGridState = new CellGridStateWaitingForInput(_cellGrid);
        }
    }

    public override void OnUnitClicked(Unit unit)
    {
        if (_unit.isMoving)
            return;

        if (unit.Equals(_unit))
        {
            _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, _unit);
            return;
        }

        if (_unitsInRange.Contains(unit))
        {
            _unit.TakeAction(unit);

            if (_unit.ActionPoints > 0)
            {
                _cellGrid.CellGridState = new CellGridStateUnitActionMode(_cellGrid, _unit);
            }
            else if (_unit.MovementPoints > 0)
            {
                _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, _unit);
            }
            else
            {
                _cellGrid.CellGridState = new CellGridStateWaitingForInput(_cellGrid);
            }
            return;
        }

        if (unit.PlayerNumber.Equals(_unit.PlayerNumber))
        {
            _cellGrid.CellGridState = new CellGridStateUnitSelected(_cellGrid, unit);
            return;
        }
    }

    public override void OnCellDeselected(Cell cell)
    {
        base.OnCellDeselected(cell);

        if (_possibleTargetCells.Contains(cell))
        {
            cell.UnMark();
            cell.MarkAsReachable();
        }

        foreach (var pathCell in _currentActionPath)
        {
            if (_possibleTargetCells.Contains(pathCell))
            {
                pathCell.UnMark();
                pathCell.MarkAsReachable();
            }
            else
            {
                pathCell.UnMark();
            }
        }
    }

    public override void OnCellSelected(Cell cell)
    {
        base.OnCellSelected(cell);
        // Tell targeting laser if currently targeted cell is a valid target so laser can color itself
        TargetingLaser.Instance.CellSelected(_possibleTargetCells.Contains(cell));
        if (!_possibleTargetCells.Contains(cell)) return;

        _currentActionPath = _unit.FindActionTargetingPath(_cellGrid.Cells, cell);
        foreach (var pathCell in _currentActionPath)
        {
            if (pathCell != cell)
            {
                pathCell.MarkAsPath();
            }
        }
    }

    public override void OnUnitHighlighted(Unit unit)
    {
        if (_unitsInRange.Contains(unit) && unit is UberUnit)
        {
            TargetingCrosshair.Instance.Show(_unit, (UberUnit) unit);

            // CursorMaster.Instance.SetHighlightedCursor();
        }
    }

    public override void OnUnitUnhighlighted(Unit unit)
    {
        TargetingCrosshair.Instance.Hide();
    }

    public override void OnStateEnter()
    {
        if (!(_unit is UberUnit))
        {
            _cellGrid.CellGridState = new CellGridStateWaitingForInput(_cellGrid);
            return;
        }

        // Debug.Log("Action mode entered");
        base.OnStateEnter();

        SquadUI.equipmentNumberControlsEnabled = true;
        SquadUI.actionModeControlsEnabled = true;

        _unit.OnUnitSelected();
        _unitCell = _unit.Cell;

        if (_unit.ActionPoints <= 0) return;

        _possibleTargetCells = _unit.GetPossibleActionTargetTiles(_cellGrid.Cells);

        var impossibleCells = _cellGrid.Cells.Except(_possibleTargetCells);

        foreach (var cell in impossibleCells)
        {
            cell.UnMark();
        }
        foreach (var cell in _possibleTargetCells)
        {
            cell.MarkAsReachable();
        }

        foreach (var currentUnit in UnitManager.Instance.allUnits)
        {
            if (currentUnit.PlayerNumber.Equals(_unit.PlayerNumber))
                continue;

            if (_unit.IsUnitAttackable(currentUnit, _unit.Cell))
            {
                currentUnit.SetState(new UnitStateMarkedAsReachableEnemy(currentUnit));
                _unitsInRange.Add(currentUnit);
            }
        }

        if (_unitCell.GetNeighbours(_cellGrid.Cells).FindAll(c => c.MovementCost <= _unit.MovementPoints).Count == 0 &&
            _unitsInRange.Count == 0)
            _unit.SetState(new UnitStateMarkedAsFinished(_unit));

        CursorMaster.Instance.SetNormalCursor();

        TargetingLaser.Instance.Show(_unit);
    }

    public override void OnStateExit()
    {
        SquadUI.equipmentNumberControlsEnabled = false;
        SquadUI.actionModeControlsEnabled = false;
        TargetingCrosshair.Instance.Hide();
        TargetingLaser.Instance.Hide();

        _unit.OnUnitDeselected();
        foreach (var unit in _unitsInRange)
        {
            if (unit == null) continue;
            unit.SetState(new UnitStateNormal(unit));
        }
        foreach (var cell in _cellGrid.Cells)
        {
            cell.UnMark();
        }

        CursorMaster.Instance.SetNormalCursor();
    }
}
