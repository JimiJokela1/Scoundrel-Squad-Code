using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveMacrochip : Equipment
{
    public int activeCooldown {get; protected set;}
    private int lastActivatedPlayerNumber;

    public ActiveMacrochip(ActiveMacrochipData data)
    {
        this.data = data;
        activeCooldown = 0;

        GameMap.Instance.cellGrid.TurnEnded += AdvanceCooldown;
    }

    public virtual void StartCooldown(int cooldown, int activatorPlayerNumber)
    {
        activeCooldown = cooldown;
        lastActivatedPlayerNumber = activatorPlayerNumber;
    }

    protected virtual void AdvanceCooldown(object sender, EventArgs e)
    {
        if (lastActivatedPlayerNumber == GameMap.Instance.cellGrid.CurrentPlayerNumber)
        {
            if (activeCooldown > 0)
            {
                activeCooldown--;
            }
        }
    }

    public override bool CanActivate(Unit activatingUnit)
    {
        if (activeCooldown > 0)
        {
            return false;
        }

        return data.CanActivate(activatingUnit);
    }

    public override string ToDescription()
    {
        string desc = "";

        if (activeCooldown > 0)
        {
            desc += "On Cooldown: " + activeCooldown.ToString() + " turns\n";
        }
        
        desc += base.ToDescription();
        return desc;
    }

    public override bool IsEnabled()
    {
        return activeCooldown <= 0;
    }
}
