using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance = null;

    private CellGrid cellGrid;

    // UI references
    public SquadUI squadUI;
    public Button activateActionModeButton;
    // public InventoryUI inventoryUI;
    public Button inventoryButton;
    public TextMeshProUGUI inventoryButtonText;
    public Button endTurnButton;
    public ShopUI shopUI;
    public LootUI lootUI;
    public RechargeUI rechargeUI;
    public Button elevatorButton;

    public TextMeshProUGUI creditsText;
    public Button menuButton;

    public TextMeshProUGUI floorNumberText;

    private InventoryUI activeInventoryUI;

    public delegate void InputEvent();

    public event InputEvent EscPressed;

    public static bool cancelKeyEnabled = true;
    public static bool globalKeyControlsEnabled = true;

    private bool initialized = false;

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

    public void UpdateCredits(int credits)
    {
        creditsText.SetText(credits.ToString());
    }

    public void UpdateEquipment(PlayerUnit playerUnit, Equipment equipment)
    {
        squadUI.UpdateEquipment(playerUnit, equipment);
    }

    private void Update()
    {
        if (globalKeyControlsEnabled)
        {
            if (cancelKeyEnabled)
            {
                if (Input.GetButtonDown("Cancel"))
                {
                    OnEscPressed();
                }
            }
        }
    }

    private void OnMenuButton()
    {
        // TODO: open menu
    }

    private void OnEndTurnButton()
    {
        if (!cellGrid.IsPlayerTurn())
        {
            return;
        }

        if (activeInventoryUI != null && activeInventoryUI.IsWindowOpen())
        {
            return;
        }

        cellGrid.EndTurn();
    }

    public virtual void OnEscPressed()
    {
        if (EscPressed != null)
        {
            EscPressed.Invoke();
        }
    }

    public void UpdateUnit(PlayerUnit unit)
    {
        if (!initialized) return;

        squadUI.UpdateUnit(unit);
    }

    public void Init(List<PlayerUnitLink> unitLinks)
    {
        if (initialized) return;
        initialized = true;

        cellGrid = FindObjectOfType<CellGrid>();
        if (cellGrid == null)
        {
            Debug.LogError("Cannot find CellGrid in scene");
        }

        if (activateActionModeButton != null)
        {
            activateActionModeButton.onClick.AddListener(OnActionModeActivated);
        }

        if (inventoryButton != null)
        {
            inventoryButton.onClick.AddListener(OnInventoryButton);
        }

        if (endTurnButton != null)
        {
            endTurnButton.onClick.AddListener(OnEndTurnButton);
        }

        if (elevatorButton != null)
        {
            elevatorButton.onClick.AddListener(OnElevatorButton);
            HideElevatorButton();
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuButton);
        }

        squadUI.Init(this, unitLinks);
        shopUI.Init(this);
        lootUI.Init(this);
    }

    public void HideElevatorButton()
    {
        elevatorButton.gameObject.SetActive(false);
    }

    public void ShowElevatorButton()
    {
        elevatorButton.gameObject.SetActive(true);
    }

    private void OnElevatorButton()
    {
        // Calculate how many of scoundrels are in elevator
        if (GameMaster.Instance.currentLevelElevator.unitsInEndElevator.Count < Squad.Instance.GetAllAliveUnits().Count)
        {
            ConfirmationWindow.Instance.ShowConfirmation("Abandon Scoundrels?", "If you activate the elevator now, some scoundrels will be left behind. Make sure everyone is in the elevator or they will surely perish. Are you sure you want to activate the elevator?", ElevatorConfirmed);
        }
        else
        {
            ConfirmationWindow.Instance.ShowConfirmation("Go up?", "Do you want to go up to the next level?", ElevatorConfirmed);
        }
    }

    private void ElevatorConfirmed()
    {
        GameMaster.Instance.ElevatorActivated();
    }

    public void UpdateCurrentLevel(int currentLevel)
    {
        floorNumberText.SetText(currentLevel.ToString("D2"));
    }

    public void OpenRechargeWindow(ChargePoint chargePoint)
    {
        if (chargePoint == null)
        {
            Debug.LogWarning("Trying to open Recharge Window for null ChargePoint");
            return;
        }

        if (activeInventoryUI != null)
        {
            activeInventoryUI.Close();
        }

        PlayerUnit selectedUnit = GetSelectedUnit();

        if (selectedUnit == null)
        {
            // All scoundrels are dead
            return;
        }

        rechargeUI.ShowRechargeUI(selectedUnit, chargePoint);
    }

    private void OnInventoryButton()
    {
        if (!cellGrid.IsPlayerTurn())
        {
            return;
        }

        if (activeInventoryUI != null && activeInventoryUI.IsWindowOpen())
        {
            activeInventoryUI.Close();
        }
        else
        {
            PlayerUnit selectedUnit = GetSelectedUnit();

            if (selectedUnit == null)
            {
                // All scoundrels are dead
                return;
            }

            LootPoint lootPoint = InteractablePointManager.Instance.GetLootPointOnCell(selectedUnit.Cell);
            if (lootPoint == null)
            {
                lootPoint = LootSpawner.Instance.SpawnEmptyLootPoint(selectedUnit.Cell);
            }

            OpenLootWindow(lootPoint);
        }
    }

    public void OpenLootWindow(LootPoint lootPoint)
    {
        if (lootPoint == null)
        {
            Debug.LogWarning("Trying to open Loot Window for null LootPoint");
            return;
        }

        if (activeInventoryUI != null)
        {
            activeInventoryUI.Close();
        }

        PlayerUnit selectedUnit = GetSelectedUnit();

        if (selectedUnit == null)
        {
            // All scoundrels are dead
            return;
        }

        List<PlayerUnit> roomUnits = Squad.Instance.GetUnitsInSameRoom(selectedUnit);

        List<LootPoint> lootPointsInRoom = lootPoint.GetLootPointsInRoom();
        Loot roomLoot = LootPoint.GetLootInPoints(lootPointsInRoom);

        activeInventoryUI = lootUI;
        lootUI.ShowUI(lootPoint, selectedUnit, roomLoot, lootPointsInRoom);

        squadUI.ShowInventoryUnits();
        squadUI.MarkUnitsInSameRoom(roomUnits, selectedUnit);

        int creditsGained = roomLoot.credits;
        foreach (LootPoint lootPointInRoom in lootPointsInRoom)
        {
            if (lootPointInRoom.loot.credits > 0)
            {
                lootPointInRoom.RemoveCredits();
            }
        }

        if (creditsGained > 0)
        {
            Squad.Instance.AddCredits(creditsGained);
        }

        inventoryButtonText.SetText("Close Inventory");
    }

    public void OpenShopWindow(ShopPoint shopPoint)
    {
        if (activeInventoryUI != null)
        {
            activeInventoryUI.Close();
        }

        PlayerUnit selectedUnit = GetSelectedUnit();
        if (selectedUnit == null)
        {
            // All scoundrels are dead
            return;
        }

        activeInventoryUI = shopUI;
        shopUI.ShowUI(shopPoint, selectedUnit);

        List<PlayerUnit> roomUnits = new List<PlayerUnit>();
        if (selectedUnit != null)
        {
            roomUnits = Squad.Instance.GetUnitsInSameRoom(selectedUnit);
        }

        squadUI.ShowInventoryUnits();
        squadUI.MarkUnitsInSameRoom(roomUnits, selectedUnit);

        inventoryButtonText.SetText("Exit Shop");
    }

    public void CloseInventory(InteractablePoint point, List<InteractablePoint> additionalPoints = null)
    {
        inventoryButtonText.SetText("Inventory");
        point.ClosePoint();
        if (additionalPoints != null)
        {
            foreach (InteractablePoint additionalPoint in additionalPoints)
            {
                if (additionalPoint != point)
                {
                    additionalPoint.ClosePoint();
                }
            }
        }
        activeInventoryUI = null;
        squadUI.ShowNormalUnits();
        squadUI.MarkUnitsInSameRoom(Squad.Instance.GetAllAliveUnits(), Squad.Instance.selectedUnit);
    }

    public virtual void OnUnitUIClicked(PlayerUnit unit)
    {
        cellGrid.OnUnitUIClicked(unit);
    }

    public void SelectUnit(PlayerUnit unit)
    {
        squadUI.HighlightUnit(unit);

        RefreshInventoryUI(unit);
    }

    public void OnActionModeActivated()
    {
        cellGrid.ActivateUnitActionMode();
    }

    public void OnUnitEquipmentSlotClicked(PlayerUnit unit, Equipment equipment, UnitUI unitUI, SlotUI slotUI)
    {
        // If inventory is open, drop item (or sell back to start shop)
        if (activeInventoryUI != null && activeInventoryUI.IsWindowOpen())
        {
            // If start shop is open, sell item back
            if (StartShop.Instance.IsOpen() && activeInventoryUI == shopUI)
            {
                if (unit.UnequipItem(equipment, false) == equipment)
                {
                    // Refund price
                    Squad.Instance.AddCredits(shopUI.GetSoldItemPrice(equipment));
                    // Add item back to shop
                    activeInventoryUI.AddItem(equipment);
                    // Selects unit if not yet selected
                    cellGrid.OnUnitUIClicked(unit);
                    // Update the inventory and the unit UI
                    RefreshInventoryUI(Squad.Instance.selectedUnit);
                    UpdateUnit(unit);
                }
            }
            else
            {
                // Drop item into new loot pile on unit's tile
                unit.DropItem(equipment);
                // Selects unit if not yet selected
                cellGrid.OnUnitUIClicked(unit);

                // unitUI.HighlightSlot(slotUI);
                // activeInventoryUI.openPoint.AddItem(equipment);

                // Update the inventory and the unit UI
                RefreshInventoryUI(Squad.Instance.selectedUnit);
                UpdateUnit(unit);
            }

        } // If inventory is not open, select equipment
        else if (cellGrid.OnUnitEquipmentSlotClicked(unit, equipment))
        {
            squadUI.HighlightUnit(unit);
            unitUI.HighlightSlot(slotUI);
        }
    }

    private void RefreshInventoryUI(PlayerUnit selectedUnit)
    {
        if (activeInventoryUI != null && activeInventoryUI.IsWindowOpen())
        {
            if (activeInventoryUI == lootUI)
            {
                InteractablePoint point = lootUI.openPoint;
                LootPoint lootPoint = (LootPoint) point;
                if (lootPoint != null) // && lootUI.lastSelectedUnit != selectedUnit)
                {
                    OpenLootWindow(lootPoint);
                }
            }
            else if (activeInventoryUI == shopUI)
            {
                activeInventoryUI.Refresh(selectedUnit);
            }
        }
    }

    private PlayerUnit GetSelectedUnit()
    {
        // If no unit is selected, try to select first alive unit
        PlayerUnit selectedUnit = Squad.Instance.selectedUnit;
        if (selectedUnit == null)
        {
            selectedUnit = Squad.Instance.GetFirstAliveUnit();
            // if (selectedUnit != null)
            // {
            //     OnUnitUIClicked(selectedUnit);
            // }
        }

        return selectedUnit;
    }

    public bool IsFullScreenWindowOpen()
    {
        if (activeInventoryUI != null)
        {
            return activeInventoryUI.IsWindowOpen();
        }

        return false;
    }
}
