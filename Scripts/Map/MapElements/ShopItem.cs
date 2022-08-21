using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class ShopItem
{
    public LootableItem item;
    public int price;

    public ShopItem()
    {
        this.item = Equipment.EmptyEquipment();
    }

    public ShopItem(LootableItem item, int price)
    {
        this.item = item;
        this.price = price;
    }
}

[System.Serializable]
public class ShopItemList : InteractablePointContents
{
    public List<ShopItem> shopItems;

    public ShopItemList(List<ShopItem> shopItems, Shopkeeper shopkeeper)
    {
        this.shopItems = shopItems;
        this.lootkeeper = shopkeeper;
    }

    public ShopItemList()
    {
        shopItems = new List<ShopItem>();
    }

    public bool IsEmpty()
    {
        if (shopItems == null || shopItems.Count == 0)
        {
            return true;
        }

        // if any item is non-null, this is not empty
        foreach (ShopItem item in shopItems)
        {
            if (item != null && item.item != null)
            {
                return false;
            }
        }

        return true;
    }
}
