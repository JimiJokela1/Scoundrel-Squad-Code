using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EquipmentData : LootableItemData
{
    public EquipmentSlot slotSize;
    public EquipmentActivationType activationType;

    [Tooltip("Raises unit's max armor (and current armor when first equipped)")]
    public int bonusArmor;

    [Tooltip("Passive modifier to movement points of equipper")]
    public int movementModifier;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (bonusArmor < 0) bonusArmor = 0;
    }

    public virtual void Activate(Unit activatingUnit, Unit targetUnit, Equipment equipment)
    {

    }

    public virtual void Activate(Unit activatingUnit, Cell targetCell, Equipment equipment)
    {

    }

    /// <summary>
    /// Activate without target. Called when equipped item is selected, then clicked again. 
    /// </summary>
    /// <param name="activatingUnit"></param>
    /// <param name="equipment"></param>
    public virtual void Activate(Unit activatingUnit, Equipment equipment)
    {

    }

    public virtual bool CanTargetTile()
    {
        return false;
    }

    public virtual bool CanTargetUnit()
    {
        return false;
    }

    public virtual bool CanActivateWithoutTarget()
    {
        return false;
    }

    public virtual bool RequiresLineOfSight()
    {
        return true;
    }

    public virtual int GetRange()
    {
        return 0;
    }

    public virtual bool CanActivate(Unit activatingUnit)
    {
        // By default always returns false, meaning it is a passive item.
        // Can be overridden in derived classes.
        return false;
    }

    public virtual int GetMovementModifier()
    {
        return movementModifier;
    }

    public virtual int GetHitChanceModifier(List<string> synergyTags)
    {
        return 0;
    }

    public virtual int GetCriticalChanceModifier(List<string> synergyTags)
    {
        return 0;
    }

    public virtual int GetDamageModifier(List<string> synergyTags)
    {
        return 0;
    }

    public virtual int GetDefenceHitChanceModifier()
    {
        return 0;
    }

    public virtual int GetDefenceCriticalHitChanceModifier()
    {
        return 0;
    }

    public virtual int GetDefenceDamageModifier()
    {
        return 0;
    }

    /// <summary>
    /// Gets Action point cost based on slot size
    /// </summary>
    /// <returns></returns>
    public int GetActionPointCost()
    {
        int actionPointCost = 0;

        if (slotSize == EquipmentSlot.Big)
        {
            actionPointCost = 2;
        }
        else if (slotSize == EquipmentSlot.Small)
        {
            actionPointCost = 1;
        }

        return actionPointCost;
    }

    public override string ToDescription()
    {
        string desc = "";

        if (movementModifier != 0)
        {
            desc += "MP: " + movementModifier.ToString() + "\n";
        }

        desc += base.ToDescription();

        return desc;
    }

    [System.Serializable]
    public class EquipmentSynergy
    {
        public string tag;
        public int bonusHitChance;
        public int bonusCriticalChance;
        public int bonusDamage;
    }

    public virtual int GetHitChance(UberUnit activatingUnit, UberUnit targetUnit, Equipment equipment)
    {
        return 0;
    }

    public virtual int GetCriticalChance(UberUnit activatingUnit, UberUnit targetUnit, Equipment equipment)
    {
        return 0;
    }

    public virtual int GetTotalDamageModifier(UberUnit activatingUnit, UberUnit targetUnit, Equipment equipment)
    {
        return 0;
    }

    public virtual string GetTargetingDescription(UberUnit activatingUnit, UberUnit targetUnit, Equipment equipment)
    {
        return "";
    }
}

public enum EquipmentSlot
{
    Big,
    Small
}

public enum EquipmentActivationType
{
    Active,
    Passive
}



