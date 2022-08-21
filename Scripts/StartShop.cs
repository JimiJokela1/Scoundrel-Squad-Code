using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartShop : MonoBehaviour
{
    private static StartShop _Instance = null;
    public static StartShop Instance 
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<StartShop>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find Startshop instance in scene");
                }
                return _Instance;
            }
        }
    }

    public Shopkeeper shopkeeper;
    private ShopPoint shop;
    private bool open = false;

    public void OpenStartShop(Cell cell)
    {
        if (shopkeeper == null)
        {
            Debug.LogError("Start shop shopkeeper is null");
            return;
        }

        open = true;

        shop = ShopSpawner.Instance.CreateShopPoint(cell, new Vector2Int(1,0));

        ShopItemList shopItems = ShopSpawner.Instance.GetRandomShopItems(shopkeeper);

        if (shopItems.IsEmpty())
        {
            Debug.LogError("Empty shopitemlist in startshop, no start shop created");
            Destroy(shop.gameObject);
            return;
        }

        shop.Init(cell, shopItems, true, "Are you sure you want to leave the shop? It might be a while before we see another.");

        shop.PointClosed += OnShopClosed;
        GameUI.Instance.OpenShopWindow(shop);
    }

    public bool IsOpen()
    {
        return open;
    }

    private void OnShopClosed(object sender, EventArgs e)
    {
        open = false;
        Destroy(shop.gameObject);
    }
}
