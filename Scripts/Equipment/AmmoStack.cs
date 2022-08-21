using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoStack : Equipment
{
    public int ammoCount;
    public AmmoData ammoData
    {
        get { return (AmmoData) data; }
    }

    public AmmoStack(int ammoCount, AmmoData ammoData)
    {
        this.ammoCount = ammoCount;
        this.data = ammoData;

        if (this.ammoData == null)
        {
            Debug.LogError("AmmoStack does not have a data of type AmmoData");
        }
    }

    public override string ToDescription()
    {
        string desc = "";

        if (ammoData == null)
        {
            Debug.LogWarning("AmmoStack does not have a data of type AmmoData");
            return base.ToDescription();
        }

        desc += ammoCount.ToString() + "/" + ammoData.stackSize.ToString() + "\n";

        desc += base.ToDescription();
        return desc;
    }

    public override bool UnitHasRoomForItem(UberUnit unit)
    {
        if (unit.HasRoomForEquipment(this))
        {
            return true;
        }
        
        List<AmmoStack> sameAmmoType = unit.GetAmmoStacksOfType(this.ammoData);

        int homelessAmmo = this.ammoCount;
        foreach(AmmoStack ammoStack in sameAmmoType)
        {
            homelessAmmo -= ammoData.stackSize - ammoStack.ammoCount;
        }

        if (homelessAmmo <= 0)
        {
            return true;
        }
        
        return false;
    }

    public override bool EquipItem(UberUnit unit)
    {
        return unit.EquipAmmoStack(this);
    }

    public override bool RequiresConfirmation()
    {
        // Doesn't require conventional confirmation message,
        // LootUI handles split confirmation
        return false;
    }

    public override string GetConfirmationMessage(PlayerUnit unit)
    {
        // For LootUI split confirmation
        return "";
    }

    public List<AmmoStack> SplitStack(int splitCount)
    {
        List<AmmoStack> splitStacks = new List<AmmoStack>();
        if (splitCount <= 0 || splitCount >= ammoCount)
        {
            splitStacks.Add(this);
            return splitStacks;
        }
        else
        {
            splitStacks.Add(new AmmoStack(splitCount, ammoData));
            splitStacks.Add(new AmmoStack(ammoCount - splitCount, ammoData));
            return splitStacks;
        }
    }

    /// <summary>
    /// Combines AmmoStacks and returns list of combined stacks or null if unable to combine
    /// </summary>
    public List<AmmoStack> CombineWithAmmoStack(AmmoStack other)
    {
        List<AmmoStack> combined = new List<AmmoStack>();
        if (other.ammoData == this.ammoData)
        {
            if (this.ammoCount + other.ammoCount <= ammoData.stackSize)
            {
                combined.Add(new AmmoStack(this.ammoCount + other.ammoCount, ammoData));
                return combined;
            }
            else
            {
                int overflow = this.ammoCount + other.ammoCount - ammoData.stackSize;
                combined.Add(new AmmoStack(ammoData.stackSize, ammoData));
                combined.Add(new AmmoStack(overflow, ammoData));
                return combined;
            }
        }
        else
        {
            return null;
        }
    }
}
