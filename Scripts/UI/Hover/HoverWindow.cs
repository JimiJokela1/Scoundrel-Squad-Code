using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HoverWindow : MonoBehaviour
{
	public static float HOVER_APPEAR_DELAY = 0f;
	public static float HOVER_FULL_TEXT_DELAY = 1f;

	public static HoverWindow instance = null;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(this);
		}
	}

	protected bool isShowing = false;
	public GameObject toEnable;

	public RectTransform window;
	public TextMeshProUGUI windowText;
	public Vector2 offsetFromMouse;
	public float fadeInTime;
	public float fadeOutTime;

	private int callerID;
	private bool followMouse = true;
	private bool fadeIn = false;
	private bool fadeOut = false;
	private float fadeTimer = 0f;
	private CanvasGroup canvasGroup;

	private void Start()
	{
		canvasGroup = window.GetComponent<CanvasGroup>();
		Hide();
	}

	public void Show()
	{
		isShowing = true;
		toEnable.SetActive(true);
		OnShow();
	}

	public void Hide()
	{
		isShowing = false;
		toEnable.SetActive(false);
		OnHide();
	}

	public bool IsShowing()
	{
		return isShowing;
	}

	public virtual void Close()
	{
		Hide();
		OnClose();
	}

	private void Update()
	{
		if (this.IsShowing())
		{
			if (fadeIn)
			{
				if (fadeInTime == 0f)
				{
					fadeIn = false;
					fadeTimer = 0f;
					canvasGroup.alpha = 1f;
				}
				else
				{
					fadeTimer += Time.deltaTime;
					canvasGroup.alpha = Mathf.Lerp(0f, 1f, fadeTimer / fadeInTime);

					if (fadeTimer >= fadeInTime)
					{
						fadeIn = false;
						fadeTimer = 0f;
					}
				}
			}

			if (fadeOut)
			{
				if (fadeOutTime == 0f)
				{
					fadeOut = false;
					fadeTimer = 0f;
					canvasGroup.alpha = 0f;
				}
				else
				{
					fadeTimer += Time.deltaTime;
					canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeOutTime);

					if (fadeTimer >= fadeOutTime)
					{
						fadeOut = false;
						fadeTimer = 0f;
						Hide();
					}
				}
			}

			if (followMouse)
			{
				Vector2 mousePos = Input.mousePosition;
				float scale = FindObjectOfType<Canvas>().scaleFactor;
				if (scale != 0)
				{
					mousePos = mousePos / scale;
				}

				// the last factor is because cursor image extends to the right of the actual cursor point
				Vector2 newPos = mousePos + offsetFromMouse + new Vector2(40f, 0f);
				float rightEdge = newPos.x + window.sizeDelta.x;
				// float screenWidth = Screen.width;
				float scaledScreenWidth = Screen.width / scale;
				if (rightEdge >= scaledScreenWidth)
				{
					newPos = mousePos - offsetFromMouse;
					SetLeftOfMouse();
				}
				else
				{
					SetRightOfMouse();
				}

				window.anchoredPosition = newPos;
			}
		}
	}

	public bool IsHoverActive(MonoBehaviour caller)
	{
		return (this.callerID == caller.GetInstanceID());
	}

	/// <summary>
	/// Only changes the text doesnt affect showing or hiding or fading
	/// </summary>
	/// <param name="text"></param>
	public void UpdateHoverText(string text, MonoBehaviour caller)
	{
		text = AddLineBreaks(text);
		if (IsHoverActive(caller) && text != windowText.text)
		{
			// windowText.SetText(text);
			windowText.text = text;
			windowText.ForceMeshUpdate(true);
			ScaleWindow();
		}
	}

	string AddLineBreaks(string text)
	{
		text = text.Replace(". ", ".<br>");
		if (text.EndsWith("<br>"))
		{
			text.Remove(text.Length - 3);
		}
		return text;
	}

	/// <summary>
	/// Open hover window and displays the text, saves caller id for hide IDing
	/// </summary>
	/// <param name="info"></param>
	/// <param name="caller"></param>
	public void ShowWindow(string info, MonoBehaviour caller, bool followMouse = true)
	{
		if (caller != null)
		{
			if (this.callerID == caller.GetInstanceID())
			{
				return;
			}
			Show();
			this.callerID = caller.GetInstanceID();
			this.followMouse = followMouse;
			info = AddLineBreaks(info);
			// windowText.SetText(info);
			windowText.text = info;
			windowText.ForceMeshUpdate(true);
			ScaleWindow();
			StartFadeIn();
		}
	}

	/// <summary>
	/// Hides the hover window if someone else hasn't activated it after caller
	/// </summary>
	/// <param name="caller"></param>
	public void HideWindow(MonoBehaviour caller)
	{
		if (caller != null)
		{
			if (this.callerID == caller.GetInstanceID())
			{
				this.callerID = -1;
				StartFadeOut();
				// Hide();
			}
		}
	}

	public void ScaleWindow()
	{
		// window.sizeDelta = new Vector2(100f, 55f);
		windowText.ForceMeshUpdate(true);
		window.sizeDelta = windowText.GetPreferredValues(windowText.text) + new Vector2(10f, 10f);
		windowText.ForceMeshUpdate(true);
		// for (int i = 0; i < 100; i++) {
		// 	if (windowText.isTextOverflowing) {
		// 		window.sizeDelta += new Vector2(10f, 0);

		// 	}
		// }
	}

	private void SetLeftOfMouse()
	{
		window.anchorMin = new Vector2(1f, window.anchorMin.y);
		window.anchorMax = new Vector2(1f, window.anchorMax.y);
		window.pivot = new Vector2(1f, window.pivot.y);
	}

	private void SetRightOfMouse()
	{
		window.anchorMin = new Vector2(0f, window.anchorMin.y);
		window.anchorMax = new Vector2(0f, window.anchorMax.y);
		window.pivot = new Vector2(0f, window.pivot.y);
	}

	private void StartFadeIn()
	{
		if (fadeOut && fadeTimer != 0f)
		{
			fadeTimer = Mathf.Lerp(0f, fadeInTime, canvasGroup.alpha);
		}
		fadeIn = true;
		fadeOut = false;
	}

	private void StartFadeOut()
	{
		if (fadeIn && fadeTimer != 0f)
		{
			fadeTimer = Mathf.Lerp(0f, fadeOutTime, canvasGroup.alpha);
		}
		fadeIn = false;
		fadeOut = true;
	}

	private void ResetValues()
	{
		windowText.SetText("");
	}

	protected void OnShow()
	{
		canvasGroup.alpha = 0f;
		ResetValues();
	}

	protected void OnHide()
	{
		canvasGroup.alpha = 0f;
		fadeTimer = 0f;
		fadeIn = false;
		fadeOut = false;
		ResetValues();
	}

	public bool CanClose()
	{
		return false;
	}

	protected void OnClose()
	{

	}

}
