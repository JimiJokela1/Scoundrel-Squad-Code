using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TargetingUI : MonoBehaviour
{
    private static TargetingUI _Instance = null;
    public static TargetingUI Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<TargetingUI>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find singleton instance of TargetingUI");
                }
            }
            return _Instance;
        }
    }

    public RectTransform pivot;
    public GameObject contents;
    public TextMeshProUGUI chanceToHitText;
    public Canvas canvas;

    private bool active = false;

    public void Show(int chanceToHit)
    {
        active = true;
        contents.SetActive(true);
        chanceToHitText.SetText(chanceToHit.ToString() + "%");
        if (canvas.scaleFactor != 0f)
        {
            pivot.anchoredPosition = Input.mousePosition / canvas.scaleFactor;
        }
    }

    public void Hide()
    {
        active = false;
        chanceToHitText.SetText("");
        pivot.anchoredPosition = Vector2.zero;
        contents.SetActive(true);
    }

    private void Update()
    {
        if (active)
        {
            if (canvas.scaleFactor != 0f)
            {
                pivot.anchoredPosition = Input.mousePosition / canvas.scaleFactor;
            }
        }
    }

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else if (_Instance != this)
        {
            Debug.Log("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        Hide();
    }
}
