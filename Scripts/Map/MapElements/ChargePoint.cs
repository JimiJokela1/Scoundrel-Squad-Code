using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargePoint : InteractablePoint
{
    private string pointName;
    private string description;

    [HideInInspector]
    public ChargeType chargeType;
    [HideInInspector]
    public int charges;
    [HideInInspector]
    public int chargePrice;

    public Sprite healthIcon;
    public Sprite armorIcon;
    public Sprite energyIcon;

    public override void Init(Cell cell, InteractablePointContents contents, bool closeConfirmRequired = false, string confirmationMessage = "")
    {
        if (inited) return;
        base.Init(cell, contents, closeConfirmRequired, confirmationMessage);

        if (!(contents is ChargePointContents))
        {
            if (contents == null)
            {
                Debug.LogError("Null contents");
            }
            else
            {
                Debug.LogError("Trying to open charge point with non charge point contents. Contents type: " + contents.GetType().ToString());
            }
        }
        else
        {
            ChargePointContents chargePointContents = (ChargePointContents) contents;
            this.pointName = chargePointContents.pointName;
            this.description = chargePointContents.pointDescription;
            this.chargeType = chargePointContents.chargeType;
            this.chargePrice = chargePointContents.chargePrice;
            this.charges = chargePointContents.chargeCount;
        }

        if (highlightIcon != null)
        {
            SpriteRenderer spriteRenderer = highlightIcon.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                switch (chargeType)
                {
                    case ChargeType.Health:
                        spriteRenderer.sprite = healthIcon;
                        break;
                    case ChargeType.Armor:
                        spriteRenderer.sprite = armorIcon;
                        break;
                    case ChargeType.Energy:
                        spriteRenderer.sprite = energyIcon;
                        break;
                    default:
                        break;
                }
            }
        }

        inited = true;
    }

    public override bool IsInteractable()
    {
        // Can interact even when charges are empty, UI will handle it
        return true;
    }

    public override void InteractWithPoint()
    {
        base.InteractWithPoint();

        GameUI.Instance.OpenRechargeWindow(this);
    }

    public void ChargeUnit(PlayerUnit unit)
    {
        charges--;
        Squad.Instance.DeductCredits(chargePrice);

        switch (chargeType)
        {
            case ChargePoint.ChargeType.Health:
                unit.Heal(unit.TotalHitPoints);
                break;
            case ChargePoint.ChargeType.Armor:
                unit.RechargeArmor(unit.TotalArmor);
                break;
            case ChargePoint.ChargeType.Energy:
                unit.RechargeEnergy(unit.TotalEnergy);
                break;
            default:
                Debug.LogWarning("Unrecognized Charge Type");
                break;
        }
    }

    public override string GetPointName()
    {
        return pointName;
    }

    public override string GetDescription()
    {
        return description;
    }

    public enum ChargeType
    {
        Health,
        Armor,
        Energy
    }
}

public class ChargePointContents : InteractablePointContents
{
    public string pointName;
    public string pointDescription;
    public int chargeCount;
    public int chargePrice;
    public ChargePoint.ChargeType chargeType;

    public ChargePointContents(ChargePointData data)
    {
        this.pointName = data.pointName;
        this.pointDescription = data.description;

        this.chargeCount = Random.Range(data.minCharges, data.maxCharges + 1);
        this.chargePrice = Random.Range(data.minPrice, data.maxPrice + 1);
        this.chargeType = data.chargeType;
    }
}
