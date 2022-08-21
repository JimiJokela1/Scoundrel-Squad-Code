using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveStatBoostMacrochipBuff : Buff
{
    private int movementModifier;
    private int hitChanceModifier;
    private int criticalHitChanceModifier;
    private int damageModifier;
    private int defenceHitChanceModifier;
    private int defenceCriticalHitChanceModifier;
    private int defenceDamageModifier;

    private ActiveStatBoostMacrochipData chipData;

    public ActiveStatBoostMacrochipBuff(int duration, ActiveStatBoostMacrochipData chipData, string buffSourceName)
    {
        Duration = duration;
        movementModifier = chipData.movementModifierActive;
        hitChanceModifier = chipData.hitChanceModifier;
        criticalHitChanceModifier = chipData.criticalHitChanceModifier;
        damageModifier = chipData.damageModifier;
        defenceHitChanceModifier = chipData.defenceHitChanceModifier;
        defenceCriticalHitChanceModifier = chipData.defenceCriticalHitChanceModifier;
        defenceDamageModifier = chipData.defenceDamageModifier;

        this.chipData = chipData;

        this.buffSourceName = buffSourceName;
    }
    
    public override void Apply(Unit unit)
    {
        base.Apply(unit);

        if (unit is UberUnit)
        {
            UberUnit uberUnit = (UberUnit)unit;
            uberUnit.ModifyMovementPoints(movementModifier);
        }
    }

    public override void Undo(Unit unit)
    {
        if (unit is UberUnit)
        {
            UberUnit uberUnit = (UberUnit)unit;
            uberUnit.ModifyMovementPoints(-movementModifier);
        }
    }

    public override Buff Clone()
    {
        return new ActiveStatBoostMacrochipBuff(this.Duration, this.chipData, this.buffSourceName);
    }

    public override int GetMovementModifier()
    {
        return movementModifier;
    }

    public override int GetHitChanceModifier()
    {
        return hitChanceModifier;
    }

    public override int GetCriticalChanceModifier()
    {
        return criticalHitChanceModifier;
    }

    public override int GetDamageModifier()
    {
        return damageModifier;
    }

    public override int GetDefenceHitChanceModifier()
    {
        return defenceHitChanceModifier;
    }

    public override int GetDefenceCriticalChanceModifier()
    {
        return defenceCriticalHitChanceModifier;
    }

    public override int GetDefenceDamageModifier()
    {
        return defenceDamageModifier;
    }
}
