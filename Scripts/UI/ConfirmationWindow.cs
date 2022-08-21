using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmationWindow : MonoBehaviour
{
    private static ConfirmationWindow _Instance = null;
    public static ConfirmationWindow Instance
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<ConfirmationWindow>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find singleton Instance of type ConfirmationWindow.");
                }
                return _Instance;
            }
        }
    }

    public GameObject contents;
    public Button yesButton;
    public Button noButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Slider splitSlider;
    public TextMeshProUGUI splitCountText;

    private UnityAction yesAction;
    private UnityAction noAction;

    public Vector2 sizeOffset;

    public int splitAmount = 0;
    private int splitMax = 0;

    private void Awake()
    {
        yesButton.onClick.AddListener(OnYesPressed);
        noButton.onClick.AddListener(OnNoPressed);
        splitSlider.onValueChanged.AddListener(SplitSliderValueChanged);
        Clear();
    }

    private void SplitSliderValueChanged(float value)
    {
        splitAmount = Mathf.RoundToInt(Mathf.Lerp(0f, splitMax, value));
        splitCountText.SetText(splitAmount.ToString());
    }

    public void ScaleWindow()
    {
        messageText.ForceMeshUpdate(true);
        Vector2 sizeDelta = GetComponent<RectTransform>().sizeDelta;
        sizeDelta = messageText.GetPreferredValues(messageText.text, sizeDelta.x, 0f) + sizeOffset;
        GetComponent<RectTransform>().sizeDelta = sizeDelta;
        messageText.ForceMeshUpdate(true);
    }

    public void ShowConfirmation(string title, string message, UnityAction yesAction, UnityAction noAction = null, bool yesAllowed = true, bool noAllowed = true)
    {
        GameUI.globalKeyControlsEnabled = false;
        contents.SetActive(true);

        this.yesAction = yesAction;
        this.noAction = noAction;

        if (!yesAllowed && !noAllowed)
        {
            Debug.LogWarning("At least one response must be allowed for confirmation window");
            noAllowed = true;
        }
        
        if (!yesAllowed)
        {
            yesButton.gameObject.SetActive(false);
        }

        if (!noAllowed)
        {
            noButton.gameObject.SetActive(false);
        }

        titleText.SetText(title);
        messageText.SetText(message);
        // ScaleWindow();
    }

    public void ShowAmmoStackSplitConfirmation(string title, string message, int maxCount, UnityAction yesAction, UnityAction noAction)
    {
        GameUI.globalKeyControlsEnabled = false;
        contents.SetActive(true);

        this.yesAction = yesAction;
        this.noAction = noAction;

        titleText.SetText(title);
        messageText.SetText(message);
        // ScaleWindow();

        splitSlider.gameObject.SetActive(true);
        splitMax = maxCount;
        splitSlider.value = 1f;
        SplitSliderValueChanged(1f);
    }

    public void OnYesPressed()
    {
        if (yesAction != null)
        {
            yesAction.Invoke();
        }
        Clear();

        GameUI.globalKeyControlsEnabled = true;
    }

    public void OnNoPressed()
    {
        if (noAction != null)
        {
            noAction.Invoke();
        }
        Clear();

        GameUI.globalKeyControlsEnabled = true;
    }

    public void Clear()
    {
        yesAction = null;
        noAction = null;
        titleText.SetText("");
        messageText.SetText("");

        splitCountText.SetText("");
        splitSlider.gameObject.SetActive(false);

        yesButton.gameObject.SetActive(true);
        noButton.gameObject.SetActive(true);

        contents.SetActive(false);
    }
}
