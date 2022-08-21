using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Equippable core that defines unit stats and equipment slots
/// </summary>
[CreateAssetMenu(menuName = "Equipment/Core", fileName = "UnitCore.asset")]
public class UnitCoreData : LootableItemData
{
    public int maxHP;
    public int maxActionPoints;
    public int movementPoints;
    public int maxEnergy;
    public int smallSlots;
    public int bigSlots;

    public override string ToDescription()
    {
        string desc = "";

        desc += "HP: " + maxHP.ToString() + "\n";
        desc += "AP: " + maxActionPoints.ToString() + "\n";
        desc += "MP: " + movementPoints.ToString() + "\n";
        desc += "Energy: " + maxEnergy.ToString() + "\n";
        desc += "Small slots: " + smallSlots.ToString() + "\n";
        desc += "Big slots: " + bigSlots.ToString() + "\n";

        desc += base.ToDescription();
        return desc;
    }
}
