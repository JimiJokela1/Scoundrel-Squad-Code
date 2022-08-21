using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextHoverer : MonoBehaviour {

	public static TextHoverer instance = null;

	private void Awake()
	{
		if (instance == null) {
			instance = this;
		} else {
			Destroy(this);
		}
	}

	List<TextHoverData> hoverData;
	
	public TextMeshProUGUI hoverText;
	public Vector3 offSetHoverWindow;
	public Image bg;

	Vector2 originalSize;

	public class TextHoverData {
		public TMP_Text textToCheck;
		public string textToFind;
		public string textToDisplay;
		public string callerID;

		public TextHoverData(TMP_Text textToCheck, string textToFind, string textToDisplay, string callerID) {
			this.textToCheck = textToCheck;
			this.textToFind = textToFind;
			this.textToDisplay = textToDisplay;
			this.callerID = callerID;
		}
	}

	void Start()
	{
		hoverData = new List<TextHoverData>();
		//bg = GetComponent<Image>();
		bg.enabled = false;
		originalSize = hoverText.rectTransform.sizeDelta;
	}

	public void RemoveHoverItems(string callerID) {
		List<TextHoverData> remove = new List<TextHoverData>();
		if (hoverData != null && hoverData.Count > 0) {
			foreach(TextHoverData item in hoverData) {
				if (item.callerID == callerID) {
					remove.Add(item);
				}
			}
		}

		if (remove.Count > 0) {
			foreach(TextHoverData removeItem in remove) {
				hoverData.Remove(removeItem);
			}
		}
	}

	public void AddHoverItem(TMP_Text textToCheck, string textToFind, string textToDisplay, string callerID) {
		hoverData.Add(new TextHoverData(textToCheck, textToFind, textToDisplay, callerID));
	}
	
	private void Update()
	{
		bool foundAWord = false;
		if (hoverData == null || hoverData.Count == 0) {
			return;
		}

		foreach(TextHoverData item in hoverData) {
			int nearestWord = TMP_TextUtilities.FindIntersectingWord(item.textToCheck, Input.mousePosition, null);
			if (nearestWord == -1) {
				continue;
			}
			TMP_WordInfo word = item.textToCheck.textInfo.wordInfo[nearestWord];
			if (word.characterCount <= 0) {
				continue;
			}
			// Debug.Log(word.GetWord());
			
			if (word.GetWord() == item.textToFind) {
				hoverText.SetText(item.textToDisplay);
				// if (hoverText.isTextOverflowing) {
				// 	hoverText.rectTransform.sizeDelta = new Vector2(hoverText.rectTransform.sizeDelta.x, hoverText.preferredHeight);
				// 	bg.rectTransform.sizeDelta = new Vector2(bg.rectTransform.sizeDelta.x, hoverText.preferredHeight);
				// }
				Vector3 pos = item.textToCheck.transform.TransformPoint((item.textToCheck.textInfo.characterInfo[word.firstCharacterIndex].bottomLeft
				+ item.textToCheck.textInfo.characterInfo[word.firstCharacterIndex + word.characterCount - 1].bottomRight) / 2);
				// pos += (item.textToCheck.textInfo.characterInfo[firstChar].bottomLeft + item.textToCheck.textInfo.characterInfo[firstChar + word.characterCount - 1].bottomRight) / 2;
				// pos.x = pos.x / Screen.width * CanvasFinder.GetMainCanvas().GetComponent<CanvasScaler>().referenceResolution.x;
				// pos.y = pos.y / Screen.height * CanvasFinder.GetMainCanvas().GetComponent<CanvasScaler>().referenceResolution.y;
				pos += offSetHoverWindow; // + new Vector3(0f, hoverText.preferredHeight / 2f, 0f);
				transform.position = pos;
				foundAWord = true;
				bg.enabled = true;
			}
		}
		
		if (!foundAWord) {
			bg.enabled = false;
			hoverText.SetText("");
		}
	}
}
