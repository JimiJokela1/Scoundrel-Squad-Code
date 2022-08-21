using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CursorHighlighterUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorMaster.Instance.SetHighlightedCursor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorMaster.Instance.SetNormalCursor();
    }
}
