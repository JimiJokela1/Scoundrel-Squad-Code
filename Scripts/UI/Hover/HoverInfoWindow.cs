using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HoverInfoWindow : MonoBehaviour
{
	public static HoverInfoWindow instance = null;

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

	public TextMeshProUGUI titleText;
	public TextMeshProUGUI infoText;
	private int callerID;

	private void Start()
	{
		Hide();
	}

	/// <summary>
	/// Open info panel and displays the text, saves caller id for hide IDing
	/// </summary>
	/// <param name="info"></param>
	/// <param name="caller"></param>
	public void ShowInfo(string title, string info, MonoBehaviour caller)
	{
		Show();
		if (caller != null)
		{
			this.callerID = caller.GetInstanceID();
		}
		// this.callerID = callerID;
		titleText.SetText(title);
		infoText.SetText(info);
	}

	/// <summary>
	/// Hides the info panel if someone else hasn't activated it after caller
	/// </summary>
	/// <param name="caller"></param>
	public void HideInfo(MonoBehaviour caller)
	{
		if (caller != null)
		{
			if (this.callerID == caller.GetInstanceID())
			{
				Hide();
			}
		}
	}

	/// <summary>
	/// Only changes the text doesnt affect showing or hiding or fading
	/// </summary>
	/// <param name="text"></param>
	public void UpdateHoverText(string title, string text, MonoBehaviour caller)
	{
		if (IsHoverActive(caller))
		{
			titleText.SetText(title);
			infoText.SetText(text);
		}
	}

	public bool IsHoverActive(MonoBehaviour caller)
	{
		return (this.callerID == caller.GetInstanceID());
	}

	public virtual void Show()
	{
		isShowing = true;
		if (toEnable != null)
		{
			toEnable.SetActive(true);
		}
		OnShow();
	}

	public virtual void Hide()
	{
		isShowing = false;
		if (toEnable != null)
		{
			toEnable.SetActive(false);
		}
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

	private void ResetValues()
	{
		infoText.SetText("");
		titleText.SetText("");
	}

	protected void OnShow()
	{
		ResetValues();
	}

	protected void OnHide()
	{
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
