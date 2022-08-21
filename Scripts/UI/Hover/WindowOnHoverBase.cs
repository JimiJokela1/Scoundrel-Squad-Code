using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public abstract class WindowOnHoverBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool overrideGlobalDelays = false;
    public float _appearDelay = 0f;
    public float appearDelay
    {
        get
        {
            if (overrideGlobalDelays)
            {
                return _appearDelay;
            }
            else
            {
                return HoverWindow.HOVER_APPEAR_DELAY;
            }
        }
    }

    public float _fullTextDelay = 0f;
    public float fullTextDelay
    {
        get
        {
            if (overrideGlobalDelays)
            {
                return _fullTextDelay;
            }
            else
            {
                return HoverWindow.HOVER_FULL_TEXT_DELAY;
            }
        }
    }

    protected string hoverTitle = "";
    protected string hoverText = "";
    public bool hoverWindowFollowMouse = true;
    public float infoWindowDelay = 0f;
    protected string infoTextTitle = "";
    protected string infoText = "";

    float hoverTimer = 0f;
    bool hoverTimerActive = false;
    private bool activateHover;

    float infoWindowTimer = 0f;
    bool infoWindowTimerActive = false;
    private bool activateInfoWindow;

    float hoverFullTextTimer = 1f;
    bool hoverFullTextTimerActive = false;
    private bool activateFullText;

    protected virtual void SetText()
    {
        hoverText = "";
        hoverTitle = "";

        infoTextTitle = "";
        infoText = "";
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        PointerEntered();
    }

    public virtual void PointerEntered()
    {
        if (!WindowOnHover.hoversEnabled)
        {
            return;
        }

        SetText();

        if (appearDelay > 0f)
        {
            hoverTimerActive = true;
            hoverTimer = 0f;
        }
        else
        {
            activateHover = true;
        }

        if (infoWindowDelay > 0f)
        {
            infoWindowTimerActive = true;
            infoWindowTimer = 0f;
        }
        else
        {
            activateInfoWindow = true;
        }

        if (fullTextDelay > 0f)
        {
            hoverFullTextTimerActive = true;
            hoverFullTextTimer = 0f;
        }
        else
        {
            activateFullText = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExited();
    }

    protected virtual void PointerExited()
    {
        CloseHover();
        CloseInfoWindow();
    }

    private void Update()
    {
        if (!WindowOnHover.hoversEnabled)
        {
            return;
        }

        if (hoverTimerActive)
        {
            hoverTimer += Time.deltaTime;
            if (hoverTimer >= appearDelay)
            {
                activateHover = true;
                hoverTimer = 0f;
                hoverTimerActive = false;
            }
        }

        if (infoWindowTimerActive)
        {
            infoWindowTimer += Time.deltaTime;
            if (infoWindowTimer >= infoWindowDelay)
            {
                activateInfoWindow = true;
                infoWindowTimer = 0f;
                infoWindowTimerActive = false;
            }
        }

        if (activateInfoWindow)
        {
            OpenInfoWindow();
        }

        if (hoverFullTextTimerActive)
        {
            hoverFullTextTimer += Time.deltaTime;
            if (hoverFullTextTimer >= fullTextDelay)
            {
                activateFullText = true;
                hoverFullTextTimer = 0f;
                hoverFullTextTimerActive = false;
            }
        }

        if (activateHover)
        {
            OpenHover();
        }
    }

    public void UpdateHoverInfoWindowText(string newTitle, string newText)
    {
        if (newTitle != null && newText != null)
        {
            infoTextTitle = newTitle;
            infoText = newText;
            HoverInfoWindow.instance.UpdateHoverText(infoTextTitle, infoText, this);
        }
    }

    public void UpdateHoverText(string newText)
    {
        hoverText = newText;
		string textToDisplay = GetTextToDisplay();

        if (!string.IsNullOrEmpty(textToDisplay))
        {
            HoverWindow.instance.UpdateHoverText(textToDisplay, this);
        }
        else
        {
            CloseHover();
        }
    }

    private string GetTitle()
    {
        if (!string.IsNullOrEmpty(hoverTitle))
        {
            return "<b>" + hoverTitle + "</b>";
        }
        else
        {
            return "";
        }
    }

    private string GetFullText()
    {
        if (!string.IsNullOrEmpty(GetTitle()) || !string.IsNullOrEmpty(hoverText))
        {
            return GetTitle() + "\n" + hoverText;
        }
        else
        {
            return "";
        }
    }

    private string GetTextToDisplay()
    {
        string textToDisplay = "";
        if (activateFullText)
        {
            textToDisplay = GetFullText();
        }
        else
        {
            textToDisplay = GetTitle();
        }
        return textToDisplay;
    }

    public void OpenHover()
    {
        string textToDisplay = GetTextToDisplay();

        if (HoverWindow.instance.IsHoverActive(this) && !string.IsNullOrEmpty(textToDisplay))
        {
            HoverWindow.instance.UpdateHoverText(textToDisplay, this);
        }

        if (!string.IsNullOrEmpty(textToDisplay))
        {
            HoverWindow.instance.ShowWindow(textToDisplay, this, hoverWindowFollowMouse);
        }
    }

    public void OpenInfoWindow()
    {
        if (infoTextTitle != null && infoText != null
        && (infoTextTitle != "" || infoText != ""))
        {
            HoverInfoWindow.instance.ShowInfo(infoTextTitle, infoText, this);
        }
    }

    public void CloseInfoWindow()
    {
        activateInfoWindow = false;
        infoWindowTimerActive = false;
        infoWindowTimer = 0f;
        HoverInfoWindow.instance.HideInfo(this);
    }

    public void CloseHover()
    {
        activateHover = false;
        hoverTimerActive = false;
        hoverTimer = 0f;

        activateFullText = false;
        hoverFullTextTimerActive = false;
        hoverFullTextTimer = 0f;
        HoverWindow.instance.HideWindow(this);
    }

    protected virtual void OnDisable()
    {
        CloseHover();
        CloseInfoWindow();
    }

    public static string UppercaseFirst(string s)
    {
        // Check for empty string.
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }
        // Return char and concat substring.
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}
