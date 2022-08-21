using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/AmmoType", fileName = "AmmoType")]
public class AmmoData : EquipmentData
{
    public DamageType damageType;
    public int stackSize;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (stackSize < 1) stackSize = 1;
    }

    public override string ToDescription()
    {
        string desc = "";

        desc += base.ToDescription();
        return desc;
    }
}
