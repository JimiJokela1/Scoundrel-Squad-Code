using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EquippedItemHover : WindowOnHoverBase
{
    protected override void SetText()
    {
        SlotUI itemUI = GetComponentInParent<SlotUI>();
        LootableItem item = null;
        if (itemUI != null) item = itemUI.equippedEquipment;

        if (item != null)
        {
            infoTextTitle = item.GetItemName();
            infoText = item.ToDescription();
        }
    }
}
