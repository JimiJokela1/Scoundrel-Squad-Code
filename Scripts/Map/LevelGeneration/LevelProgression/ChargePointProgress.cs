using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progress/ChargePointProgress", fileName = "ChargePointProgress")]
public class ChargePointProgress : ScriptableObject
{
    public int activateLevel;
    public List<ChargePointProgressionTier> chargePointPool;

    public List<ChargePointData> ChooseChargePoints()
    {
        List<ChargePointData> chargePoints = new List<ChargePointData>();
        foreach (ChargePointProgressionTier tier in chargePointPool)
        {
            List<TierElement> chosen = tier.ChooseElements();
            foreach (TierElement element in chosen)
            {
                if (element is ChargePointWithWeight)
                {
                    chargePoints.Add(((ChargePointWithWeight) element).chargePoint);
                }
            }
        }
        return chargePoints;
    }

    private void Reset()
    {
        chargePointPool = new List<ChargePointProgressionTier>()
        {
            new ChargePointProgressionTier()
        };
    }

    protected virtual void OnValidate()
    {
        if (chargePointPool != null && chargePointPool.Count > 0)
        {
            foreach (var tier in chargePointPool)
            {
                tier.OnValidate();
            }
        }
    }
}

[System.Serializable]
public class ChargePointProgressionTier : ChoiceTier
{
    [Tooltip("List of loot chests with chance weights")]
    public List<ChargePointWithWeight> chargePointTable;

    public ChargePointProgressionTier()
    {
        chargePointTable = new List<ChargePointWithWeight>()
        {
            new ChargePointWithWeight()
        };
    }

    protected override List<TierElement> GetElements()
    {
        return new List<TierElement>(chargePointTable);
    }
}

[System.Serializable]
public class ChargePointWithWeight : TierElement
{
    public ChargePointData chargePoint;

    public override bool IsElementNull()
    {
        return chargePoint == null;
    }
}
