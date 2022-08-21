using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopPoint : InteractablePoint
{
    public ShopItemList shopItemList;

    public List<ShopItem> shopItems
    {
        get
        {
            return shopItemList.shopItems;
        }
        set
        {
            shopItemList.shopItems = value;
        }
    }

    private void Awake()
    {
        // For testing
        // PointClicked += InteractClick;
    }

    public void InteractClick(object sender, EventArgs e)
    {
        InteractWithPoint();
    }

    public override void Init(Cell cell, InteractablePointContents contents, bool closeConfirmRequired = false, string confirmationMessage = "")
    {
        if (inited) return;
        base.Init(cell, contents, closeConfirmRequired, confirmationMessage);

        if (contents == null)
        {
            return;
        }

        if (!(contents is ShopItemList))
        {
            Debug.LogError("Trying to open shop point with non shopItem contents. Contents type: " + contents.GetType().ToString());
            this.shopItemList = new ShopItemList();
        }
        else
        {
            this.shopItemList = (ShopItemList) contents;
        }

        if (shopItemList.lootkeeper != null && shopItemList.lootkeeper.modelPrefab != null)
        {
            GameObject shopModel = Instantiate(shopItemList.lootkeeper.modelPrefab, transform);
        }
        else
        {
            Debug.LogWarning("No shop model set for shopkeeper. Using fallback.");
            GameObject shopModel = Instantiate(ShopSpawner.Instance.fallbackShopModel, transform);
        }
        inited = true;
    }

    public override void InteractWithPoint()
    {
        base.InteractWithPoint();

        GameUI.Instance.OpenShopWindow(this);
    }

    public override bool IsInteractable()
    {
        return gameObject.activeSelf;
    }

    public override void ClosePoint()
    {
        base.ClosePoint();

        if (shopItems == null || shopItems.Count == 0)
        {
            gameObject.SetActive(false);
        }
    }

    public override void AddItem(LootableItem item)
    {
        if (!gameObject.activeSelf)
        {
            // Reactive in case the point was deactivated when it emptied
            gameObject.SetActive(true);
        }
        shopItems.Add(new ShopItem(item, 0));
    }

    public void ReturnItem(ShopItem shopItem)
    {
        shopItems.Add(shopItem);
    }

    public override bool RemoveItem(LootableItem item)
    {
        ShopItem foundItem = null;
        foreach (ShopItem shopItem in shopItems)
        {
            if (shopItem.item == item)
            {
                foundItem = shopItem;
            }
        }

        if (foundItem != null)
        {
            return shopItems.Remove(foundItem);
        }

        return false;
    }

    public override string GetPointName()
    {
        return shopItemList.lootkeeper.placeName;
    }

    public override string GetDescription()
    {
        return shopItemList.lootkeeper.description;
    }
}
