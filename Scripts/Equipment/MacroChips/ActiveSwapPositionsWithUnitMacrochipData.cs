using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Active Swap Position With Unit Macrochip", fileName = "ActiveSwapPositionsWithUnitMacrochipData")]
public class ActiveSwapPositionsWithUnitMacrochipData : ActiveMacrochipData
{
    public int range;

    public override void Activate(Unit activatingUnit, Unit targetUnit, Equipment equipment)
    {
        base.Activate(activatingUnit, targetUnit, equipment);

        UberUnit activatingUberUnit = (UberUnit) activatingUnit;
        if (activatingUberUnit == null) return;

        UberUnit targetUberUnit = (UberUnit) targetUnit;
        if (targetUberUnit == null) return;

        Cell targetCell = targetUnit.Cell;
        targetUberUnit.ForceMove(activatingUnit.Cell);

        activatingUberUnit.ForceMove(targetCell);
    }

    public override bool CanTargetUnit()
    {
        return true;
    }

    public override int GetRange()
    {
        return range;
    }

    public override bool RequiresLineOfSight()
    {
        return false;
    }

    public override string ToDescription()
    {
        string desc = "";

        desc += "Range: " + range.ToString() + "\n";

        desc += base.ToDescription();
        return desc;
    }
}
