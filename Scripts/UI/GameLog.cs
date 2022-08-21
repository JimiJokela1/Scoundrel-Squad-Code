using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameLog : MonoBehaviour
{
    public static GameLog Instance = null;
    
    public TextMeshProUGUI logText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple Singleton Instance GameObjects in scene. Class: " + this.GetType().ToString() + " in GameObject: " + gameObject.name);
            Destroy(this);
            return;
        }
    }

    public void AddMessage(string message)
    {
        Debug.Log("Log: " + message);
        
        string text = logText.text;
        text += "\n" + message;
        logText.SetText(text);
    }
}
