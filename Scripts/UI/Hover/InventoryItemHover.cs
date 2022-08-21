using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryItemHover : WindowOnHoverBase
{
    protected override void SetText()
    {
        InventoryItemUI itemUI = GetComponent<InventoryItemUI>();
        LootableItem item = null;
        if (itemUI != null) item = itemUI.GetItem();

        if (item != null)
        {
            infoTextTitle = item.GetItemName();
            if (itemUI.price > 0)
            {
                infoText = "Price: " + itemUI.price + "\n" + item.ToDescription();
            }
            else
            {
                infoText = item.ToDescription();
            }
            
            // hoverTitle = item.GetItemName();
            // hoverText = item.ToDescription();
        }
    }
}
