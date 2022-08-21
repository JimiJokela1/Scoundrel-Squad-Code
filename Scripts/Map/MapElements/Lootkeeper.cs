using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Lootkeeper : ScriptableObject
{
    [Tooltip("Unique identifier")]
    public string ID;
    [Tooltip("Name in game")]
    public string placeName;
    [Tooltip("Description in game")]
    public string description;
    [Tooltip("Icon used in inventory screen")]
    public Sprite icon;
    [Tooltip("Prefab for model spawned on map")]
    public GameObject modelPrefab;

    [Tooltip("Defines lists of items that can spawn here and their chances to spawn.")]
    public List<LootTier> lootTiers;

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(ID)) ID = "LooteyMcLootFace";
        if (string.IsNullOrEmpty(placeName)) placeName = "Place Name";

        if (lootTiers != null && lootTiers.Count > 0)
        {
            foreach (LootTier tier in lootTiers)
            {
                tier.OnValidate();
                
                if (tier.lootTable != null && tier.lootTable.Count > 0)
                {
                    tier.lootTable.ForEach(loot => loot.OnValidate());
                }

                if (tier.ammoLootTable != null && tier.ammoLootTable.Count > 0)
                {
                    tier.ammoLootTable.ForEach(loot => loot.OnValidate());
                }
            }
        }
    }

    private void Reset()
    {
        lootTiers = new List<LootTier>()
        {
            new LootTier()
        };
    }

    public virtual List<LootableItem> GetRandomItems()
    {
        List<LootableItem> chosenItems = new List<LootableItem>();
        foreach (LootTier tier in lootTiers)
        {
            List<TierElement> chosen = tier.ChooseElements();
            foreach (TierElement element in chosen)
            {
                if (element is AmmoWithWeigthAndStackSize)
                {
                    AmmoWithWeigthAndStackSize loot = (AmmoWithWeigthAndStackSize) element;
                    if (loot.item != null)
                    {
                        int stackSize = loot.GetRandomStackSize();
                        if (stackSize != 0)
                        {
                            chosenItems.Add(LootableItem.CreateItem(loot.item, stackSize));
                        }
                    }
                }
                else if (element is LootWithWeight)
                {
                    LootWithWeight loot = (LootWithWeight) element;
                    if (loot.item != null)
                    {
                        chosenItems.Add(LootableItem.CreateItem(loot.item));
                    }
                }
            }
        }

        return chosenItems;
    }

    [System.Serializable]
    public class LootTier : ChoiceTier
    {
        [Tooltip("List of items with chance weights")]
        public List<LootWithWeight> lootTable;

        [Tooltip("Extra list for ammo that allows determining stack sizes. Combined with Loot Table list when choosing loot.")]
        public List<AmmoWithWeigthAndStackSize> ammoLootTable;

        public LootTier()
        {
            lootTable = new List<LootWithWeight>()
            {
                new LootWithWeight()
            };
        }

        protected override List<TierElement> GetElements()
        {
            List<TierElement> elements = new List<TierElement>(lootTable);
            elements.AddRange(ammoLootTable);
            return elements;
        }
    }

    [System.Serializable]
    public class LootWithWeight : TierElement
    {
        public LootableItemData item;

        public override bool IsElementNull()
        {
            return item == null;
        }
    }

    [System.Serializable]
    public class AmmoWithWeigthAndStackSize : LootWithWeight
    {
        public int minStackSize = 0;
        public int maxStackSize = 0;

        public int GetRandomStackSize()
        {
            int stackSize = Random.Range(minStackSize, maxStackSize);
            return stackSize;
        }

        public override void OnValidate()
        {
            if (minStackSize < 0) minStackSize = 0;
            if (maxStackSize < 0) maxStackSize = 0;
            if (maxStackSize < minStackSize) maxStackSize = minStackSize;
            if (item != null && item is AmmoData)
            {
                if (maxStackSize > ((AmmoData) item).stackSize) maxStackSize = ((AmmoData) item).stackSize;
            }
            if (minStackSize > maxStackSize) minStackSize = maxStackSize;

            if (item != null && !(item is AmmoData))
            {
                Debug.LogWarning("The Item field in this class should only be used to hold ammo types. For other item types use the other loot table list.");
                item = null;
            }
        }

        // public LootableItem
    }
}
