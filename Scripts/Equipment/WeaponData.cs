using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Weapon", fileName = "WeaponAsset.asset")]
public class WeaponData : ActiveEquipmentData
{
    [Tooltip("If true, can be targeted on empty tiles also. If false, can only be targeted at units.")]
    public bool canTargetTile;

    public int range;

    public WeaponRangeType rangeType;

    // public DamageType damageType;
    public DamageType damageType
    {
        get
        {
            return ammoType.damageType;
        }
    }
    public int burstCount;
    public int minDamage;
    public int maxDamage;
    public int accuracyModifier;
    public int criticalChanceModifier;

    public AmmoData ammoType;
    public int ammoCapacity;

    public List<string> synergyTags;

    [Range(0f, 1f)]
    public int chanceOfStun;
    public int stunAmount;

    [Range(0f, 1f)]
    public int pushChance;
    public int pushStrength;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (range < 0) range = 0;
        if (burstCount < 1) burstCount = 1;
        if (minDamage < 0) minDamage = 0;
        if (maxDamage < 0) maxDamage = 0;
        if (maxDamage < minDamage) maxDamage = minDamage;
        if (ammoCapacity < 0) ammoCapacity = 0;
        if (stunAmount < 0) stunAmount = 0;
        if (pushStrength < 0) pushStrength = 0;
    }

    public override int GetRange()
    {
        return range;
    }

    public override bool CanTargetTile()
    {
        return canTargetTile;
    }

    public override string ToDescription()
    {
        string desc = "";

        desc += "Range: " + range.ToString() + "\n";
        // desc += System.Enum.GetName(typeof(WeaponRangeType), rangeType) + "\n";
        desc += "Damage: " + System.Enum.GetName(typeof(DamageType), damageType) + " " + minDamage.ToString() + "-" + maxDamage.ToString() + "\n";
        if (burstCount > 1)
        {
            desc += "Burst: " + burstCount.ToString() + "\n";
        }
        else
        {
            desc += "Single fire\n";
        }

        if (accuracyModifier > 0) desc += "Acc: +" + accuracyModifier.ToString() + "%\n";
        if (accuracyModifier < 0) desc += "Acc: " + accuracyModifier.ToString() + "%\n";

        if (criticalChanceModifier > 0) desc += "Crit: +" + criticalChanceModifier.ToString() + "%\n";
        if (criticalChanceModifier < 0) desc += "Crit: " + criticalChanceModifier.ToString() + "%\n";

        // Ammo will be handled in Weapon

        if (synergyTags != null && synergyTags.Count > 0)
        {
            for (int i = 0; i < synergyTags.Count; i++)
            {
                desc += synergyTags[i];
                if (i + 1 < synergyTags.Count) desc += ", ";
            }
            desc += "\n";
        }

        if (chanceOfStun > 0 && stunAmount > 0)
        {
            desc += "Stun: " + chanceOfStun.ToString() + "% for " + stunAmount.ToString() + " turns\n";
        }

        if (pushChance > 0 && pushStrength > 0)
        {
            desc += "Knockback: " + pushChance.ToString() + "% for " + pushStrength.ToString() + " tiles\n";
        }

        desc += base.ToDescription();
        return desc;
    }

    private void Shoot(Unit activatingUnit, Unit targetUnit, Equipment equipment)
    {
        int chanceToHit = 0;
        int criticalChance = 0;
        int damageMod = 0;

        if (activatingUnit is UberUnit && targetUnit is UberUnit)
        {
            UberUnit activatingUberUnit = (UberUnit) activatingUnit;
            UberUnit targetUberUnit = (UberUnit) targetUnit;

            chanceToHit = GetHitChance(activatingUberUnit, targetUberUnit, equipment);
            criticalChance = GetCriticalChance(activatingUberUnit, targetUberUnit, equipment);
            damageMod = GetTotalDamageModifier(activatingUberUnit, targetUberUnit, equipment);
        }
        else
        {
            Debug.LogError("Wrong type units, should be UberUnit");
            return;
        }

        // check if hit
        bool hit = (Random.Range(0, 100) < chanceToHit);

        // check if critical hit
        bool critical = (Random.Range(0, 100) < criticalChance);

        // TODO NEXT LEVEL: check stun and push

        // calc damage
        int damage = Random.Range(minDamage, maxDamage + 1) + damageMod;
        if (critical)
        {
            damage *= 2;
        }

        WeaponAttack attack = new WeaponAttack(damage, critical, damageType, rangeType);

        // log hit or miss and critical or not. Defender will log actual damage of a hit. 
        if (hit)
        {
            if (critical)
            {
                GameLog.Instance.AddMessage(activatingUnit.GetUnitName() + " attacked " + targetUnit.GetUnitName() + " with " + itemName + ". Attack hit with a " + chanceToHit + "% chance and was a critical hit with a " + criticalChance + "% chance.");
            }
            else
            {
                GameLog.Instance.AddMessage(activatingUnit.GetUnitName() + " attacked " + targetUnit.GetUnitName() + " with " + itemName + ". Attack hit with a " + chanceToHit + "% chance.");
            }
        }
        else
        {
            GameLog.Instance.AddMessage(activatingUnit.GetUnitName() + " attacked " + targetUnit.GetUnitName() + " with " + itemName + ". Attack missed with a " + chanceToHit + "% chance to hit.");
        }

        // Apply damage if hit
        if (hit)
        {
            activatingUnit.DealDamage(targetUnit, attack);
        }

        // Remove used ammo
        Weapon weapon = (Weapon) equipment;
        if (weapon.weaponData.rangeType != WeaponRangeType.Melee)
        {
            weapon.loadedAmmo -= 1;
        }
    }

    public override void Activate(Unit activatingUnit, Unit targetUnit, Equipment equipment)
    {
        // Shoot burstCount times
        for (int i = 0; i < burstCount; i++)
        {
            Shoot(activatingUnit, targetUnit, equipment);
        }

        PlayEffects(activatingUnit, targetUnit, burstCount);
    }

    public override int GetHitChance(UberUnit activatingUnit, UberUnit targetUnit, Equipment equipment)
    {
        // Start with global hit chance
        int chanceToHit = GameGlobals.HIT_CHANCE_FULL_VIEW;

        // Check if in cover
        List<Cell> path = GameMap.Instance.FindLineOfSight(activatingUnit.Cell, targetUnit.Cell);
        bool cover = GameMap.Instance.IsTargetCellInCover(path, activatingUnit.Cell, targetUnit.Cell);
        if (cover)
        {
            chanceToHit = GameGlobals.HIT_CHANCE_IN_COVER;
        }

        // Add weapon specific modifiers
        chanceToHit += accuracyModifier;

        // Get synergy bonuses
        chanceToHit += activatingUnit.GetWeaponHitChanceModifiers(synergyTags);

        // Get defences
        chanceToHit += targetUnit.GetDefenceHitChanceModifiers();

        return chanceToHit;
    }

    public override int GetCriticalChance(UberUnit activatingUnit, UberUnit targetUnit, Equipment equipment)
    {

        // Start with global hit chance
        int criticalChance = GameGlobals.BASE_CRITICAL_CHANCE;

        // Add weapon specific modifiers
        criticalChance += criticalChanceModifier;

        // Get synergy bonuses
        criticalChance += activatingUnit.GetWeaponCriticalChanceModifiers(synergyTags);

        // Get defences
        criticalChance += targetUnit.GetDefenceCriticalChanceModifiers();

        return criticalChance;
    }

    public override int GetTotalDamageModifier(UberUnit activatingUnit, UberUnit targetUnit, Equipment equipment)
    {
        int damageMod = 0;

        // Get synergy bonuses
        damageMod += activatingUnit.GetWeaponDamageModifiers(synergyTags);

        // Get defences
        damageMod += targetUnit.GetDefenceDamageModifiers();

        return damageMod;
    }

    public override string GetTargetingDescription(UberUnit activatingUnit, UberUnit targetUnit, Equipment equipment)
    {
        string desc = "";

        // Burst count
        if (burstCount > 1)
        {
            desc += "Burst: " + burstCount.ToString() + "\n";
        }

        // Hit chance
        // Start with global hit chance
        int chanceToHit = GameGlobals.HIT_CHANCE_FULL_VIEW;

        // Check if in cover
        List<Cell> path = GameMap.Instance.FindLineOfSight(activatingUnit.Cell, targetUnit.Cell);
        bool cover = GameMap.Instance.IsTargetCellInCover(path, activatingUnit.Cell, targetUnit.Cell);
        if (cover)
        {
            chanceToHit = GameGlobals.HIT_CHANCE_IN_COVER;
            desc += "Base hit chance: " + chanceToHit.ToString() + "%\n (in cover)";
        }
        else
        {
            desc += "Base hit chance: " + chanceToHit.ToString() + "%\n";
        }

        // Add weapon specific modifiers
        if (accuracyModifier != 0)
        {
            desc += itemName + ": " + accuracyModifier.ToStringWithSign() + "%\n";
        }

        // Get modifiers
        string bonuses = activatingUnit.GetWeaponHitChanceModifiersDescription(synergyTags);

        if (bonuses != "")
        {
            desc += "Attacker:\n";
            desc += bonuses;
        }

        // Get defences
        string defences = targetUnit.GetDefenceHitChanceModifiersDescription();

        if (defences != "")
        {
            desc += "Defender:\n";
            desc += defences;
        }

        desc += "--- Total hit chance: " + GetHitChance(activatingUnit, targetUnit, equipment).ToString() + "%\n\n";

        // Critical chance
        // Start with global critical hit chance
        int criticalChance = GameGlobals.BASE_CRITICAL_CHANCE;
        desc += "Base critical chance: " + criticalChance.ToString() + "%\n";

        // Add weapon specific modifiers
        if (criticalChanceModifier != 0)
        {
            desc += itemName + ": " + criticalChanceModifier.ToStringWithSign() + "%\n";
        }

        // Get modifiers
        bonuses = activatingUnit.GetWeaponCriticalChanceModifiersDescription(synergyTags);

        if (bonuses != "")
        {
            desc += "Attacker:\n";
            desc += bonuses;
        }

        // Get defences
        defences = targetUnit.GetDefenceCriticalChanceModifiersDescription();

        if (defences != "")
        {
            desc += "Defender:\n";
            desc += defences;
        }

        desc += "--- Total critical chance: " + GetCriticalChance(activatingUnit, targetUnit, equipment).ToString() + "%\n\n";

        // Damage
        desc += "Base damage: " + minDamage.ToString() + "-" + maxDamage.ToString() + "\n";

        // Get modifiers
        bonuses = activatingUnit.GetWeaponDamageModifiersDescription(synergyTags);

        if (bonuses != "")
        {
            desc += "Attacker:\n";
            desc += bonuses;
        }

        // Get defences
        defences = targetUnit.GetDefenceDamageModifiersDescription();

        if (defences != "")
        {
            desc += "Defender:\n";
            desc += defences;
        }

        int totalDamageMod = GetTotalDamageModifier(activatingUnit, targetUnit, equipment);
        if (totalDamageMod != 0)
        {
            desc += "--- Total damage: " + minDamage.ToString() + "-" + maxDamage.ToString() + " " + totalDamageMod.ToStringWithSign() + "\n\n";
        }
        else
        {
            desc += "--- Total damage: " + minDamage.ToString() + "-" + maxDamage.ToString() + "\n\n";
        }

        return desc;
    }

    public override bool CanActivate(Unit activatingUnit)
    {
        if (activationType == EquipmentActivationType.Passive)
        {
            return false;
        }

        return true;
    }

    public override bool CanTargetUnit()
    {
        return true;
    }

    protected override void PlayEffects(Unit activatingUnit, Unit targetUnit, int shots)
    {
        if (effectPrefab == null)
        {
            return;
        }
        GameObject effectsObj = Instantiate(effectPrefab, activatingUnit.transform);
        EquipmentEffect effects = effectsObj.GetComponent<EquipmentEffect>();
        if (effects == null)
        {
            Destroy(effectsObj);
            Debug.LogWarning("Equipment effect prefab must have EquipmentEffect component attached");
            return;
        }

        if (activatingUnit is UberUnit && targetUnit is UberUnit)
        {
            UberUnit unit = (UberUnit) activatingUnit;
            UberUnit target = (UberUnit) targetUnit;
            if (unit.gunBarrelPoint != null && target.targetPoint != null)
            {
                effects.PlayEffect(unit.gunBarrelPoint, target.targetPoint, new WeaponEffectArgs(shots, useEffectSounds));
            }
        }
    }
}

public class WeaponAttack
{
    public int damage;
    public bool critical;
    public DamageType damageType;
    public WeaponRangeType weaponRangeType;

    public WeaponAttack(int damage, bool critical, DamageType damageType, WeaponRangeType weaponRangeType)
    {
        this.damage = damage;
        this.critical = critical;
        this.damageType = damageType;
        this.weaponRangeType = weaponRangeType;
    }
}

public enum DamageType
{
    Light,
    Heavy,
    Energy
}

public enum WeaponRangeType
{
    Ranged,
    Melee
}
