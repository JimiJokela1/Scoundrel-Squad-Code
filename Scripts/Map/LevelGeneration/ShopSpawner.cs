using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopSpawner : MonoBehaviour
{
    public static ShopSpawner Instance = null;

    public GameObject shopPointPrefab;
    public GameObject fallbackShopModel;

    public List<Shopkeeper> shopKeepers;

    public ShopPoint SpawnEmptyShopPoint(Cell cell)
    {
        ShopItemList shopItems = new ShopItemList();

        ShopPoint shopPoint = CreateShopPoint(cell, CameraOgre.Instance.GetDownDirection());
        shopPoint.Init(cell, shopItems);

        return shopPoint;
    }

    public void SpawnRandomShop(Cell cell, List<Shopkeeper> shopPool, CameraOgre.Direction dir)
    {
        Shopkeeper randomShopkeeper = shopPool[Random.Range(0, shopPool.Count)];
        if (randomShopkeeper == null) return;

        SpawnShop(cell, randomShopkeeper, dir);
    }

    public void SpawnShop(Cell cell, Shopkeeper shopkeeper, CameraOgre.Direction dir)
    {
        ShopItemList shopItems = GetRandomShopItems(shopkeeper);

        if (shopItems.IsEmpty())
        {
            Debug.LogWarning("Empty shopitemlist");
            return;
        }

        Vector2Int dirVector = CameraOgre.Instance.directionVectors[dir];
        
        ShopPoint shopPoint = CreateShopPoint(cell, dirVector);
        shopPoint.Init(cell, shopItems);

        InteractablePointManager.Instance.AddPoint(shopPoint, cell);

    }

    private Shopkeeper GetRandomShopKeeper()
    {
        if (shopKeepers == null || shopKeepers.Count == 0)
        {
            Debug.LogError("No shopkeepers defined in ShopSpawner");
            return null;
        }

        return shopKeepers[Random.Range(0, shopKeepers.Count)];
    }

    public ShopPoint CreateShopPoint(Cell cell, Vector2Int dir)
    {
        GameObject shopPointObject = Instantiate(shopPointPrefab, LevelGenerator.Instance.levelObjectsParent);
        shopPointObject.transform.position = cell.transform.position;

        Quaternion destination_rot = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.y));
        destination_rot = Quaternion.Euler(0f, destination_rot.eulerAngles.y, 0f);
        shopPointObject.transform.localRotation = destination_rot;

        ShopPoint shopPoint = shopPointObject.GetComponent<ShopPoint>();
        if (shopPoint == null)
        {
            Debug.LogError("Invalid Shop Point Prefab. It must have ShopPoint script attached to it.");
            Destroy(shopPointObject);
            return null;
        }

        return shopPoint;
    }

    public ShopItemList GetRandomShopItems(Shopkeeper shopkeeper)
    {
        ShopItemList shopItems = new ShopItemList();
        shopItems.lootkeeper = shopkeeper;

        List<LootableItem> items = shopkeeper.GetRandomItems();

        if (items != null && items.Count > 0)
        {
            foreach (LootableItem item in items)
            {
                int price = Mathf.RoundToInt(item.GetPrice() * (1f + Random.Range(shopkeeper.minPriceModifier, shopkeeper.maxPriceModifier)));
                shopItems.shopItems.Add(new ShopItem(item, price));
            }
        }

        return shopItems;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
            Destroy(this);
            return;
        }
    }
}
