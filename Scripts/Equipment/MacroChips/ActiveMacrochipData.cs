using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveMacrochipData : ActiveEquipmentData
{
    public int energyCost;

    [Tooltip("Cooldown in turns before macrochip can be activated again.")]
    public int cooldownInTurns = 1;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (energyCost < 0) energyCost = 0;
        if (cooldownInTurns < 0) cooldownInTurns = 0;
    }
    
    public override void Activate(Unit activatingUnit, Equipment equipment)
    {
        UberUnit uberUnit = (UberUnit) activatingUnit;
        if (uberUnit == null) return;

        if (!(equipment is ActiveMacrochip))
        {
            Debug.LogError("Wrong type equipment and data matched.");
            return;
        }

        ActiveMacrochip chip = (ActiveMacrochip) equipment;
        chip.StartCooldown(cooldownInTurns, activatingUnit.PlayerNumber);

        uberUnit.currentEnergy -= energyCost;

        base.Activate(activatingUnit, equipment);
    }
    
    public override void Activate(Unit activatingUnit, Unit targetUnit, Equipment equipment)
    {
        UberUnit uberUnit = (UberUnit) activatingUnit;
        if (uberUnit == null) return;

        if (!(equipment is ActiveMacrochip))
        {
            Debug.LogError("Wrong type equipment and data matched.");
            return;
        }

        ActiveMacrochip chip = (ActiveMacrochip) equipment;
        chip.StartCooldown(cooldownInTurns, activatingUnit.PlayerNumber);

        uberUnit.currentEnergy -= energyCost;

        base.Activate(activatingUnit, targetUnit, equipment);
    }
    
    public override void Activate(Unit activatingUnit, Cell targetCell, Equipment equipment)
    {
        UberUnit uberUnit = (UberUnit) activatingUnit;
        if (uberUnit == null) return;

        if (!(equipment is ActiveMacrochip))
        {
            Debug.LogError("Wrong type equipment and data matched.");
            return;
        }

        ActiveMacrochip chip = (ActiveMacrochip) equipment;
        chip.StartCooldown(cooldownInTurns, activatingUnit.PlayerNumber);

        uberUnit.currentEnergy -= energyCost;

        base.Activate(activatingUnit, targetCell, equipment);
    }

    public override bool CanActivate(Unit activatingUnit)
    {
        if (!(activatingUnit is UberUnit))
        {
            return false;
        }

        UberUnit uberUnit = (UberUnit) activatingUnit;

        if (uberUnit.currentEnergy < energyCost)
        {
            return false;
        }

        return true;
    }

    public override string ToDescription()
    {
        string desc = "";

        desc += "Energy cost: " + energyCost + "\n";
        desc += "Cooldown: " + cooldownInTurns.ToString() + " turns\n";

        desc += base.ToDescription();

        return desc;
    }
}
