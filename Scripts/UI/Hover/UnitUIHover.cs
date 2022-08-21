using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitUIHover : WindowOnHoverBase
{
    PlayerUnit unit;

    public void Init(PlayerUnit unit)
    {
        this.unit = unit;
    }

    protected override void SetText()
    {
        if (unit != null)
        {
            infoTextTitle = unit.GetUnitName();
            infoText = unit.ToDescription();
        }
    }
}
