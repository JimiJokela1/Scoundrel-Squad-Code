using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Level Elements/Shopkeeper", fileName = "Shopkeeper")]
public class Shopkeeper : Lootkeeper
{
    [Tooltip("Name of shopkeeper")]
    public string shopKeeperName;

    [Tooltip("0 is no modifier, <0 is cheaper, >0 is more expensive, 1.0 is +100%. Modifier for each item is random between min and max.")]
    public float minPriceModifier = 0f;
    [Tooltip("0 is no modifier, <0 is cheaper, >0 is more expensive, 1.0 is +100%. Modifier for each item is random between min and max.")]
    public float maxPriceModifier = 0f;

    protected override void OnValidate()
    {
        base.OnValidate();
        if (minPriceModifier < -1f) minPriceModifier = -1f;
        if (maxPriceModifier < -1f) maxPriceModifier = -1f;
        if (maxPriceModifier < minPriceModifier) maxPriceModifier = minPriceModifier;
    }
}
