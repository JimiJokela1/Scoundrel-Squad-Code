using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorMaster : MonoBehaviour
{
    private static CursorMaster _Instance = null;
    public static CursorMaster Instance
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<CursorMaster>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find Instance of CursorMaster in scene");
                }
                return _Instance;
            }
        }
    }

    public Texture2D normalCursorTexture;
    public Texture2D highlightedCursorTexture;

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

        Init();
    }
    
    private void Init()
    {
        DontDestroyOnLoad(this);
        SetNormalCursor();
    }

    public void SetNormalCursor()
    {
        Cursor.SetCursor(normalCursorTexture, Vector2.zero, CursorMode.Auto);
    }

    public void SetHighlightedCursor()
    {
        Cursor.SetCursor(highlightedCursorTexture, Vector2.zero, CursorMode.Auto);
    }
}
