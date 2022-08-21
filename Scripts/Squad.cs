using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squad : MonoBehaviour
{
    public static Squad Instance = null;

    public List<PlayerUnitLink> playerUnits;
    public int currentCredits
    {
        get {return _currentCredits;}
    }

    public PlayerUnit selectedUnit;

    private int _currentCredits;

    public void SelectUnit(PlayerUnit unit)
    {
        selectedUnit = unit;
        GameUI.Instance.SelectUnit(unit);
        CameraOgre.Instance.JumpToPosition(unit.transform.position);
    }

    private void OnLevelStarted()
    {
        // List<PlayerUnit> alive = GetAllAliveUnits();
        // if (alive.Count > 0)
        // {
        //     SelectUnit(alive[0]);
        // }
    }

    public PlayerUnit GetFirstAliveUnit()
    {
        foreach(PlayerUnitLink unitLink in playerUnits)
        {
            if (unitLink.unit.IsAlive())
            {
                return unitLink.unit;
            }
        }

        return null;
    }

    public int GetCurrentCredits()
    {
        return _currentCredits;
    }

    public PlayerUnitData GetUnitData(PlayerUnit unit)
    {
        foreach(PlayerUnitLink link in playerUnits)
        {
            if (link.unit == unit)
            {
                return link.unitData;
            }
        }

        return null;
    }

    public void AddCredits(int credits, bool log = true)
    {
        _currentCredits += credits;
        if (log)
        {
            GameLog.Instance.AddMessage("Gained " + credits + GameGlobals.CREDITS_ABBREVIATION + ".");
        }
        GameUI.Instance.UpdateCredits(_currentCredits);
    }

    public bool DeductCredits(int price, bool log = true)
    {
        if (price > _currentCredits)
        {
            return false;
        }

        _currentCredits -= price;
        if (log)
        {
            GameLog.Instance.AddMessage("Deducted " + price + GameGlobals.CREDITS_ABBREVIATION + ".");
        }

        GameUI.Instance.UpdateCredits(_currentCredits);
        
        return true;
    }

    public List<PlayerUnit> GetAllUnits()
    {
        List<PlayerUnit> units = new List<PlayerUnit>();
        foreach (PlayerUnitLink unitLink in playerUnits)
        {
            units.Add(unitLink.unit);
        }
        return units;
    }

    public List<PlayerUnit> GetAllAliveUnits()
    {
        List<PlayerUnit> units = new List<PlayerUnit>();
        foreach (PlayerUnitLink unitLink in playerUnits)
        {
            if (unitLink.unit.IsAlive())
            {
                units.Add(unitLink.unit);
            }
        }
        return units;
    }

    public void Init()
    {
        GameUI.Instance.Init(playerUnits);
        
        GameUI.Instance.UpdateCredits(_currentCredits);
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
            Destroy(this);
            return;
        }

        GameMaster.Instance.GameStarting += Init;
        GameMaster.Instance.LevelStarted += OnLevelStarted;
    }

    public List<PlayerUnit> GetUnitsInSameRoom(PlayerUnit selectedUnit)
    {
        List<PlayerUnit> unitsInRoom = new List<PlayerUnit>();

        foreach (PlayerUnit unit in GetAllAliveUnits())
        {
            if (GameMap.Instance.AreCellsInSameRoom(unit.Cell, selectedUnit.Cell, true))
            {
                unitsInRoom.Add(unit);
            }
        }

        return unitsInRoom;
    }

    public void ChargeScoundrelsEnergy(int chargeAmount)
    {
        foreach(PlayerUnit unit in GetAllAliveUnits())
        {
            unit.RechargeEnergy(chargeAmount);
        }
    }
}

[System.Serializable]
public class PlayerUnitLink
{
    public PlayerUnit unit;
    public PlayerUnitData unitData;
}
