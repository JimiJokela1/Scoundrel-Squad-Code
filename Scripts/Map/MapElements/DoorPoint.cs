using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorPoint : InteractablePoint
{
    public Animator doorAnimator;
    public int turnsToClose = 1;

    [HideInInspector]
    public bool isOpen = false;

    private int doorOpenerPlayer;
    private int doorOpenTurns;

    public override void Init(Cell cell, InteractablePointContents contents, bool closeConfirmRequired = false, string confirmationMessage = "")
    {
        if (inited) return;
        base.Init(cell, contents, closeConfirmRequired, confirmationMessage);

        GameMap.Instance.cellGrid.TurnEnded += OnTurnChange;

        inited = true;
    }

    private void OnTurnChange(object sender, EventArgs e)
    {
        if (!isOpen)
        {
            return;
        }

        if ((sender as CellGrid).CurrentPlayerNumber == doorOpenerPlayer)
        {
            doorOpenTurns++;
            if (doorOpenTurns >= turnsToClose)
            {
                CloseDoor();
            }
        }
    }

    public override bool IsInteractable()
    {
        return !isOpen;
    }

    public void CloseDoor()
    {
        if (!isOpen) return;

        // If unit is standing on open door, cannot close
        if (UnitManager.Instance.GetActiveUnitOnCell(cell) != null) return;

        cell.BlocksLineOfSight = true;
        cell.BlocksMovement = true;
        cell.IsCover = true;
        isOpen = false;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Close");
        }

        // Also close all neighbouring doors
        List<Cell> neighbouringCells = GameMap.Instance.GetCellNeighbours(cell);
        if (neighbouringCells != null || neighbouringCells.Count > 0)
        {
            foreach (Cell neighbour in neighbouringCells)
            {
                DoorPoint door = InteractablePointManager.Instance.GetDoorPointOnCell(neighbour);
                if (door != null)
                {
                    door.CloseDoor();
                }
            }
        }
    }

    public void OpenDoor()
    {
        if (isOpen) return;

        cell.BlocksLineOfSight = false;
        cell.BlocksMovement = false;
        cell.IsCover = false;
        isOpen = true;

        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger("Open");
        }

        doorOpenerPlayer = GameMap.Instance.cellGrid.CurrentPlayerNumber;
        doorOpenTurns = 0;

        // Also open all neighbouring doors
        List<Cell> neighbouringCells = GameMap.Instance.GetCellNeighbours(cell);
        if (neighbouringCells != null || neighbouringCells.Count > 0)
        {
            foreach (Cell neighbour in neighbouringCells)
            {
                DoorPoint door = InteractablePointManager.Instance.GetDoorPointOnCell(neighbour);
                if (door != null)
                {
                    door.OpenDoor();
                }
            }
        }
    }

    public override void InteractWithPoint()
    {
        if (!isOpen)
        {
            OpenDoor();

            // Update visibility
            GameMap.Instance.UpdateFogOfWar(Squad.Instance.GetAllAliveUnits());
            GameMap.Instance.UpdateWallFade(Squad.Instance.GetAllAliveUnits());
        }
    }

    public override string GetPointName()
    {
        return "Door";
    }

    public override string GetDescription()
    {
        return "It's a door, perhaps to another room?!";
    }
}
