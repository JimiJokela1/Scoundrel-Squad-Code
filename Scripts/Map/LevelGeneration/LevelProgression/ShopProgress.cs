using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progress/ShopProgress", fileName = "ShopProgress")]
public class ShopProgress : ScriptableObject
{
    public int activateLevel;
    public List<ShopProgressionTier> shopPool;

    public List<Shopkeeper> ChooseShops()
    {
        List<Shopkeeper> shops = new List<Shopkeeper>();
        foreach (ShopProgressionTier tier in shopPool)
        {
            List<TierElement> chosen = tier.ChooseElements();
            foreach (TierElement element in chosen)
            {
                if (element is ShopkeeperWithWeight)
                {
                    shops.Add(((ShopkeeperWithWeight) element).shopkeeper);
                }
            }
        }
        return shops;
    }

    private void Reset()
    {
        shopPool = new List<ShopProgressionTier>()
        {
            new ShopProgressionTier()
        };
    }

    protected virtual void OnValidate()
    {
        if (shopPool != null && shopPool.Count > 0)
        {
            foreach (var shopTier in shopPool)
            {
                shopTier.OnValidate();
            }
        }
    }
}

[System.Serializable]
public class ShopProgressionTier : ChoiceTier
{
    [Tooltip("List of loot chests with chance weights")]
    public List<ShopkeeperWithWeight> shopTable;

    public ShopProgressionTier()
    {
        shopTable = new List<ShopkeeperWithWeight>()
        {
            new ShopkeeperWithWeight()
        };
    }

    protected override List<TierElement> GetElements()
    {
        return new List<TierElement>(shopTable);
    }
}

[System.Serializable]
public class ShopkeeperWithWeight : TierElement
{
    public Shopkeeper shopkeeper;

    public override bool IsElementNull()
    {
        return shopkeeper == null;
    }
}
