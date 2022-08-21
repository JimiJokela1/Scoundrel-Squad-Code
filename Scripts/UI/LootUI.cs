using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootUI : InventoryUI
{
    public TextMeshProUGUI creditsLootText;

    private PlayerUnit waitingForConfirmationUnit;
    private LootableItem waitingForConfirmationItem;

    private List<LootPoint> additionalOpenPoints;

    // [HideInInspector]
    // public PlayerUnit lastSelectedUnit;

    public override void Init(GameUI gameUI)
    {
        base.Init(gameUI);
    }

    public override void OnItemClicked(object sender, InventoryItemEventArgs e)
    {
        PlayerUnit selectedUnit = Squad.Instance.selectedUnit;

        if (selectedUnit == null)
        {
            GameLog.Instance.AddMessage("No scoundrel selected to equip equipment.");
            return;
        }

        if (e.item is AmmoStack)
        {
            AmmoStack ammoStack = (AmmoStack) e.item;
            waitingForConfirmationUnit = selectedUnit;
            waitingForConfirmationItem = e.item;
            ConfirmationWindow.Instance.ShowAmmoStackSplitConfirmation("Split Stack", e.item.GetConfirmationMessage(selectedUnit), ammoStack.ammoCount, () => AmmoStackSplitConfirmed(ConfirmationWindow.Instance.splitAmount), AmmoStackSplitCancelled);
        }
        else if (e.item.RequiresConfirmation())
        {
            waitingForConfirmationUnit = selectedUnit;
            waitingForConfirmationItem = e.item;
            ConfirmationWindow.Instance.ShowConfirmation("Warning!", e.item.GetConfirmationMessage(selectedUnit), EquipConfirmed, EquipCancelled);
        }
        else
        {
            EquipLootableItem(e.item, selectedUnit);
        }
    }

    private void EquipConfirmed()
    {
        if (waitingForConfirmationItem != null && waitingForConfirmationUnit != null)
        {
            EquipLootableItem(waitingForConfirmationItem, waitingForConfirmationUnit);
        }
        waitingForConfirmationItem = null;
        waitingForConfirmationUnit = null;
    }

    private void EquipCancelled()
    {
        waitingForConfirmationItem = null;
        waitingForConfirmationUnit = null;
    }

    private void AmmoStackSplitConfirmed(int splitAmount)
    {
        if (waitingForConfirmationItem == null || !(waitingForConfirmationItem is AmmoStack))
        {
            Debug.LogWarning("Trying to split non AmmoStack item");
            AmmoStackSplitCancelled();
            return;
        }

        if (waitingForConfirmationUnit == null)
        {
            Debug.LogWarning("Null unit waiting for confirmation");
            AmmoStackSplitCancelled();
            return;
        }

        AmmoStack ammoStack = (AmmoStack) waitingForConfirmationItem;
        if (splitAmount == ammoStack.ammoCount)
        {
            EquipLootableItem(ammoStack, waitingForConfirmationUnit);
        }
        else
        {
            List<AmmoStack> splitStacks = ammoStack.SplitStack(splitAmount);
            AmmoStack splitTarget = splitStacks.Find(a => a.ammoCount == splitAmount);
            AmmoStack otherStack = splitStacks.Find(a => a != splitTarget);
            if (splitTarget != null)
            {
                if (EquipLootableItem(splitTarget, waitingForConfirmationUnit))
                {
                    RemoveItem(waitingForConfirmationItem);

                    if (otherStack != null)
                    {
                        LootSpawner.Instance.SpawnAmmoStack(waitingForConfirmationUnit.Cell, otherStack);
                        AddItem(otherStack);
                    }
                }
            }
        }

        waitingForConfirmationItem = null;
        waitingForConfirmationUnit = null;
    }

    private void AmmoStackSplitCancelled()
    {
        waitingForConfirmationItem = null;
        waitingForConfirmationUnit = null;
    }

    private bool EquipLootableItem(LootableItem item, PlayerUnit selectedUnit)
    {
        if (!selectedUnit.HasRoomForItem(item))
        {
            GameLog.Instance.AddMessage("Selected Scoundrel cannot fit " + item.GetItemName() + ".");
            return false;
        }

        item.EquipItem(selectedUnit);
        RemoveItem(item);
        GameUI.Instance.UpdateUnit(selectedUnit);
        return true;
    }

    public override void RemoveItem(LootableItem item)
    {
        if (!openPoint.RemoveItem(item))
        {
            if (additionalOpenPoints != null && additionalOpenPoints.Count > 0)
            {
                foreach (InteractablePoint point in additionalOpenPoints)
                {
                    if (point.RemoveItem(item))
                    {
                        break;
                    }
                }
            }
        }

        if (lootableItemItemUIs.ContainsKey(item))
        {
            Destroy(lootableItemItemUIs[item].gameObject);
        }
    }

    public override void Clear()
    {
        base.Clear();

        creditsLootText.SetText("");

        additionalOpenPoints = null;
        waitingForConfirmationItem = null;
        waitingForConfirmationUnit = null;
    }

    protected override void CloseConfirmed()
    {
        if (openPoint != null)
        {
            if (additionalOpenPoints != null && additionalOpenPoints.Count > 0)
            {
                GameUI.Instance.CloseInventory(openPoint, new List<InteractablePoint>(additionalOpenPoints));
            }
            else
            {
                GameUI.Instance.CloseInventory(openPoint);
            }
        }

        Clear();
        contents.SetActive(false);
        // windowOpen = false;
    }

    public override void ShowUI(InteractablePoint point, PlayerUnit selectedUnit)
    {
        if (!(point is LootPoint))
        {
            Debug.LogError("Trying to show loot UI for non-LootPoint");
            return;
        }

        LootPoint lootPoint = (LootPoint) point;
        ShowUI(point, selectedUnit, lootPoint.loot);
    }

    public void ShowUI(InteractablePoint point, PlayerUnit selectedUnit, Loot loot, List<LootPoint> additionalPoints = null)
    {
        if (!(point is LootPoint))
        {
            Debug.LogError("Trying to show loot UI for non-LootPoint");
            return;
        }

        base.ShowUI(point, selectedUnit);

        // lastSelectedUnit = selectedUnit;

        if (additionalPoints != null)
        {
            additionalOpenPoints = new List<LootPoint>(additionalPoints);
        }
        else
        {
            additionalOpenPoints = null;
        }

        LootPoint lootPoint = (LootPoint) point;
        int credits = loot.credits;
        List<LootableItem> items = loot.items;

        if (credits > 0)
        {
            creditsLootText.SetText("+ " + credits.ToString() + GameGlobals.CREDITS_ABBREVIATION);
        }

        if (lootPoint.loot.lootkeeper != null)
        {
            if (!string.IsNullOrEmpty(lootPoint.loot.lootkeeper.placeName))
            {
                titleText.SetText(lootPoint.loot.lootkeeper.placeName);
            }

            if (lootPoint.loot.lootkeeper.icon != null)
            {
                iconImage.sprite = lootPoint.loot.lootkeeper.icon;
                iconImage.color = Color.white;
            }
        }

        foreach (LootableItem item in items)
        {
            AddItem(item);
        }
    }

    public override void AddItem(LootableItem item)
    {
        InventoryItemUI itemUI = CreateItem(item);

        if (itemUI != null)
        {
            itemUI.Init(item);
            itemUI.ItemClicked += OnItemClicked;

            itemUIs.Add(itemUI);
            lootableItemItemUIs.Add(item, itemUI);
        }
    }

    private InventoryItemUI CreateItem(LootableItem item)
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
