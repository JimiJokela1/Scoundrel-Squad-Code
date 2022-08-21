using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UberUnit))]
public class UnitStartingEquipment : MonoBehaviour
{
    public List<UnitCoreData> corePool;
    public int randomWeaponCount;
    public List<WeaponData> weaponPool;
    public int randomPassiveMacrochipCount;
    public List<PassiveMacrochipData> passiveMacrochipPool;

    private bool applied = false;

    public void ApplyStartingEquipment()
    {
        if (applied)
        {
            return;
        }

        applied = true;

        UberUnit unit = GetComponent<UberUnit>();
        if (unit == null)
        {
            return;
        }

        if (corePool == null || corePool.Count == 0)
        {
            Debug.LogError("Assign at least one core to starting equipment");
            unit.ChangeCore(MasterOfEquipment.Instance.GetFallbackCore());
        }
        else
        {
            unit.ChangeCore(corePool[Random.Range(0, corePool.Count)]);
        }

        if (weaponPool == null || weaponPool.Count == 0)
        {
            if (!(unit is PlayerUnit))
            {
                Debug.LogWarning("No starting weapons set for unit");
            }
        }
        else
        {
            for (int i = 0; i < randomWeaponCount; i++)
            {
                Weapon randomWeapon = new Weapon(weaponPool[Random.Range(0, weaponPool.Count)]);
                if (unit.HasRoomForEquipment(randomWeapon))
                {
                    unit.EquipItem(randomWeapon, false);
                }
                else
                {
                    Debug.Log("No room for starting equipment");
                }
            }
        }

        if (passiveMacrochipPool == null || passiveMacrochipPool.Count == 0)
        {

        }
        else
        {
            for (int i = 0; i < randomPassiveMacrochipCount; i++)
            {
                Equipment randomMacroChip = new Equipment(passiveMacrochipPool[Random.Range(0, passiveMacrochipPool.Count)]);
                if (unit.HasRoomForEquipment(randomMacroChip))
                {
                    unit.EquipItem(randomMacroChip, false);
                }
            }
        }
    }

    // private void Awake()
    // {
    //     ApplyStartingEquipment();    
    // }
}
