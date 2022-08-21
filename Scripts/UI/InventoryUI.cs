using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class InventoryUI : MonoBehaviour
{
    public GameObject inventoryItemBigPrefab;
    public GameObject inventoryItemSmallPrefab;
    public Transform bigInventoryItemParent;
    public Transform smallInventoryItemParent;

    public GameObject contents;

    public TextMeshProUGUI titleText;
    public Image iconImage;

    [HideInInspector]
    public InteractablePoint openPoint;

    protected List<InventoryItemUI> itemUIs;
    protected Dictionary<LootableItem, InventoryItemUI> lootableItemItemUIs;

    // private bool windowOpen = false;

    public virtual bool IsWindowOpen()
    {
        return openPoint != null;
    }

    public virtual void Init(GameUI gameUI)
    {
        itemUIs = new List<InventoryItemUI>();
        lootableItemItemUIs = new Dictionary<LootableItem, InventoryItemUI>();
        openPoint = null;
        // windowOpen = false;

        gameUI.EscPressed += CloseButton;
    }

    public virtual void Refresh(PlayerUnit selectedUnit)
    {
        InteractablePoint point = openPoint;

        Clear();

        ShowUI(point, selectedUnit);
    }

    public virtual void ShowUI(InteractablePoint point, PlayerUnit selectedUnit)
    {
        contents.SetActive(true);
        // windowOpen = true;
        Clear();

        openPoint = point;
    }

    protected virtual void CloseButton()
    {
        Close();
    }

    public virtual void Close()
    {
        if (openPoint != null && openPoint.closeConfirmRequired)
        {
            ConfirmationWindow.Instance.ShowConfirmation("Warning!", openPoint.confirmationMessage, CloseConfirmed, null);
        }
        else
        {
            CloseConfirmed();
        }
    }

    protected virtual void CloseConfirmed()
    {
        if (openPoint != null)
        {
            GameUI.Instance.CloseInventory(openPoint);
        }

        Clear();
        contents.SetActive(false);
        // windowOpen = false;
    }

    public virtual void OnItemClicked(object sender, InventoryItemEventArgs e)
    {

    }

    public virtual void RemoveItem(LootableItem item)
    {

    }

    public virtual void AddItem(LootableItem item)
    {

    }

    public virtual void Clear()
    {
        foreach (InventoryItemUI item in itemUIs)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        itemUIs.Clear();
        lootableItemItemUIs.Clear();

        openPoint = null;

        titleText.SetText("");

        iconImage.sprite = null;
        iconImage.color = Color.clear;
    }
}
