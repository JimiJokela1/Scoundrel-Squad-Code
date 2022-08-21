using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InteractablePoint))]
public class InteractablePointHover : WindowOnHoverBase
{
    InteractablePoint point;

    private void Awake()
    {
        point = GetComponent<InteractablePoint>();
        if (point == null)
        {
            Debug.LogError("Cannot access attached UberUnit");
            Destroy(this);
        }

        point.PointHighlighted += OnHighlighted;
        point.PointUnhighlighted += OnUnhighlighted;
    }

    protected override void SetText()
    {
        infoTextTitle = point.GetPointName();
        infoText = point.GetDescription();
    }

    private void OnHighlighted(object sender, EventArgs e)
    {
        PointerEntered();
    }

    private void OnUnhighlighted(object sender, EventArgs e)
    {
        PointerExited();
    }
}
