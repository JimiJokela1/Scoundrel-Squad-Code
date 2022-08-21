using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Progress/LootProgress", fileName = "LootProgress")]
public class LootProgress : ScriptableObject
{
    public int activateLevel;
    public List<LootProgressionTier> lootChestPool;

    public List<LootChest> ChooseLootChests()
    {
        List<LootChest> lootChests = new List<LootChest>();
        foreach (LootProgressionTier tier in lootChestPool)
        {
            List<TierElement> chosen = tier.ChooseElements();
            foreach (TierElement element in chosen)
            {
                if (element is LootChestWithWeight)
                {
                    lootChests.Add(((LootChestWithWeight) element).lootChest);
                }
            }
        }
        return lootChests;
    }

    private void Reset()
    {
        lootChestPool = new List<LootProgressionTier>()
        {
            new LootProgressionTier()
        };
    }

    protected virtual void OnValidate()
    {
        if (lootChestPool != null && lootChestPool.Count > 0)
        {
            foreach (var tier in lootChestPool)
            {
                tier.OnValidate();
            }
        }
    }
}

[System.Serializable]
public class LootProgressionTier : ChoiceTier
{
    [Tooltip("List of loot chests with chance weights")]
    public List<LootChestWithWeight> lootTable;

    public LootProgressionTier()
    {
        lootTable = new List<LootChestWithWeight>()
        {
            new LootChestWithWeight()
        };
    }

    protected override List<TierElement> GetElements()
    {
        return new List<TierElement>(lootTable);
    }
}

[System.Serializable]
public class LootChestWithWeight : TierElement
{
    public LootChest lootChest;

    public override bool IsElementNull()
    {
        return lootChest == null;
    }
}
