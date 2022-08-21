using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSymbolHover : WindowOnHoverBase
{
    PlayerUnit unit;

    public void Init(PlayerUnit unit)
    {
        this.unit = unit;
    }

    protected override void SetText()
    {
        if (unit == null) return;

        infoTextTitle = "Movement";
        string desc = "";
        desc += "Movement points (MP): " + unit.MovementPoints.ToString() + "/" + unit.TotalMovementPoints.ToString() + ".\n\n";

        string modifiersText = "";

        foreach (Equipment equipment in unit.equipped)
        {
            if (equipment.data.GetMovementModifier() > 0)
            {
                modifiersText += equipment.GetItemName() + ": +" + equipment.data.GetMovementModifier().ToString() + "\n";
            }
            else if (equipment.data.GetMovementModifier() < 0)
            {
                modifiersText += equipment.GetItemName() + ": " + equipment.data.GetMovementModifier().ToString() + "\n";
            }
        }

        foreach (Buff buff in unit.Buffs)
        {
            if (buff.GetMovementModifier() > 0)
            {
                modifiersText += buff.buffSourceName + ": +" + buff.GetMovementModifier().ToString() + "\n";
            }
            else if (buff.GetMovementModifier() < 0)
            {
                modifiersText += buff.buffSourceName + ": " + buff.GetMovementModifier().ToString() + "\n";
            }
        }

        if (modifiersText != "")
        {
            desc += "Modifiers: \n";
            desc += modifiersText;
        }

        infoText = desc;
    }
}
