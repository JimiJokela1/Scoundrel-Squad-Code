using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loot : InteractablePointContents
{
    public List<LootableItem> items;
    public int credits;

    public Loot()
    {
        items = new List<LootableItem>();
        credits = 0;
    }

    public Loot(List<LootableItem> items, int credits)
    {
        this.items = items;
        this.credits = credits;
    }

    public bool IsEmpty()
    {
        return ((items == null || items.Count == 0) && credits == 0);
    }
}
