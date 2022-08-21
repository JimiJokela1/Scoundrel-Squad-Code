using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI ammoStackText;

    public event EventHandler<InventoryItemEventArgs> ItemClicked;
    public event EventHandler<InventoryItemEventArgs> ItemHovered;
    public event EventHandler<InventoryItemEventArgs> ItemUnhovered;

    [HideInInspector]
    public int price = 0;

    private LootableItem item;

    public LootableItem GetItem()
    {
        return item;
    }

    public void Init(LootableItem item)
    {
        this.item = item;
        icon.sprite = item.GetIcon();

        if (item != null && item is AmmoStack)
        {
            AmmoStack ammoStack = (AmmoStack) item;
            if (ammoStack.data != null && ammoStack.data is AmmoData)
            {
                AmmoData ammoData = (AmmoData) ammoStack.data;
                ammoStackText.SetText(ammoStack.ammoCount.ToString() + "/" + ammoData.stackSize.ToString());
            }
        }
    }

    public void Init(LootableItem item, int price)
    {
        Init(item);
        priceText.SetText(price.ToString());
        this.price = price;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnItemClicked();
    }

    public virtual void OnItemClicked()
    {
        if (ItemClicked != null)
        {
            ItemClicked.Invoke(this, new InventoryItemEventArgs(item, this));
        }
    }

    public virtual void OnItemHovered()
    {
        if (ItemHovered != null)
        {
            ItemHovered.Invoke(this, new InventoryItemEventArgs(item, this));
        }
    }

    public virtual void OnItemUnhovered()
    {
        if (ItemUnhovered != null)
        {
            ItemUnhovered.Invoke(this, new InventoryItemEventArgs(item, this));
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnItemHovered();
        
        CursorMaster.Instance.SetHighlightedCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnItemUnhovered();

        CursorMaster.Instance.SetNormalCursor();
    }
}

public class InventoryItemEventArgs : EventArgs
{
    public LootableItem item;
    public InventoryItemUI itemUI;

    public InventoryItemEventArgs(LootableItem item, InventoryItemUI itemUI)
    {
        this.item = item;
        this.itemUI = itemUI;
    }
}
