using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopUI : InventoryUI
{
    private Dictionary<InventoryItemUI, ShopItem> itemShopItems;

    private PlayerUnit waitingForConfirmationUnit;
    private LootableItem waitingForConfirmationItem;
    private int waitingForConfirmationPrice;

    private List<ShopItem> soldItems = new List<ShopItem>();

    public override void Init(GameUI gameUI)
    {
        base.Init(gameUI);
        itemShopItems = new Dictionary<InventoryItemUI, ShopItem>();
    }

    public override void OnItemClicked(object sender, InventoryItemEventArgs e)
    {
        PlayerUnit selectedUnit = Squad.Instance.selectedUnit;

        if (selectedUnit == null)
        {
            GameLog.Instance.AddMessage("No scoundrel selected to buy equipment.");
            return;
        }

        if (e.item.RequiresConfirmation())
        {
            waitingForConfirmationItem = e.item;
            waitingForConfirmationUnit = selectedUnit;
            waitingForConfirmationPrice = itemShopItems[e.itemUI].price;
            ConfirmationWindow.Instance.ShowConfirmation("Warning!", e.item.GetConfirmationMessage(selectedUnit), BuyConfirmed, BuyCancelled);
        }
        else
        {
            BuyItem(e.item, itemShopItems[e.itemUI].price, selectedUnit);
        }
    }

    private void BuyConfirmed()
    {
        if (waitingForConfirmationItem != null && waitingForConfirmationUnit != null)
        {
            BuyItem(waitingForConfirmationItem, waitingForConfirmationPrice, waitingForConfirmationUnit);
        }
        waitingForConfirmationItem = null;
        waitingForConfirmationUnit = null;
        waitingForConfirmationPrice = 0;
    }

    private void BuyCancelled()
    {
        waitingForConfirmationItem = null;
        waitingForConfirmationUnit = null;
        waitingForConfirmationPrice = 0;
    }

    private void BuyItem(LootableItem item, int price, PlayerUnit selectedUnit)
    {
        if (!selectedUnit.HasRoomForItem(item))
        {
            GameLog.Instance.AddMessage("Selected Scoundrel cannot fit " + item.GetItemName() + ".");
            return;
        }

        if (Squad.Instance.currentCredits < price)
        {
            GameLog.Instance.AddMessage("The Squad doesn't have enough " + GameGlobals.CREDITS_ABBREVIATION + " for " + item.GetItemName() + ".");
            return;
        }

        // Pay price
        Squad.Instance.DeductCredits(price);
        // Equip item on selected unit
        item.EquipItem(selectedUnit);

        // Save the shopitem in the soldItems list in case it is returned to the shop
        ShopPoint shopPoint = (ShopPoint) openPoint;
        ShopItem shopItem = shopPoint.shopItems.Find(s => s.item == item);
        if (shopItem != null)
        {
            soldItems.Add(shopItem);
        }
        else
        {
            Debug.LogWarning("Could not find ShopItem matching the LootableItem being bought. Could not store item for possible refund later.");
        }

        // Remove item from shop and ui
        RemoveItem(item);
        // Update unit UI to show newly equipped item
        GameUI.Instance.squadUI.GetUnitUI(selectedUnit).SetEquipment(selectedUnit, true);
    }

    public override void RemoveItem(LootableItem item)
    {
        GameObject itemUIgameObject = lootableItemItemUIs[item].gameObject;
        openPoint.RemoveItem(item);
        itemShopItems.Remove(lootableItemItemUIs[item]);
        itemUIs.Remove(lootableItemItemUIs[item]);
        lootableItemItemUIs.Remove(item);
        Destroy(itemUIgameObject);
    }

    public override void Clear()
    {
        base.Clear();
        itemShopItems.Clear();
    }

    public override void ShowUI(InteractablePoint point, PlayerUnit selectedUnit)
    {
        if (!(point is ShopPoint))
        {
            Debug.LogError("Trying to show shop UI for non-ShopPoint");
            return;
        }

        ShopPoint shopPoint = (ShopPoint) point;

        base.ShowUI(point, selectedUnit);

        if (shopPoint.shopItemList.lootkeeper != null)
        {
            if (!string.IsNullOrEmpty(shopPoint.shopItemList.lootkeeper.placeName))
            {
                titleText.SetText(shopPoint.shopItemList.lootkeeper.placeName);
            }

            if (shopPoint.shopItemList.lootkeeper.icon != null)
            {
                iconImage.sprite = shopPoint.shopItemList.lootkeeper.icon;
                iconImage.color = Color.white;
            }
        }

        foreach (ShopItem shopItem in shopPoint.shopItems)
        {
            AddShopItem(shopItem);
        }
    }

    public void AddShopItem(ShopItem shopItem)
    {
        InventoryItemUI itemUI = CreateItem(shopItem.item, shopItem.price);
        if (itemUI != null)
        {
            itemShopItems.Add(itemUI, shopItem);

            itemUI.Init(shopItem.item, shopItem.price);
            itemUI.ItemClicked += OnItemClicked;

            itemUIs.Add(itemUI);
            lootableItemItemUIs.Add(shopItem.item, itemUI);
        }
    }

    public override void AddItem(LootableItem item)
    {
        ShopPoint shopPoint = (ShopPoint) openPoint;
        // I don't know when it would already be in the shoppoint's list so this check is propably unnecessary
        ShopItem shopItem = shopPoint.shopItems.Find(s => s.item == item);
        if (shopItem != null)
        {
            AddShopItem(shopItem);
        }
        else
        {
            shopItem = soldItems.Find(s => s.item == item);
            if (shopItem != null)
            {
                shopPoint.ReturnItem(shopItem);
                AddShopItem(shopItem);
            }
            else
            {
                Debug.LogWarning("Adding an item to shop failed. Could not find matching existing ShopItem for the LootableItem. This method implementation is meant for returning items to shop.");
            }
        }
    }

    public int GetSoldItemPrice(LootableItem item)
    {
        ShopItem shopItem = soldItems.Find(s => s.item == item);
        if (shopItem != null)
        {
            return shopItem.price;
        }
        else
        {
            return 0;
        }
    }

    private InventoryItemUI CreateItem(LootableItem item, int price)
    {
        GameObject inventoryItem = null;
        if (item.GetSlotSize() == EquipmentSlot.Big)
        {
            inventoryItem = Instantiate(inventoryItemBigPrefab, bigInventoryItemParent);
        }
        else if (item.GetSlotSize() == EquipmentSlot.Small)
        {
            inventoryItem = Instantiate(inventoryItemSmallPrefab, smallInventoryItemParent);
        }

        InventoryItemUI itemUI = inventoryItem.GetComponent<InventoryItemUI>();
        if (itemUI == null)
        {
            Debug.LogError("Invalid Inventory Item UI prefab");
            Destroy(inventoryItem);
            return null;
        }

        return itemUI;
    }
}
