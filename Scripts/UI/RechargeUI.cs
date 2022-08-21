using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RechargeUI : MonoBehaviour
{
    private PlayerUnit unit;
    private ChargePoint openPoint;

    public void ShowRechargeUI(PlayerUnit selectedUnit, ChargePoint chargePoint)
    {
        openPoint = chargePoint;
        unit = selectedUnit;

        string chargeTypeName = System.Enum.GetName(typeof(ChargePoint.ChargeType), openPoint.chargeType);
        string title = "Recharge " + chargeTypeName;
        string message = "Do you want to recharge " + unit.GetUnitName() + "'s " + chargeTypeName + " for " + openPoint.chargePrice + "kp?";
        
        bool unitFull = false;
        switch (openPoint.chargeType)
        {
            case ChargePoint.ChargeType.Health:
                if (unit.HitPoints == unit.TotalHitPoints) unitFull = true;
                break;
            case ChargePoint.ChargeType.Armor:
                if (unit.currentArmor == unit.TotalArmor) unitFull = true;
                break;
            case ChargePoint.ChargeType.Energy:
                if (unit.currentEnergy == unit.TotalEnergy) unitFull = true;
                break;
            default:
                Debug.LogWarning("Unrecognized Charge Type");
                break;
        }

        if (openPoint.charges <= 0)
        {
            message = "Out of charges!";
            ConfirmationWindow.Instance.ShowConfirmation(title, message, RechargeCancelled, RechargeCancelled, false, true);
        }
        else if (unitFull)
        {
            message = "Unit does not need a recharge.";
            ConfirmationWindow.Instance.ShowConfirmation(title, message, RechargeCancelled, RechargeCancelled, false, true);
        }
        else
        {
            if (Squad.Instance.GetCurrentCredits() < chargePoint.chargePrice)
            {
                ConfirmationWindow.Instance.ShowConfirmation(title, message, RechargeCancelled, RechargeCancelled, false, true);
            }
            else
            {
                ConfirmationWindow.Instance.ShowConfirmation(title, message, RechargeConfirmed, RechargeCancelled);
            }
        }
    }

    private void RechargeConfirmed()
    {
        openPoint.ChargeUnit(unit);

        Clear();
    }

    private void RechargeCancelled()
    {
        Clear();
    }

    private void Clear()
    {
        openPoint = null;
        unit = null;
    }
}
