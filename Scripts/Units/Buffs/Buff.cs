using System.Collections.Generic;
/// <summary>
/// Class representing an "upgrade" to a unit.
/// </summary>
public abstract class Buff
{
    public string buffSourceName = "";
    
    /// <summary>
    /// Determines how long the buff should last (expressed in turns). If set to negative number, buff will be permanent.
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Describes how the unit should be upgraded.
    /// </summary>
    public virtual void Apply(Unit unit)
    {
        unit.Buffs.Add(this);
    }
    
    /// <summary>
    /// Returns units stats to normal.
    /// </summary>
    public abstract void Undo(Unit unit);

    /// <summary>
    /// Returns deep copy of the Buff object.
    /// </summary>
    public abstract Buff Clone();

    public virtual int GetMovementModifier()
    {
        return 0;
    }

    public virtual int GetHitChanceModifier()
    {
        return 0;
    }

    public virtual int GetCriticalChanceModifier()
    {
        return 0;
    }

    public virtual int GetDamageModifier()
    {
        return 0;
    }

    public virtual int GetDefenceHitChanceModifier()
    {
        return 0;
    }

    public virtual int GetDefenceCriticalChanceModifier()
    {
        return 0;
    }

    public virtual int GetDefenceDamageModifier()
    {
        return 0;
    }
}