using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject contents;
    public Image background;
    public Image equipmentIcon;
    public Image reloadOverlay;
    public Image disabledIcon;
    public TextMeshProUGUI ammoStackText;

    public Image activeSymbol;
    public TextMeshProUGUI slotNumberText;

    public Sprite normalBackground;
    public Sprite focusBackground;

    public event EventHandler<SlotEventArgs> SlotClicked;
    public event EventHandler<SlotEventArgs> SlotHovered;
    public event EventHandler<SlotEventArgs> SlotUnhovered;

    // private bool equipped = false;
    [HideInInspector]
    public Equipment equippedEquipment;
    private Color origBackgroundColor;

    public bool isEquipped
    {
        get
        {
            return equippedEquipment != null;
        }
    }

    public void Init()
    {
        // origBackgroundColor = background.color;
        Clear();
    }

    public void Equip(Equipment equipment, int equipmentNumber)
    {
        Sprite sprite = equipment.data.icon;
        equipmentIcon.sprite = sprite;
        equipmentIcon.color = Color.white;
        // equipped = true;
        equippedEquipment = equipment;
        if (equipmentNumber == -1)
        {
            slotNumberText.SetText("");
            activeSymbol.enabled = false;
        }
        else
        {
            slotNumberText.SetText(equipmentNumber.ToString());
            activeSymbol.enabled = true;
        }

        UpdateEquipmentInfo();
    }

    public void Clear()
    {
        equipmentIcon.sprite = null;
        equipmentIcon.color = Color.clear;
        // equipped = false;
        equippedEquipment = null;
        ammoStackText.SetText("");
        slotNumberText.SetText("");
        activeSymbol.enabled = false;
        background.sprite = normalBackground;
        HideReloadOverlay();
        disabledIcon.enabled = false;
    }

    public void Hide()
    {
        contents.SetActive(false);
    }

    public void Show()
    {
        contents.SetActive(true);
    }

    public void Highlight()
    {
        // background.color = new Color(0, 1, 0, 0.3f);
        background.sprite = focusBackground;
    }

    public void UnHighlight()
    {
        // background.color = origBackgroundColor;
        background.sprite = normalBackground;
    }

    public void UpdateEquipmentInfo()
    {
        if (equippedEquipment is AmmoStack)
        {
            AmmoStack ammoStack = (AmmoStack) equippedEquipment;
            if (ammoStack.data != null && ammoStack.data is AmmoData)
            {
                AmmoData ammoData = (AmmoData) ammoStack.data;
                ammoStackText.SetText(ammoStack.ammoCount.ToString() + "/" + ammoData.stackSize.ToString());
            }
        }
        else
        {
            ammoStackText.SetText("");
        }

        if (equippedEquipment is Weapon)
        {
            Weapon weapon = (Weapon) equippedEquipment;
            if (weapon.loadedAmmo == 0 && weapon.weaponData.rangeType != WeaponRangeType.Melee)
            {
                ShowReloadOverlay();
            }
            else
            {
                HideReloadOverlay();
            }

            // TODO: set weapon loaded ammo graphic
        }

        // figure out if equipment is disabled and set disabled icon
        if (equippedEquipment.IsEnabled())
        {
            disabledIcon.enabled = false;
        }
        else
        {
            disabledIcon.enabled = true;
        }
    }

    public void ShowReloadOverlay()
    {
        reloadOverlay.enabled = true;
    }

    public void HideReloadOverlay()
    {
        reloadOverlay.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnSlotClicked();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnSlotHovered();

        if (isEquipped)
        {
            CursorMaster.Instance.SetHighlightedCursor();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnSlotUnhovered();

        CursorMaster.Instance.SetNormalCursor();
    }

    public virtual void OnSlotClicked()
    {
        if (!isEquipped)
        {
            return;
        }

        if (SlotClicked != null)
        {
            SlotClicked.Invoke(this, new SlotEventArgs(equippedEquipment, this));
        }
    }

    public virtual void OnSlotHovered()
    {
        if (SlotHovered != null)
        {
            SlotHovered.Invoke(this, new SlotEventArgs(equippedEquipment, this));
        }
    }

    public virtual void OnSlotUnhovered()
    {
        if (SlotUnhovered != null)
        {
            SlotUnhovered.Invoke(this, new SlotEventArgs(equippedEquipment, this));
        }
    }
}

public class SlotEventArgs : EventArgs
{
    public Equipment equipment;
    public SlotUI slot;

    public SlotEventArgs(Equipment equipment, SlotUI slot)
    {
        this.equipment = equipment;
        this.slot = slot;
    }
}
