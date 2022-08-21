using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LootableItem
{
    public abstract EquipmentSlot GetSlotSize();
    public abstract bool EquipItem(UberUnit unit);
    public abstract Sprite GetIcon();
    public abstract string GetItemName();
    public abstract bool UnitHasRoomForItem(UberUnit unit);
    public abstract int GetPrice();

    public virtual string ToDescription()
    {
        return "";
    }

    public static LootableItem CreateItem(LootableItemData data, int stackSize = -1)
    {
        if (data is AmmoData)
        {
            if (stackSize == -1)
            {
                return new AmmoStack(((AmmoData) data).stackSize, (AmmoData) data);
            }
            else
            {
                return new AmmoStack(stackSize, (AmmoData) data);
            }
        }
        else if (data is WeaponData)
        {
            return new Weapon((WeaponData) data);
        }
        else if (data is UnitCoreData)
        {
            return new UnitCore((UnitCoreData) data);
        }
        else if (data is ActiveMacrochipData)
        {
            return new ActiveMacrochip((ActiveMacrochipData) data);
        }
        else if (data is EquipmentData)
        {
            // Debug.Log("Item: " + data.itemName + " is handled as equipment base type EquipmentData");
            return new Equipment((EquipmentData) data);
        }
        else
        {
            Debug.LogError("Unrecognized LootableItemData");
            return null;
        }
    }

    public virtual bool RequiresConfirmation()
    {
        return false;
    }

    public virtual string GetConfirmationMessage(PlayerUnit unit)
    {
        return "";
    }
}
