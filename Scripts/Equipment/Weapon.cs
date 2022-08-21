using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : Equipment
{
    public int loadedAmmo;

    public WeaponData weaponData
    {
        get
        {
            if (!(data is WeaponData))
            {
                Debug.LogError("Weapon with non WeaponData data");
                return null;
            }
            return (WeaponData) data;
        }
    }

    public Weapon(WeaponData data)
    {
        this.data = data;
        loadedAmmo = data.ammoCapacity;
    }

    public override bool CanActivate(Unit activatingUnit)
    {
        if (!(data is WeaponData))
        {
            return false;
        }

        WeaponData weaponData = (WeaponData) data;

        if (loadedAmmo < weaponData.burstCount && !(activatingUnit is EnemyUnit) && weaponData.rangeType != WeaponRangeType.Melee)
        {
            return false;
        }

        return data.CanActivate(activatingUnit);
    }

    public override string ToDescription()
    {
        string desc = "";

        WeaponData weaponData = (WeaponData) data;
        if (weaponData == null)
        {
            Debug.LogWarning("Weapon with data not of type WeaponData");
            return base.ToDescription();
        }

        if (weaponData.ammoCapacity > 0)
        {
            desc += weaponData.ammoType.itemName + ": " + loadedAmmo.ToString() + "/" + weaponData.ammoCapacity.ToString() + "\n";
        }
        
        desc += base.ToDescription();
        return desc;
    }
}
