using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnitUIMini : MonoBehaviour
{
    public GameObject contents;
    public Image portrait;

    public Image background;
    public Sprite normalBackground;
    public Sprite unitHoveredBackground;
    public Sprite unitHighlightedBackground;

    public Sprite deadIcon;

    public Image movementSymbol;
    public Image action1Symbol;
    public Image action2Symbol;
    public TextMeshProUGUI movementPointsText;

    public event EventHandler UnitUIClicked;

    private bool dead = false;

    public void Init(PlayerUnitLink unit)
    {
        portrait.sprite = unit.unitData.icon;

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

    public void Show()
    {
        contents.SetActive(true);
    }

    public void Hide()
    {
        contents.SetActive(false);
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

    public void MarkAsDead()
    {
        portrait.sprite = deadIcon;
        dead = true;

        MarkAsAbsent();
        HideMovementAndActionSymbols();
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
    }

    public void MarkAsAbsent()
    {
        UnHighlight();
    }

    public void Highlight(PlayerUnit unit)
    {
        if (dead) return;

        background.sprite = unitHighlightedBackground;
    }

    public void UnHighlight()
    {
        background.sprite = normalBackground;
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

        if (background.sprite != unitHighlightedBackground)
        {
            background.sprite = unitHoveredBackground;

            CursorMaster.Instance.SetHighlightedCursor();
        }
    }

    public virtual void OnPointerExited()
    {
        if (dead) return;

        if (background.sprite != unitHighlightedBackground)
        {
            background.sprite = normalBackground;

            CursorMaster.Instance.SetNormalCursor();
        }
    }
}
