using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator
{
    public List<Cell> startElevator;
    public List<Cell> endElevator;

    [HideInInspector]
    public List<PlayerUnit> unitsInEndElevator;

    public Elevator(List<PlayerUnit> playerUnits, List<Cell> startElevator, List<Cell> endElevator)
    {
        this.startElevator = startElevator;
        this.endElevator = endElevator;

        unitsInEndElevator = new List<PlayerUnit>();

        foreach (PlayerUnit playerUnit in playerUnits)
        {
            playerUnit.UnitMoved += OnPlayerUnitMoved;
        }
    }

    private void OnPlayerUnitMoved(object sender, MovementEventArgs e)
    {
        PlayerUnit unit = (PlayerUnit) sender;
        if (endElevator.Contains(e.DestinationCell))
        {
            if (!unitsInEndElevator.Contains(unit))
            {
                unitsInEndElevator.Add(unit);
                GameUI.Instance.ShowElevatorButton();
            }
        }
        else if (unitsInEndElevator.Contains(unit))
        {
            unitsInEndElevator.Remove(unit);
            if (unitsInEndElevator.Count == 0)
            {
                GameUI.Instance.HideElevatorButton();
            }

        }
    }
}
