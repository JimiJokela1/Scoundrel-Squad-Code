using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileHover : WindowOnHoverBase
{
    SquareTile3D tile;

    private void Awake()
    {
        tile = GetComponent<SquareTile3D>();
        if (tile == null)
        {
            Debug.LogError("Cannot access attached UberUnit");
            Destroy(this);
        }

        tile.CellHighlighted += OnHighlighted;
        tile.CellDehighlighted += OnUnhighlighted;
    }

    protected override void SetText()
    {
        if (tile.tileData == LevelData.TileData.StartElevator)
        {
            infoTextTitle = "Elevator";
            infoText = "This elevator is where the scoundrels arrived on this level.";
        }

        if (tile.tileData == LevelData.TileData.EndElevator)
        {
            infoTextTitle = "Elevator";
            infoText = "This elevator can take the scoundrels to a new level.";
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
}
