using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UberUnit))]
public class UberUnitHover : WindowOnHoverBase
{
    UberUnit unit;

    private bool targetingMode = false;

    private void Awake()
    {
        unit = GetComponent<UberUnit>();
        if (unit == null)
        {
            Debug.LogError("Cannot access attached UberUnit");
            Destroy(this);
            return;
        }

        unit.UnitHighlighted += OnHighlighted;
        unit.UnitDehighlighted += OnUnhighlighted;
    }

    public void SetTargetingMode(UberUnit attacker)
    {
        targetingMode = true;

        string title = unit.GetUnitName();
        string desc = attacker.GetTargetingDescription(unit);
        UpdateHoverInfoWindowText(title, desc);
    }

    protected override void SetText()
    {
        if (targetingMode)
        {

        }
        else
        {
            infoTextTitle = unit.GetUnitName();
            infoText = unit.ToDescription();
        }
    }

    private void OnHighlighted(object sender, EventArgs e)
    {
        PointerEntered();
    }

    private void OnUnhighlighted(object sender, EventArgs e)
    {
        PointerExited();
    }

    protected override void PointerExited()
    {
        targetingMode = false;
        base.PointerExited();
    }
}
