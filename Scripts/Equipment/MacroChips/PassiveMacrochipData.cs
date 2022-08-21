using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Passive Macrochip", fileName = "PassiveMacrochip.asset")]
public class PassiveMacrochipData : EquipmentData
{
    [Tooltip("Synergies that this equipment gives to weapons")]
    public List<EquipmentSynergy> synergies;

    [Tooltip("Bonus to hit chance for any weapon")]
    public int bonusHitChance;
    [Tooltip("Bonus to critical hit chance for any weapon")]
    public int bonusCriticalChance;
    [Tooltip("Bonus to damage for any weapon")]
    public int bonusDamage;

    [Tooltip("Reduces attack hit chance against unit equipped with this")]
    public int defenceHitChanceMod;
    [Tooltip("Reduces attack critical hit chance against unit equipped with this")]
    public int defenceCriticalChanceMod;
    [Tooltip("Reduces attack damage against unit equipped with this")]
    public int defenceDamageMod;

    protected override void OnValidate()
    {
        base.OnValidate();

        activationType = EquipmentActivationType.Passive;
    }

    public override int GetHitChanceModifier(List<string> synergyTags)
    {
        int mod = bonusHitChance;
        if (synergies == null || synergies.Count == 0)
        {
            return mod;
        }

        foreach (var syn in synergies)
        {
            if (synergyTags.Contains(syn.tag))
            {
                mod += syn.bonusHitChance;
            }
        }

        return mod;
    }

    public override int GetCriticalChanceModifier(List<string> synergyTags)
    {
        int mod = bonusCriticalChance;
        if (synergies == null || synergies.Count == 0)
        {
            return mod;
        }

        foreach (var syn in synergies)
        {
            if (synergyTags.Contains(syn.tag))
            {
                mod += syn.bonusCriticalChance;
            }
        }

        return mod;
    }

    public override int GetDamageModifier(List<string> synergyTags)
    {
        int mod = bonusDamage;
        if (synergies == null || synergies.Count == 0)
        {
            return mod;
        }

        foreach (var syn in synergies)
        {
            if (synergyTags.Contains(syn.tag))
            {
                mod += syn.bonusDamage;
            }
        }

        return mod;
    }

    public override int GetDefenceHitChanceModifier()
    {
        return defenceHitChanceMod;
    }

    public override int GetDefenceCriticalHitChanceModifier()
    {
        return defenceCriticalChanceMod;
    }

    public override int GetDefenceDamageModifier()
    {
        return defenceDamageMod;
    }

    public override void Activate(Unit activatingUnit, Unit targetUnit, Equipment equipment)
    {

    }

    public override void Activate(Unit activatingUnit, Cell targetCell, Equipment equipment)
    {

    }

    public override bool CanActivate(Unit activatingUnit)
    {
        return false;
    }
    
    public override string ToDescription()
    {
        string desc = "";

        // Synergies
        if (synergies != null && synergies.Count > 0)
        {
            foreach (EquipmentSynergy synergy in synergies)
            {
                desc += synergy.tag + " weapons: ";

                if (synergy.bonusDamage > 0)
                {
                    desc += "Damage: +" + synergy.bonusDamage.ToString() + " ";
                }
                else if (synergy.bonusDamage < 0)
                {
                    desc += "Damage: " + synergy.bonusDamage.ToString() + " ";
                }

                if (synergy.bonusHitChance > 0)
                {
                    desc += "Hit %: +" + synergy.bonusHitChance.ToString() + " ";
                }
                else if (synergy.bonusHitChance < 0)
                {
                    desc += "Hit %: " + synergy.bonusHitChance.ToString() + " ";
                }

                if (synergy.bonusCriticalChance > 0)
                {
                    desc += "Crit %: +" + synergy.bonusCriticalChance.ToString() + " ";
                }
                else if (synergy.bonusCriticalChance < 0)
                {
                    desc += "Crit %: " + synergy.bonusCriticalChance.ToString() + " ";
                }

                desc += "\n";
            }
        }

        // Any weapon bonuses
        if (bonusHitChance != 0 || bonusCriticalChance != 0 || bonusDamage != 0)
        {
            desc += "Any weapon: ";
        }

        if (bonusDamage > 0)
        {
            desc += "Damage: +" + bonusDamage.ToString() + " ";
        }
        else if (bonusDamage < 0)
        {
            desc += "Damage: " + bonusDamage.ToString() + " ";
        }

        if (bonusHitChance > 0)
        {
            desc += "Hit %: +" + bonusHitChance.ToString() + " ";
        }
        else if (bonusHitChance < 0)
        {
            desc += "Hit %: " + bonusHitChance.ToString() + " ";
        }

        if (bonusCriticalChance > 0)
        {
            desc += "Crit %: +" + bonusCriticalChance.ToString() + " ";
        }
        else if (bonusCriticalChance < 0)
        {
            desc += "Crit %: " + bonusCriticalChance.ToString() + " ";
        }
        
        if (bonusHitChance != 0 || bonusCriticalChance != 0 || bonusDamage != 0)
        {
            desc += "\n";
        }
        
        // Defences
        if (defenceDamageMod != 0 || defenceHitChanceMod != 0 || defenceCriticalChanceMod != 0)
        {
            desc += "Defensive: ";
        }

        if (defenceDamageMod > 0)
        {
            desc += "Damage: +" + defenceDamageMod.ToString() + " ";
        }
        else if (defenceDamageMod < 0)
        {
            desc += "Damage: " + defenceDamageMod.ToString() + " ";
        }

        if (defenceHitChanceMod > 0)
        {
            desc += "Hit %: +" + defenceHitChanceMod.ToString() + " ";
        }
        else if (defenceHitChanceMod < 0)
        {
            desc += "Hit %: " + defenceHitChanceMod.ToString() + " ";
        }

        if (defenceCriticalChanceMod > 0)
        {
            desc += "Crit %: +" + defenceCriticalChanceMod.ToString() + " ";
        }
        else if (defenceCriticalChanceMod < 0)
        {
            desc += "Crit %: " + defenceCriticalChanceMod.ToString() + " ";
        }
        
        if (defenceDamageMod != 0 || defenceHitChanceMod != 0 || defenceCriticalChanceMod != 0)
        {
            desc += "\n";
        }

        desc += base.ToDescription();
        return desc;
    }
}
