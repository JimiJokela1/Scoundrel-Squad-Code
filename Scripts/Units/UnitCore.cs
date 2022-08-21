using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCore : LootableItem
{
    public List<Equipment> equipped;
    public UnitCoreData data;

    public UnitCore()
    {
        this.data = MasterOfEquipment.Instance.fallbackCore;
        if (equipped == null)
        {
            equipped = new List<Equipment>();
        }
    }

    public UnitCore(UnitCoreData core)
    {
        this.data = core;
        if (equipped == null)
        {
            equipped = new List<Equipment>();
        }
    }

    public override bool RequiresConfirmation()
    {
        return true;
    }

    public override string GetConfirmationMessage(PlayerUnit unit)
    {
        string message = "Are you sure you want to swap the core? The old core will be destroyed and unrecoverable. ";

        if (unit.core != null && unit.core.data != null)
        {
            UnitCoreData old = unit.core.data;
            message += "\n";
            message += "Max HP: " + old.maxHP + " -> " + data.maxHP + "\n";
            message += "Max Energy: " + old.maxEnergy + " -> " + data.maxEnergy + "\n";
            message += "AP: " + old.maxActionPoints + " -> " + data.maxActionPoints + "\n";
            message += "MP: " + old.movementPoints + " -> " + data.movementPoints + "\n";
            message += "Big slots: " + old.bigSlots + " -> " + data.bigSlots + "\n";
            message += "Small slots: " + old.smallSlots + " -> " + data.smallSlots + "\n";
        }
        else
        {
            message += "\n";
            message += "Max HP: 0 -> " + data.maxHP + "\n";
            message += "Max Energy: 0 -> " + data.maxEnergy + "\n";
            message += "AP: 0 -> " + data.maxActionPoints + "\n";
            message += "MP: 0 -> " + data.movementPoints + "\n";
            message += "Big slots: 0 -> " + data.bigSlots + "\n";
            message += "Small slots: 0 -> " + data.smallSlots + "\n";
        }

        return message;
    }

    public override int GetPrice()
    {
        return data.price;
    }

    public void Initialize(UberUnit unit)
    {
        if (equipped == null)
        {
            equipped = new List<Equipment>();
        }

        if (unit == null)
        {
            Debug.LogError("Trying to init core with null unit");
            return;
        }

        if (data == null)
        {
            Debug.LogError("Null UnitCoreData, please set a valid UnitCoreData before initializing");
            return;
        }

        unit.UpdateCoreStatsToUnit(data);
    }

    public void Switch(UnitCoreData newCore, UberUnit unit)
    {
        if (data == null)
        {
            data = newCore;
        }
        else
        {
            List<Equipment> previousEquipped = new List<Equipment>(equipped);

            foreach (Equipment equipment in previousEquipped)
            {
                unit.UnequipItem(equipment);
            }

            data = newCore;

            foreach(Equipment equipment in previousEquipped)
            {
                unit.EquipItem(equipment);
            }
        }
    }

    public bool HasRoomForEquipment(Equipment equipment, List<Equipment> equipped)
    {
        int equippedSmall = 0;
        int equippedBig = 0;

        foreach(Equipment eq in equipped)
        {
            if (eq.data.slotSize == EquipmentSlot.Small)
            {
                equippedSmall++;
            }
            else if (eq.data.slotSize == EquipmentSlot.Big)
            {
                equippedBig++;
            }
        }

        if (equipment.data.slotSize == EquipmentSlot.Small)
        {
            if (data.smallSlots > equippedSmall)
            {
                return true;
            }
        }
        else if (equipment.data.slotSize == EquipmentSlot.Big)
        {
            if (data.bigSlots > equippedBig)
            {
                return true;
            }
        }

        return false;
    }

    public override EquipmentSlot GetSlotSize()
    {
        // Only for displaying in inventories
        return EquipmentSlot.Big;
    }

    public override bool EquipItem(UberUnit unit)
    {
        unit.ChangeCore(this.data);
        return true;
    }

    public override bool UnitHasRoomForItem(UberUnit unit)
    {
        return true;
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

        desc += data.ToDescription();
        return desc;
    }
}
