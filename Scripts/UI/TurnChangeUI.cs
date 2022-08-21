using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnChangeUI : MonoBehaviour
{
    private static TurnChangeUI _Instance = null;
    public static TurnChangeUI Instance
    {
        get
        {
            if (_Instance != null)
            {
                return _Instance;
            }
            else
            {
                _Instance = FindObjectOfType<TurnChangeUI>();
                if (_Instance == null)
                {
                    Debug.LogError("Cannot find singleton Instance of type TurnChangeUI.");
                }
                return _Instance;
            }
        }
    }

    public TextMeshProUGUI turnChangeAnnounceText;
    public Color playerTextColor;
    public Color enemyTextColor;
    public float turnChangeAnnouncementVisibleDuration = 2f;
    public float fadeSpeed = 0.01f;

    private bool interruptFade = false;

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
        turnChangeAnnounceText.SetText("");

        GameMap.Instance.cellGrid.TurnEnded += OnTurnChange;
    }

    private void OnTurnChange(object sender, EventArgs e)
    {
        if ((sender as CellGrid).CurrentPlayer is HumanPlayer)
        {
            ShowPlayerTurnAnnouncement();
        }
        else
        {
            ShowEnemyTurnAnnouncement();
        }
    }

    public void ShowEnemyTurnAnnouncement()
    {
        interruptFade = true;

        turnChangeAnnounceText.color = enemyTextColor;
        turnChangeAnnounceText.SetText("CORPORATION TURN");
    }

    public void ShowPlayerTurnAnnouncement()
    {
        interruptFade = false;

        turnChangeAnnounceText.color = playerTextColor;
        turnChangeAnnounceText.SetText("SCOUNDREL TURN");

        StartCoroutine(HideTurnAnnouncementAfterDelay());
    }

    private IEnumerator HideTurnAnnouncementAfterDelay()
    {
        yield return new WaitForSeconds(turnChangeAnnouncementVisibleDuration);

        while (turnChangeAnnounceText.color.a > 0.01f)
        {
            if (interruptFade)
            {
                break;
            }

            Color color = turnChangeAnnounceText.color;
            color.a -= fadeSpeed;
            turnChangeAnnounceText.color = color;
            yield return null;
        }

        if (!interruptFade)
        {
            turnChangeAnnounceText.color = playerTextColor;
            turnChangeAnnounceText.SetText("");
        }

        interruptFade = false;
    }
}
