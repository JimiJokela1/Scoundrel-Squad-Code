using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Active Stat Boost Macrochip", fileName = "ActiveStatBoostMacrochipData")]
public class ActiveStatBoostMacrochipData : ActiveMacrochipData
{
    [Header("Active mode")]
    [Tooltip("Duration of active effects when activated.")]
    public int durationInTurns = 1;

    [Tooltip("Modifier to movement point amount")]
    public int movementModifierActive;

    [Tooltip("Modifier to hit chance for any weapon")]
    public int hitChanceModifier;
    [Tooltip("Modifier to critical hit chance for any weapon")]
    public int criticalHitChanceModifier;
    [Tooltip("Modifier to damage for any weapon")]
    public int damageModifier;

    [Tooltip("Modifier to attack hit chance against unit equipped with this")]
    public int defenceHitChanceModifier;
    [Tooltip("Modifier to  attack critical hit chance against unit equipped with this")]
    public int defenceCriticalHitChanceModifier;
    [Tooltip("Modifier to  attack damage against unit equipped with this")]
    public int defenceDamageModifier;
    
    protected override void OnValidate()
    {
        base.OnValidate();

        if (durationInTurns <= 0) durationInTurns = 1;
    }

    public override int GetHitChanceModifier(List<string> synergyTags)
    {
        int mod = hitChanceModifier;

        return mod;
    }

    public override int GetCriticalChanceModifier(List<string> synergyTags)
    {
        int mod = criticalHitChanceModifier;

        return mod;
    }

    public override int GetDamageModifier(List<string> synergyTags)
    {
        int mod = damageModifier;

        return mod;
    }

    public override int GetDefenceHitChanceModifier()
    {
        return defenceHitChanceModifier;
    }

    public override int GetDefenceCriticalHitChanceModifier()
    {
        return defenceCriticalHitChanceModifier;
    }

    public override int GetDefenceDamageModifier()
    {
        return defenceDamageModifier;
    }

    public override void Activate(Unit activatingUnit, Equipment equipment)
    {
        base.Activate(activatingUnit, equipment);
        
        UberUnit uberUnit = (UberUnit) activatingUnit;
        if (uberUnit == null) return;

        ActiveStatBoostMacrochipBuff buff = new ActiveStatBoostMacrochipBuff(durationInTurns, this, equipment.GetItemName());
        buff.Apply(uberUnit);
    }

    public override bool CanActivateWithoutTarget()
    {
        return true;
    }
    
    public override string ToDescription()
    {
        string desc = "";

        desc += "Effect duration: " + durationInTurns.ToString() + " turns\n";

        if (movementModifierActive > 0)
        {
            desc += "\nMP: +" + movementModifierActive.ToString() + "\n";
        }
        else if (movementModifierActive < 0)
        {
            desc += "\nMP: " + movementModifierActive.ToString() + "\n";
        }
        else
        {
            desc += "\n";
        }

        // Any weapon bonuses
        if (hitChanceModifier != 0 || criticalHitChanceModifier != 0 || damageModifier != 0)
        {
            desc += "Any weapon: ";
        }

        if (damageModifier > 0)
        {
            desc += "Damage: +" + damageModifier.ToString() + " ";
        }
        else if (damageModifier < 0)
        {
            desc += "Damage: " + damageModifier.ToString() + " ";
        }

        if (hitChanceModifier > 0)
        {
            desc += "Hit %: +" + hitChanceModifier.ToString() + " ";
        }
        else if (hitChanceModifier < 0)
        {
            desc += "Hit %: " + hitChanceModifier.ToString() + " ";
        }

        if (criticalHitChanceModifier > 0)
        {
            desc += "Crit %: +" + criticalHitChanceModifier.ToString() + " ";
        }
        else if (criticalHitChanceModifier < 0)
        {
            desc += "Crit %: " + criticalHitChanceModifier.ToString() + " ";
        }
        
        if (hitChanceModifier != 0 || criticalHitChanceModifier != 0 || damageModifier != 0)
        {
            desc += "\n";
        }
        
        // Defences
        if (defenceDamageModifier != 0 || defenceHitChanceModifier != 0 || defenceCriticalHitChanceModifier != 0)
        {
            desc += "Defensive: ";
        }

        if (defenceDamageModifier > 0)
        {
            desc += "Damage: +" + defenceDamageModifier.ToString() + " ";
        }
        else if (defenceDamageModifier < 0)
        {
            desc += "Damage: " + defenceDamageModifier.ToString() + " ";
        }

        if (defenceHitChanceModifier > 0)
        {
            desc += "Hit %: +" + defenceHitChanceModifier.ToString() + " ";
        }
        else if (defenceHitChanceModifier < 0)
        {
            desc += "Hit %: " + defenceHitChanceModifier.ToString() + " ";
        }

        if (defenceCriticalHitChanceModifier > 0)
        {
            desc += "Crit %: +" + defenceCriticalHitChanceModifier.ToString() + " ";
        }
        else if (defenceCriticalHitChanceModifier < 0)
        {
            desc += "Crit %: " + defenceCriticalHitChanceModifier.ToString() + " ";
        }
        
        if (defenceDamageModifier != 0 || defenceHitChanceModifier != 0 || defenceCriticalHitChanceModifier != 0)
        {
            desc += "\n";
        }

        desc += "\n";

        desc += base.ToDescription();
        return desc;
    }
}
