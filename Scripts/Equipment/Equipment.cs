using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : LootableItem
{
    public EquipmentData data;
    public int armorPoints;

    public Equipment()
    {

    }

    public Equipment(EquipmentData data)
    {
        this.data = data;
        armorPoints = data.bonusArmor;
    }

    public static bool IsEmptyEquipment(Equipment equipment)
    {
        if (equipment == null || equipment.data == null)
        {
            return true;
        }
        return equipment.data == MasterOfEquipment.Instance.emptyEquipment;
    }

    public static Equipment EmptyEquipment()
    {
        Equipment equipment = new Equipment();
        equipment.data = MasterOfEquipment.Instance.emptyEquipment;
        return equipment;
    }

    public virtual void ApplyEquipEffects(UberUnit equippingUnit)
    {
        // if (data.bonusArmor > 0)
        // {
        //     equippingUnit.ModifyArmorPoints(data.bonusArmor, armorPoints);
        // }

        if (data.movementModifier != 0)
        {
            equippingUnit.ModifyMovementPoints(data.movementModifier);
        }
    }

    public virtual void UndoEquipEffects(UberUnit equippingUnit)
    {
        // if (data.bonusArmor > 0)
        // {
        //     equippingUnit.ModifyArmorPoints(-data.bonusArmor, -armorPoints);
        // }

        if (data.movementModifier != 0)
        {
            equippingUnit.ModifyMovementPoints(-data.movementModifier);
        }
    }

    public void Activate(Unit activatingUnit, Unit targetUnit)
    {
        data.Activate(activatingUnit, targetUnit, this);
    }

    public void Activate(Unit activatingUnit, Cell targetCell)
    {
        data.Activate(activatingUnit, targetCell, this);
    }

    public void Activate(Unit activatingUnit)
    {
        data.Activate(activatingUnit, this);
    }

    public virtual bool CanActivate(Unit activatingUnit)
    {
        return data.CanActivate(activatingUnit);
    }

    public int GetHitChanceModifier(List<string> synergyTags)
    {
        return data.GetHitChanceModifier(synergyTags);
    }

    public int GetCriticalChanceModifier(List<string> synergyTags)
    {
        return data.GetCriticalChanceModifier(synergyTags);
    }

    public int GetDamageModifier(List<string> synergyTags)
    {
        return data.GetDamageModifier(synergyTags);
    }

    public int GetDefenceHitChanceModifier()
    {
        return data.GetDefenceHitChanceModifier();
    }

    public int GetDefenceCriticalChanceModifier()
    {
        return data.GetDefenceCriticalHitChanceModifier();
    }

    public int GetDefenceDamageModifier()
    {
        return data.GetDefenceDamageModifier();
    }

    public bool CanTargetTile()
    {
        return data.CanTargetTile();
    }

    public bool CanTargetUnit()
    {
        return data.CanTargetUnit();
    }

    public bool CanActivateWithoutTarget()
    {
        return data.CanActivateWithoutTarget();
    }

    public int GetRange()
    {
        return data.GetRange();
    }

    public bool RequiresLineOfSight()
    {
        return data.RequiresLineOfSight();
    }

    /// <summary>
    /// Gets Action point cost based on slot size
    /// </summary>
    public int GetActionPointCost()
    {
        return data.GetActionPointCost();
    }

    public override EquipmentSlot GetSlotSize()
    {
        return data.slotSize;
    }

    public override bool EquipItem(UberUnit unit)
    {
        return unit.EquipItem(this);
    }

    public override bool UnitHasRoomForItem(UberUnit unit)
    {
        return unit.HasRoomForEquipment(this);
    }

    public override Sprite GetIcon()
    {
        return data.icon;
    }

    public override string GetItemName()
    {
        return data.itemName;
    }

    public override string ToDescription()
    {
        string desc = "";

        if (data.bonusArmor != 0)
        {
            desc += "Armor: " + armorPoints.ToString() + "/" + data.bonusArmor.ToString() + "\n";
        }

        desc += data.ToDescription();
        return desc;
    }

    public override int GetPrice()
    {
        return data.price;
    }

    public virtual int GetHitChance(UberUnit activatingUnit, UberUnit targetUnit)
    {
        return data.GetHitChance(activatingUnit, targetUnit, this);
    }

    public virtual string GetTargetingDescription(UberUnit activatingUnit, UberUnit targetUnit)
    {
        return data.GetTargetingDescription(activatingUnit, targetUnit, this);
    }

    public virtual bool IsEnabled()
    {
        return true;
    }
}
