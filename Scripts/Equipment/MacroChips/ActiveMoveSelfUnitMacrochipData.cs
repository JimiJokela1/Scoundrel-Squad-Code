using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Active Move Self Unit Macrochip", fileName = "ActiveMoveSelfUnitMacrochipData")]
public class ActiveMoveSelfUnitMacrochipData : ActiveMacrochipData
{
    public int range;

    public override void Activate(Unit activatingUnit, Cell targetCell, Equipment equipment)
    {
        base.Activate(activatingUnit, targetCell, equipment);

        UberUnit activatingUberUnit = (UberUnit) activatingUnit;
        if (activatingUberUnit == null) return;

        activatingUberUnit.ForceMove(targetCell);
    }

    public override bool CanTargetTile()
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
