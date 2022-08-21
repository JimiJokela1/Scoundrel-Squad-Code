using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootPoint : InteractablePoint
{
    public Loot loot;

    public void InteractClick(object sender, EventArgs e)
    {
        InteractWithPoint();
    }

    public override void Init(Cell cell, InteractablePointContents contents, bool closeConfirmRequired = false, string confirmationMessage = "")
    {
        if (inited) return;
        base.Init(cell, contents, closeConfirmRequired, confirmationMessage);

        if (!(contents is Loot))
        {
            if (contents == null)
            {
                Debug.LogError("Null contents");
            }
            else
            {
                Debug.LogError("Trying to open loot point with non loot contents. Contents type: " + contents.GetType().ToString());
            }
            this.loot = new Loot();
        }
        else
        {
            this.loot = (Loot) contents;
        }

        if (loot.lootkeeper != null && loot.lootkeeper.modelPrefab != null)
        {
            GameObject model = Instantiate(loot.lootkeeper.modelPrefab, transform);
        }
        else
        {
            Debug.Log("No loot model found in loot. Using fallback.");
            GameObject model = Instantiate(LootSpawner.Instance.emptyLoot.modelPrefab, transform);
        }
        inited = true;
    }

    public override bool IsInteractable()
    {
        return gameObject.activeSelf;
    }

    public override void InteractWithPoint()
    {
        base.InteractWithPoint();

        GameUI.Instance.OpenLootWindow(this);
    }

    public override void ClosePoint()
    {
        base.ClosePoint();

        if (loot.IsEmpty())
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
        loot.items.Add(item);
    }

    public override bool RemoveItem(LootableItem item)
    {
        return loot.items.Remove(item);
    }

    public void RemoveCredits()
    {
        loot.credits = 0;
    }

    public List<LootPoint> GetLootPointsInRoom()
    {
        List<Room> rooms = GameMap.Instance.GetCellRooms(cell, false);
        return InteractablePointManager.Instance.GetLootPointsInRooms(rooms);
    }

    public static Loot GetLootInPoints(List<LootPoint> lootPoints)
    {
        List<LootableItem> items = new List<LootableItem>();
        int credits = 0;

        foreach (LootPoint lootPoint in lootPoints)
        {
            items.AddRange(lootPoint.loot.items);
            credits += lootPoint.loot.credits;
        }

        return new Loot(items, credits);
    }

    public override string GetPointName()
    {
        return loot.lootkeeper.placeName;
    }

    public override string GetDescription()
    {
        return loot.lootkeeper.description;
    }
}
