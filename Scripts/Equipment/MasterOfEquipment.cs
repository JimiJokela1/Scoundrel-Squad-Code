using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterOfEquipment : MonoBehaviour
{
    private static MasterOfEquipment _Instance = null;
    public static MasterOfEquipment Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<MasterOfEquipment>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find any instance of type MasterOfEquipment in scene");
                }
            }
            return _Instance;
        }
    }

    /// <summary>
    /// Obsolete
    /// </summary>
    public List<Equipment> armorThatHasAlreadyBeenEquipped;

    public EquipmentData emptyEquipment;
    public UnitCoreData fallbackCore;

    public AmmoData lightAmmo;
    public AmmoData heavyAmmo;
    public AmmoData energyAmmo;

    /// <summary>
    /// Obsolete
    /// </summary>
    /// <param name="equipment"></param>
    public bool IsArmorFresh(Equipment equipment)
    {
        if (armorThatHasAlreadyBeenEquipped == null)
        {
            armorThatHasAlreadyBeenEquipped = new List<Equipment>();
        }

        return !armorThatHasAlreadyBeenEquipped.Contains(equipment);
    }

    public UnitCoreData GetFallbackCore()
    {
        return fallbackCore;
    }

    /// <summary>
    /// Obsolete
    /// </summary>
    /// <param name="equipment"></param>
    public void RegisterArmorAsEquipped(Equipment equipment)
    {
        if (armorThatHasAlreadyBeenEquipped == null)
        {
            armorThatHasAlreadyBeenEquipped = new List<Equipment>();
        }

        armorThatHasAlreadyBeenEquipped.Add(equipment);
    }

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else if (_Instance != this)
        {
            Debug.LogError("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
            Destroy(this);
            return;
        }
    }
}