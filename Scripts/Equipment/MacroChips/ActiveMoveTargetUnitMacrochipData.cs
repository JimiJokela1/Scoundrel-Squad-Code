using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Active Move Target Unit Macrochip", fileName = "ActiveMoveTargetUnitMacrochipData")]
public class ActiveMoveTargetUnitMacrochipData : ActiveMacrochipData
{
    public int range;

    public override void Activate(Unit activatingUnit, Unit targetUnit, Equipment equipment)
    {
        base.Activate(activatingUnit, targetUnit, equipment);

        UberUnit targetUberUnit = (UberUnit) targetUnit;
        if (targetUberUnit == null) return;

        Cell targetCell = null;
        // Find the closest cell to target unit that is adjacent to activating unit and does not block movement
        // foreach (Cell surroundingCell in GameMap.Instance.cellGrid.GetSurroundingCells(activatingUnit.Cell))
        // {
        //     if (targetCell == null && !surroundingCell.BlocksMovement)
        //     {
        //         targetCell = surroundingCell;
        //     }

        //     if (targetCell != null)
        //     {
        //         if (!surroundingCell.BlocksMovement &&
        //             surroundingCell.GetDistance(targetUnit.Cell) < targetCell.GetDistance(targetUnit.Cell))
        //         {
        //             targetCell = surroundingCell;
        //         }
        //     }
        // }

        // Find first open cell on path to target unit, so usually cell next to activating unit unless theres cover there
        List<Cell> lineOfSightPath = GameMap.Instance.FindLineOfSight(activatingUnit.Cell, targetUnit.Cell);
        foreach(Cell pathCell in lineOfSightPath)
        {
            if (!pathCell.BlocksMovement)
            {
                targetCell = pathCell;
                break;
            }
        }

        if (targetCell != null)
        {
            targetUberUnit.ForceMove(targetCell);
        }
    }

    public override bool CanTargetUnit()
    {
        return true;
    }

    public override int GetRange()
    {
        return range;
    }

    public override string ToDescription()
    {
        string desc = "";

        desc += "Range: " + range.ToString() + "\n";

        desc += base.ToDescription();
        return desc;
    }
}
