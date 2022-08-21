using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Equipment/Active Reveal Points Macrochip", fileName = "ActiveRevealPointsMacrochipData")]
public class ActiveRevealPointsMacrochipData : ActiveMacrochipData
{
    [Tooltip("Activating this chip will reveal all these targets on the map")]
    public List<FogOfWar.RevealableType> revealTargets;

    [Tooltip("Duration of reveal when activated. If duration is 0, it will last until end of level.")]
    public int durationInTurns = 1;

    protected override void OnValidate()
    {
        base.OnValidate();

        if (durationInTurns < 0) durationInTurns = 0;

        if (revealTargets == null || revealTargets.Count == 0)
        {
            Debug.Log("Remember to set reveal targets for this ActiveRevealPointsMacrochipData.");
            revealTargets = new List<FogOfWar.RevealableType>();
            revealTargets.Add(FogOfWar.RevealableType.Loot);
        }
    }

    public override void Activate(Unit activatingUnit, Equipment equipment)
    {
        UberUnit uberUnit = (UberUnit) activatingUnit;
        if (uberUnit == null) return;

        if (!(equipment is ActiveMacrochip))
        {
            Debug.LogError("Wrong type equipment and data matched.");
            return;
        }

        ActiveMacrochip chip = (ActiveMacrochip) equipment;
        chip.StartCooldown(cooldownInTurns, activatingUnit.PlayerNumber);

        uberUnit.currentEnergy -= energyCost;

        List<IRevealable> revealables = new List<IRevealable>();

        foreach (FogOfWar.RevealableType revealTarget in revealTargets)
        {
            revealables.AddRange(FogOfWar.GetRevealTargetsOfType(revealTarget));
        }

        FogOfWar.RevealEffect revealEffect = new FogOfWar.RevealEffect(this, revealables, durationInTurns, activatingUnit.PlayerNumber);
        GameMap.Instance.fogOfWar.ActivateRevealEffect(revealEffect);
    }

    public override bool CanActivateWithoutTarget()
    {
        return true;
    }

    public override string ToDescription()
    {
        string desc = "";

        foreach (FogOfWar.RevealableType revealTarget in revealTargets)
        {
            switch (revealTarget)
            {
                case FogOfWar.RevealableType.Loot:
                    desc += "Reveals all item containers on the map.\n";
                    break;
                case FogOfWar.RevealableType.Shop:
                    desc += "Reveals all shops on the map.\n";
                    break;
                case FogOfWar.RevealableType.HealthStation:
                    desc += "Reveals all health rechargers on the map.\n";
                    break;
                case FogOfWar.RevealableType.ArmorStation:
                    desc += "Reveals all armor rechargers on the map.\n";
                    break;
                case FogOfWar.RevealableType.EnergyStation:
                    desc += "Reveals all energy rechargers on the map.\n";
                    break;
                case FogOfWar.RevealableType.Elevator:
                    desc += "Reveals the exit elevator on the map.\n";
                    break;
                case FogOfWar.RevealableType.Enemies:
                    desc += "Reveals all enemies on the map.\n";
                    break;
                case FogOfWar.RevealableType.BareMap:
                    desc += "Reveals the layout of the map.\n";
                    break;
                case FogOfWar.RevealableType.FullMap:
                    desc += "Reveals the layout of the map and everything on the map.\n";
                    break;
            }
        }

        if (durationInTurns > 0)
        {
            desc += "Lasts for " + durationInTurns.ToString() + " turns\n";
        }
        else
        {
            desc += "Lasts until the end of the level.\n";
        }

        desc += base.ToDescription();
        return desc;
    }
}
