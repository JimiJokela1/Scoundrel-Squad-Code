using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitUIChildRaycastElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private UnitUI unitUI;
    private UnitUIMini unitUIMini;

    private void Awake()
    {
        unitUI = GetComponentInParent<UnitUI>();
        unitUIMini = GetComponentInParent<UnitUIMini>();

        if (unitUI == null && unitUIMini == null)
        {
            Debug.LogError("Cannot find UnitUI or UnitUIMini in parent. UnitUIBackground needs one of them in parent to do anything.");
            return;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (unitUI != null)
        {
            unitUI.OnUnitClicked();
        }
        else if (unitUIMini != null)
        {
            unitUIMini.OnUnitClicked();
        }
        else
        {
            Debug.LogError("Cannot find UnitUI or UnitUIMini in parent");
            return;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (unitUI != null)
        {
            unitUI.OnPointerEntered();
        }
        
        if (unitUIMini != null)
        {
            unitUIMini.OnPointerEntered();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (unitUI != null)
        {
            unitUI.OnPointerExited();
        }
        
        if (unitUIMini != null)
        {
            unitUIMini.OnPointerExited();
        }
    }
}
