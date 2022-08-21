using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LootableItemData : ScriptableObject
{
    [Tooltip("Unique identifier, not shown in game")]
    public string ID;
    [Tooltip("Name of item shown in game")]
    public string itemName;
    [Tooltip("Description shown in game")]
    [TextArea]
    public string description;
    [Tooltip("Icon shown in UI")]
    public Sprite icon;
    [Tooltip("Default price in shops")]
    public int price;

    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(ID)) ID = "ItemID";
        if (string.IsNullOrEmpty(itemName)) itemName = "Item Name";
        if (price < 0) price = 0;
    }

    public virtual string ToDescription()
    {
        return description;
    }
}
