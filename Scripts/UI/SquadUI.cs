using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadUI : MonoBehaviour
{

    public List<UnitUI> units;
    public List<UnitUIMini> unitsMini;

    public Transform selectedUnitParent;
    public Transform unselectedUnitsParent;
    public Transform inventoryUnitsParent;

    private Dictionary<PlayerUnit, UnitUI> unitUIs;
    private Dictionary<UnitUI, PlayerUnit> unitUIsReversed;

    private Dictionary<PlayerUnit, UnitUIMini> unitUIMinis;
    private Dictionary<UnitUIMini, PlayerUnit> unitUIMinisReversed;

    public static bool selectUnitControlsEnabled = false;
    public static bool equipmentNumberControlsEnabled = false;
    public static bool actionModeControlsEnabled = false;

    public void Init(GameUI gameUI, List<PlayerUnitLink> squadUnits)
    {
        if (units == null || units.Count == 0)
        {
            Debug.LogWarning("Add UnitUIs in inspector");
            return;
        }

        unitUIs = new Dictionary<PlayerUnit, UnitUI>();
        unitUIsReversed = new Dictionary<UnitUI, PlayerUnit>();

        unitUIMinis = new Dictionary<PlayerUnit, UnitUIMini>();
        unitUIMinisReversed = new Dictionary<UnitUIMini, PlayerUnit>();

        for (int i = 0; i < squadUnits.Count && i < units.Count && i < unitsMini.Count; i++)
        {
            units[i].Init(GameGlobals.MAX_BIG_SLOT_COUNT, GameGlobals.MAX_SMALL_SLOT_COUNT, GameGlobals.MAX_UNIT_BAR_BLOCK_COUNT, squadUnits[i]);

            unitsMini[i].Init(squadUnits[i]);

            unitUIs.Add(squadUnits[i].unit, units[i]);
            unitUIsReversed.Add(units[i], squadUnits[i].unit);

            unitUIMinis.Add(squadUnits[i].unit, unitsMini[i]);
            unitUIMinisReversed.Add(unitsMini[i], squadUnits[i].unit);

            units[i].UnitUIClicked += OnUnitUIClicked;
            units[i].UnitEquipmentSlotClicked += OnUnitEquipmentSlotClicked;

            unitsMini[i].UnitUIClicked += OnUnitUIClicked;

            squadUnits[i].unit.UnitDestroyed += OnUnitDied;
            squadUnits[i].unit.UnitAttacked += OnUnitAttacked;
            squadUnits[i].unit.UnitMoved += OnUnitAction;
            squadUnits[i].unit.UnitUsedAction += OnUnitAction;
        }

        gameUI.EscPressed += EscPressed;
    }

    private void Update()
    {
        if (GameUI.globalKeyControlsEnabled)
        {
            if (Input.GetMouseButtonDown(1))
            {
                ToggleActionMode();
            }

            if (selectUnitControlsEnabled)
            {
                SelectUnitsControls();
            }

            if (actionModeControlsEnabled)
            {
                ActionModeControls();
            }

            if (equipmentNumberControlsEnabled)
            {
                SelectEquipmentByNumberControls();
            }
        }
    }

    private void ToggleActionMode()
    {
        GameMap.Instance.cellGrid.ToggleUnitActionMode();
    }

    private void ActionModeControls()
    {
        PlayerUnit selectedUnit = Squad.Instance.selectedUnit;
        //TODO: get all possible targets
        // and swap between them by moving camera to them and the mouse over them maybe?

        if (Input.GetButtonDown("NextTarget"))
        {

        }
        else if (Input.GetButtonDown("PreviousTarget"))
        {

        }
    }

    private void SelectEquipmentByNumberControls()
    {
        PlayerUnit selectedUnit = Squad.Instance.selectedUnit;
        if (selectedUnit != null)
        {
            if (Input.GetButtonDown("Equipment1"))
            {
                unitUIs[selectedUnit].SelectSlotByNumber(1);
            }
            else if (Input.GetButtonDown("Equipment2"))
            {
                unitUIs[selectedUnit].SelectSlotByNumber(2);
            }
            else if (Input.GetButtonDown("Equipment3"))
            {
                unitUIs[selectedUnit].SelectSlotByNumber(3);
            }
            else if (Input.GetButtonDown("Equipment4"))
            {
                unitUIs[selectedUnit].SelectSlotByNumber(4);
            }
            else if (Input.GetButtonDown("Equipment5"))
            {
                unitUIs[selectedUnit].SelectSlotByNumber(5);
            }
            else if (Input.GetButtonDown("Equipment6"))
            {
                unitUIs[selectedUnit].SelectSlotByNumber(6);
            }
            else if (Input.GetButtonDown("Equipment7"))
            {
                unitUIs[selectedUnit].SelectSlotByNumber(7);
            }
            else if (Input.GetButtonDown("Equipment8"))
            {
                unitUIs[selectedUnit].SelectSlotByNumber(8);
            }
            else if (Input.GetButtonDown("Equipment9"))
            {
                unitUIs[selectedUnit].SelectSlotByNumber(9);
            }
        }
    }

    private bool TrySelectNext(List<PlayerUnit> alive, PlayerUnit unit, PlayerUnit origUnit)
    {
        if (origUnit == unit)
        {
            return false;
        }

        if (unit.HasActionsOrMovementLeft())
        {
            GameUI.Instance.OnUnitUIClicked(unit);
            return true;
        }
        else
        {
            for (int i = 0; i < alive.Count; i++)
            {
                if (alive[i] == unit)
                {
                    if (i < units.Count - 1)
                    {
                        return TrySelectNext(alive, alive[i + 1], origUnit);
                    }
                    else
                    {
                        return TrySelectNext(alive, alive[0], origUnit);
                    }
                }
            }
        }

        return false;
    }

    private bool TrySelectPrevious(List<PlayerUnit> alive, PlayerUnit unit, PlayerUnit origUnit)
    {
        if (origUnit == unit)
        {
            return false;
        }

        if (unit.HasActionsOrMovementLeft())
        {
            GameUI.Instance.OnUnitUIClicked(unit);
            return true;
        }
        else
        {
            for (int i = 0; i < alive.Count; i++)
            {
                if (alive[i] == unit)
                {
                    if (i > 0)
                    {
                        return TrySelectPrevious(alive, alive[i - 1], origUnit);
                    }
                    else
                    {
                        return TrySelectPrevious(alive, alive[alive.Count - 1], origUnit);
                    }
                }
            }
        }

        return false;
    }

    private void SelectUnitsControls()
    {
        List<PlayerUnit> alive = Squad.Instance.GetAllAliveUnits();
        PlayerUnit selectedUnit = Squad.Instance.selectedUnit;

        if (Input.GetButtonDown("NextScoundrel"))
        {
            if (selectedUnit == null)
            {
                if (alive != null && alive.Count > 0)
                {
                    if (alive[0].HasActionsOrMovementLeft())
                    {
                        GameUI.Instance.OnUnitUIClicked(alive[0]);
                    }
                    else if (alive.Count > 1)
                    {
                        TrySelectNext(alive, alive[1], alive[0]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < alive.Count; i++)
                {
                    if (alive[i] == selectedUnit)
                    {
                        if (i < alive.Count - 1)
                        {
                            if (alive[i + 1].HasActionsOrMovementLeft())
                            {
                                GameUI.Instance.OnUnitUIClicked(alive[i + 1]);
                            }
                            else
                            {
                                TrySelectNext(alive, alive[i + 1], selectedUnit);
                            }
                        }
                        else
                        {
                            if (alive[0].HasActionsOrMovementLeft())
                            {
                                GameUI.Instance.OnUnitUIClicked(alive[0]);
                            }
                            else
                            {
                                TrySelectNext(alive, alive[0], selectedUnit);
                            }
                        }
                    }
                }
            }
        }
        else if (Input.GetButtonDown("PreviousScoundrel"))
        {
            if (selectedUnit == null)
            {
                if (alive != null && alive.Count > 0)
                {
                    if (alive[0].HasActionsOrMovementLeft())
                    {
                        GameUI.Instance.OnUnitUIClicked(alive[0]);
                    }
                    else if (alive.Count > 1)
                    {
                        TrySelectPrevious(alive, alive[alive.Count - 1], alive[0]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < alive.Count; i++)
                {
                    if (alive[i] == selectedUnit)
                    {
                        if (i > 0)
                        {
                            if (alive[i - 1].HasActionsOrMovementLeft())
                            {
                                GameUI.Instance.OnUnitUIClicked(alive[i - 1]);
                            }
                            else
                            {
                                TrySelectPrevious(alive, alive[i - 1], selectedUnit);
                            }
                        }
                        else
                        {
                            if (alive[alive.Count - 1].HasActionsOrMovementLeft())
                            {
                                GameUI.Instance.OnUnitUIClicked(alive[alive.Count - 1]);
                            }
                            else
                            {
                                TrySelectPrevious(alive, alive[alive.Count - 1], selectedUnit);
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnUnitAction(object sender, EventArgs e)
    {
        if (sender is PlayerUnit)
        {
            PlayerUnit unit = (PlayerUnit) sender;
            UpdateUnit(unit);
        }
    }

    private void OnUnitAttacked(object sender, AttackEventArgs e)
    {
        if (sender is PlayerUnit)
        {
            PlayerUnit unit = (PlayerUnit) sender;
            unitUIs[unit].UpdateBars(unit);
        }
    }

    public void UpdateEquipment(PlayerUnit playerUnit, Equipment equipment)
    {
        unitUIs[playerUnit].UpdateEquipment(equipment);
    }

    public void UpdateUnit(PlayerUnit unit)
    {
        if (unitUIs != null && unitUIs.ContainsKey(unit))
        {
            unitUIs[unit].UpdateMovementAndActionSymbols(unit);
            unitUIs[unit].UpdateBars(unit);
            unitUIs[unit].SetEquipment(unit, (Squad.Instance.selectedUnit == unit));
        }

        if (unitUIMinis != null && unitUIMinis.ContainsKey(unit))
        {
            unitUIMinis[unit].UpdateMovementAndActionSymbols(unit);
        }
    }

    private void EscPressed()
    {
        // TODO: unselect unit or back to movement mode
    }

    public void OnUnitEquipmentSlotClicked(object sender, SlotEventArgs e)
    {
        if (!(sender is UnitUI))
        {
            Debug.LogError("Invalid event sender");
            return;
        }
        UnitUI unit = (UnitUI) sender;
        GameUI.Instance.OnUnitEquipmentSlotClicked(unitUIsReversed[unit], e.equipment, unit, e.slot);

        UpdateEquipment(unitUIsReversed[unit], e.equipment);
    }

    public void OnUnitDied(object sender, EventArgs e)
    {
        if (sender is PlayerUnit)
        {
            MarkUnitAsDead((PlayerUnit) sender);
        }
    }

    public void OnUnitUIClicked(object sender, EventArgs e)
    {
        if ((sender is UnitUI))
        {
            UnitUI unitUI = (UnitUI) sender;

            GameUI.Instance.OnUnitUIClicked(unitUIsReversed[unitUI]);
        }
        else if (sender is UnitUIMini)
        {
            UnitUIMini unitUIMini = (UnitUIMini) sender;

            GameUI.Instance.OnUnitUIClicked(unitUIMinisReversed[unitUIMini]);
        }
        else
        {
            Debug.LogError("wrong type sending event");
            return;
        }
    }

    private void HighlightUnit(UnitUI unit)
    {
        foreach (UnitUI unitUI in units)
        {
            unitUI.UnHighlight();
            // unitUI.transform.SetParent(unselectedUnitsParent, false);
        }

        unit.Highlight(unitUIsReversed[unit]);
        // unit.transform.SetParent(selectedUnitParent, false);
        // RectTransform unitTransform = unit.GetComponent<RectTransform>();
        // unitTransform.anchorMin = new Vector2(0, 1);
        // unitTransform.anchorMax = new Vector2(0, 1);
        // unitTransform.pivot = new Vector2(0, 1);
        // unitTransform.anchoredPosition = Vector2.zero;
    }

    private void HighlightUnit(UnitUIMini unit)
    {
        foreach (UnitUIMini unitUIMini in unitsMini)
        {
            unitUIMini.UnHighlight();
        }

        unit.Highlight(unitUIMinisReversed[unit]);
    }

    public void HighlightUnit(PlayerUnit unit)
    {
        HighlightUnit(unitUIs[unit]);
        HighlightUnit(unitUIMinis[unit]);
    }

    public UnitUI GetUnitUI(PlayerUnit unit)
    {
        return unitUIs[unit];
    }

    public void MarkUnitAsDead(PlayerUnit unit)
    {
        unitUIs[unit].MarkAsDead();
        unitUIMinis[unit].MarkAsDead();
    }

    public void MarkUnitsInSameRoom(List<PlayerUnit> roomUnits, PlayerUnit selectedUnit)
    {
        foreach (UnitUI unit in units)
        {
            unit.MarkAsAbsent();
        }

        foreach (UnitUIMini unitMini in unitsMini)
        {
            unitMini.MarkAsAbsent();
        }

        foreach (PlayerUnit unit in roomUnits)
        {
            unitUIs[unit].MarkAsPresent(unit);
            unitUIMinis[unit].MarkAsPresent(unit);
        }

        if (selectedUnit != null)
        {
            HighlightUnit(selectedUnit);
        }
    }

    public void ShowInventoryUnits()
    {
        foreach (UnitUIMini unitUIMini in unitsMini)
        {
            unitUIMini.Hide();
        }

        foreach (UnitUI unitUI in units)
        {
            unitUI.InventoryModeOn(unitUIsReversed[unitUI]);
            unitUI.transform.SetParent(inventoryUnitsParent, false);
        }
    }

    public void ShowNormalUnits()
    {
        foreach (UnitUIMini unitUIMini in unitsMini)
        {
            unitUIMini.Show();
        }

        foreach (UnitUI unitUI in units)
        {
            unitUI.InventoryModeOff();
            unitUI.transform.SetParent(selectedUnitParent, false);
            RectTransform unitTransform = unitUI.GetComponent<RectTransform>();
            unitTransform.anchorMin = new Vector2(0, 1);
            unitTransform.anchorMax = new Vector2(0, 1);
            unitTransform.pivot = new Vector2(0, 1);
            unitTransform.anchoredPosition = Vector2.zero;
        }
    }
}

public class UnitSlotEventArgs : EventArgs
{
    public PlayerUnit unit;
    public Equipment equipment;

    public UnitSlotEventArgs(PlayerUnit unit, Equipment equipment)
    {
        this.unit = unit;
        this.equipment = equipment;
    }
}
