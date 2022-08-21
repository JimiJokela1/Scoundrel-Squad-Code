using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitUI : MonoBehaviour
{
    public GameObject contents;
    public Image portrait;
    public TextMeshProUGUI characterNameText;
    public Image background;
    public Sprite normalBackground;
    public Sprite unitHoveredBackground;
    public Sprite unitHighlightedBackground;

    public Image awayFader;

    public Transform bigSlotsParent;
    public Transform smallSlotsParent;

    public GameObject bigSlotPrefab;
    public GameObject smallSlotPrefab;

    public Sprite deadIcon;

    public ProgressBar healthBar;
    public ProgressBar armorBar;
    public ProgressBar energyBar;

    public Image movementSymbol;
    public Image action1Symbol;
    public Image action2Symbol;
    public TextMeshProUGUI movementPointsText;

    public event EventHandler UnitUIClicked;
    public event EventHandler<SlotEventArgs> UnitEquipmentSlotClicked;

    private List<SlotUI> bigSlots;
    private List<SlotUI> smallSlots;

    private Dictionary<int, SlotUI> slotNumbers;

    private bool dead = false;
    private bool inventoryMode = false;

    public void Init(int maxBigSlotCount, int maxSmallSlotCount, int maxBarBlockCount, PlayerUnitLink unit)
    {
        slotNumbers = new Dictionary<int, SlotUI>();
        // origBackgroundColor = background.color;
        awayFader.enabled = false;

        if (healthBar != null) healthBar.Init(maxBarBlockCount);
        if (armorBar != null) armorBar.Init(maxBarBlockCount);
        if (energyBar != null) energyBar.Init(maxBarBlockCount);
        UpdateBars(unit.unit);

        bigSlots = new List<SlotUI>();
        for (int i = 0; i < maxBigSlotCount; i++)
        {
            GameObject slot = Instantiate(bigSlotPrefab, bigSlotsParent);
            SlotUI slotUI = slot.GetComponent<SlotUI>();

            if (slotUI == null)
            {
                Debug.LogError("Invalid Big Slot Prefab");
                break;
            }
            slotUI.SlotClicked += OnSlotClicked;
            slotUI.Init();
            bigSlots.Add(slotUI);
        }

        smallSlots = new List<SlotUI>();
        for (int i = 0; i < maxSmallSlotCount; i++)
        {
            GameObject slot = Instantiate(smallSlotPrefab, smallSlotsParent);

            SlotUI slotUI = slot.GetComponent<SlotUI>();

            if (slotUI == null)
            {
                Debug.LogError("Invalid Small Slot Prefab");
                break;
            }
            slotUI.SlotClicked += OnSlotClicked;
            slotUI.Init();
            smallSlots.Add(slotUI);
        }

        SetEquipment(unit.unit);

        portrait.sprite = unit.unitData.icon;
        characterNameText.SetText(unit.unitData.characterName);

        UnitUIHover unitHover = GetComponentInChildren<UnitUIHover>();
        if (unitHover != null)
        {
            unitHover.Init(unit.unit);
        }

        MovementSymbolHover movementSymbolHover = GetComponentInChildren<MovementSymbolHover>();
        if (movementSymbolHover != null)
        {
            movementSymbolHover.Init(unit.unit);
        }
    }

    public void SelectSlotByNumber(int equipmentNumber)
    {
        if (slotNumbers.ContainsKey(equipmentNumber))
        {
            slotNumbers[equipmentNumber].OnSlotClicked();
        }
    }

    public void UpdateMovementAndActionSymbols(PlayerUnit unit)
    {
        if (dead) return;

        // if (unit.MovementPoints > 0)
        // {
        //     movementSymbol.enabled = true;
        // }
        // else
        // {
        //     movementSymbol.enabled = false;
        // }

        movementPointsText.SetText(unit.MovementPoints.ToString());

        if (unit.ActionPoints == 2)
        {
            action1Symbol.enabled = true;
            action2Symbol.enabled = true;
        }
        else if (unit.ActionPoints == 1)
        {
            action1Symbol.enabled = true;
            action2Symbol.enabled = false;
        }
        else
        {
            action1Symbol.enabled = false;
            action2Symbol.enabled = false;
        }
    }

    public void UpdateBars(PlayerUnit unit)
    {
        if (dead) return;

        if (healthBar != null)
        {
            healthBar.SetEmptyBlockAmount(unit.TotalHitPoints);
            healthBar.SetFilledBlockAmount(unit.HitPoints);
        }
        if (armorBar != null)
        {
            armorBar.SetEmptyBlockAmount(unit.TotalArmor);
            armorBar.SetFilledBlockAmount(unit.currentArmor);
        }
        if (energyBar != null)
        {
            energyBar.SetEmptyBlockAmount(unit.TotalEnergy);
            energyBar.SetFilledBlockAmount(unit.currentEnergy);
        }
    }

    public void UpdateEquipment(Equipment equipment)
    {
        if (equipment.data.slotSize == EquipmentSlot.Big)
        {
            for (int i = 0; i < bigSlots.Count; i++)
            {
                if (bigSlots[i].isEquipped && bigSlots[i].equippedEquipment == equipment)
                {
                    bigSlots[i].UpdateEquipmentInfo();
                    break;
                }
            }
        }
        else if (equipment.data.slotSize == EquipmentSlot.Small)
        {
            for (int i = 0; i < smallSlots.Count; i++)
            {
                if (smallSlots[i].isEquipped && smallSlots[i].equippedEquipment == equipment)
                {
                    smallSlots[i].UpdateEquipmentInfo();
                    break;
                }
            }
        }
    }

    public void SetEquipment(PlayerUnit unit, bool selectedUnit = false)
    {
        if (dead) return;

        slotNumbers.Clear();

        ChangeBigSlotsCount(unit.core.data.bigSlots);
        ChangeSmallSlotsCount(unit.core.data.smallSlots);

        List<Equipment> orderedEquipment = new List<Equipment>(unit.equipped);
        orderedEquipment = orderedEquipment.OrderBy(e => e.data.activationType).ToList();

        List<Equipment> bigEquipment = orderedEquipment.FindAll(e => e.GetSlotSize() == EquipmentSlot.Big);
        List<Equipment> smallEquipment = orderedEquipment.FindAll(e => e.GetSlotSize() == EquipmentSlot.Small);

        int equipmentNumber = 1;
        foreach (Equipment equipment in bigEquipment)
        {
            if (!selectedUnit)
            {
                EquipItem(equipment, -1);
                continue;
            }

            if (equipment.data.activationType == EquipmentActivationType.Active)
            {
                EquipItem(equipment, equipmentNumber);
                equipmentNumber++;
            }
            else
            {
                EquipItem(equipment, -1);
            }
        }

        foreach (Equipment equipment in smallEquipment)
        {
            if (!selectedUnit)
            {
                EquipItem(equipment, -1);
                continue;
            }

            if (equipment.data.activationType == EquipmentActivationType.Active)
            {
                EquipItem(equipment, equipmentNumber);
                equipmentNumber++;
            }
            else
            {
                EquipItem(equipment, -1);
            }
        }

        if (unit.activeEquipment.data.slotSize == EquipmentSlot.Big)
        {
            foreach (SlotUI slot in bigSlots)
            {
                if (slot.equippedEquipment == unit.activeEquipment)
                {
                    HighlightSlot(slot);
                }
            }
        }
        else if (unit.activeEquipment.data.slotSize == EquipmentSlot.Small)
        {
            foreach (SlotUI slot in smallSlots)
            {
                if (slot.equippedEquipment == unit.activeEquipment)
                {
                    HighlightSlot(slot);
                }
            }
        }
    }

    protected void EquipItem(Equipment equipment, int equipmentNumber)
    {
        if (equipment.data.slotSize == EquipmentSlot.Big)
        {
            EquipBigSlotItem(equipment, equipmentNumber);
        }
        else if (equipment.data.slotSize == EquipmentSlot.Small)
        {
            EquipSmallSlotItem(equipment, equipmentNumber);
        }
    }

    public void HighlightSlot(SlotUI slotUI)
    {
        foreach (SlotUI slot in smallSlots)
        {
            slot.UnHighlight();
        }

        foreach (SlotUI slot in bigSlots)
        {
            slot.UnHighlight();
        }

        slotUI.Highlight();
    }

    public void UnhighlightAllSlots()
    {
        foreach (SlotUI slot in smallSlots)
        {
            slot.UnHighlight();
        }

        foreach (SlotUI slot in bigSlots)
        {
            slot.UnHighlight();
        }
    }

    public void MarkAsDead()
    {
        portrait.sprite = deadIcon;
        dead = true;

        MarkAsAbsent();
        HideBars();
        HideMovementAndActionSymbols();
    }

    public void HideBars()
    {
        if (healthBar != null)
        {
            healthBar.SetEmptyBlockAmount(0);
            healthBar.SetFilledBlockAmount(0);
        }
        if (armorBar != null)
        {
            armorBar.SetEmptyBlockAmount(0);
            armorBar.SetFilledBlockAmount(0);
        }
        if (energyBar != null)
        {
            energyBar.SetEmptyBlockAmount(0);
            energyBar.SetFilledBlockAmount(0);
        }
    }

    public void HideMovementAndActionSymbols()
    {
        movementSymbol.enabled = false;
        movementPointsText.SetText("");
        action1Symbol.enabled = false;
        action2Symbol.enabled = false;
    }

    public void MarkAsPresent(PlayerUnit unit)
    {
        if (dead) return;

        UnHighlight();
        SetEquipment(unit);

        awayFader.enabled = false;
    }

    public void MarkAsAbsent()
    {
        UnHighlight();

        UnhighlightAllSlots();
        ChangeBigSlotsCount(0);
        ChangeSmallSlotsCount(0);

        awayFader.enabled = true;

        // background.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
    }

    public void OnSlotClicked(object sender, SlotEventArgs e)
    {
        if (UnitEquipmentSlotClicked != null)
        {
            UnitEquipmentSlotClicked.Invoke(this, e);
        }
    }

    public void InventoryModeOn(PlayerUnit unit)
    {
        inventoryMode = true;
        contents.SetActive(true);

        if (!dead)
        {
            SetEquipment(unit);
        }
    }

    public void InventoryModeOff()
    {
        inventoryMode = false;
    }

    public void Highlight(PlayerUnit unit)
    {
        if (dead) return;

        contents.SetActive(true);

        SetEquipment(unit, true);

        if (inventoryMode)
        {
            background.sprite = unitHighlightedBackground;
        }
    }

    public void UnHighlight()
    {
        // if (dead) return;

        if (!inventoryMode)
        {
            UnhighlightAllSlots();
            ChangeBigSlotsCount(0);
            ChangeSmallSlotsCount(0);

            contents.SetActive(false);
        }

        background.sprite = normalBackground;
    }

    protected void EquipBigSlotItem(Equipment equipment, int equipmentNumber)
    {
        for (int i = 0; i < bigSlots.Count; i++)
        {
            if (!bigSlots[i].isEquipped)
            {
                if (equipmentNumber != -1)
                {
                    slotNumbers.Add(equipmentNumber, bigSlots[i]);
                }
                bigSlots[i].Equip(equipment, equipmentNumber);
                break;
            }
        }
    }

    protected void EquipSmallSlotItem(Equipment equipment, int equipmentNumber)
    {
        for (int i = 0; i < smallSlots.Count; i++)
        {
            if (!smallSlots[i].isEquipped)
            {
                if (equipmentNumber != -1)
                {
                    slotNumbers.Add(equipmentNumber, smallSlots[i]);
                }
                smallSlots[i].Equip(equipment, equipmentNumber);
                break;
            }
        }
    }

    protected void ChangeSmallSlotsCount(int count)
    {
        foreach (SlotUI slot in smallSlots)
        {
            slot.Clear();
            slot.Hide();
        }

        for (int i = 0; i < count && i < smallSlots.Count; i++)
        {
            smallSlots[i].Show();
        }
    }

    protected void ChangeBigSlotsCount(int count)
    {
        foreach (SlotUI slot in bigSlots)
        {
            slot.Clear();
            slot.Hide();
        }

        for (int i = 0; i < count && i < bigSlots.Count; i++)
        {
            bigSlots[i].Show();
        }
    }

    public virtual void OnUnitClicked()
    {
        if (UnitUIClicked != null)
        {
            UnitUIClicked.Invoke(this, new EventArgs());
        }
    }

    public virtual void OnPointerEntered()
    {
        if (dead) return;

        if (inventoryMode && background.sprite != unitHighlightedBackground)
        {
            background.sprite = unitHoveredBackground;

            CursorMaster.Instance.SetHighlightedCursor();
        }
    }

    public virtual void OnPointerExited()
    {
        if (dead) return;

        if (inventoryMode && background.sprite != unitHighlightedBackground)
        {
            background.sprite = normalBackground;

            CursorMaster.Instance.SetNormalCursor();
        }
    }
}
