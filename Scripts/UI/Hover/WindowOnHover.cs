using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Add this to UI gameobjects with a raycastable component (alpha 0 image if nothing else) to show hover
/// </summary>
public class WindowOnHover : WindowOnHoverBase
{
	public static bool hoversEnabled = true;

	public string _hoverTitle = "";
	public string _hoverText = "";
	public string _infoTextTitle = "";
	public string _infoText = "";

	protected override void SetText()
	{
		hoverTitle = _hoverTitle;
		hoverText = _hoverText;
		infoTextTitle = _infoTextTitle;
		infoText = _infoText;
	}
}
