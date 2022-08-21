using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    private static MainMenu _Instance = null;
    public static MainMenu Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<MainMenu>();
            }
            return _Instance;
        }
    }

    public GameObject mainMenuScreen;
    public GameObject optionsScreen;
    public GameObject loadingScreen;

    public Button newGameButton;
    public Button continueGameButton;
    public Button optionsButton;
    public Button quitButton;

    protected void OnNewGameButtonPressed()
    {
        GameMaster.Instance.StartNewGame();
    }

    protected void OnContinueButtonPressed()
    {
        Debug.Log("Continuing saved game not implemented yet");
    }

    protected void OnOptionsButtonPressed()
    {
        mainMenuScreen.SetActive(false);
        optionsScreen.SetActive(true);
    }

    protected void OnQuitButtonPressed()
    {
        Application.Quit();
    }

    private void Awake()
    {
        if (_Instance == null)
        {
            _Instance = this;
        }
        else if (_Instance != this)
        {
            Debug.Log("Multiple singleton instances found: MainMenu");
            Destroy(gameObject);
            return;
        }
        
        Init();
    }

    private void Init()
    {
        DontDestroyOnLoad(gameObject);

        newGameButton.onClick.AddListener(OnNewGameButtonPressed);
        continueGameButton.onClick.AddListener(OnContinueButtonPressed);
        optionsButton.onClick.AddListener(OnOptionsButtonPressed);
        quitButton.onClick.AddListener(OnQuitButtonPressed);
        Hide();
    }

    public void ShowLoadingScreen()
    {
        mainMenuScreen.SetActive(false);
        optionsScreen.SetActive(false);
        loadingScreen.SetActive(true);
    }

    public void Show()
    {
        mainMenuScreen.SetActive(true);
        optionsScreen.SetActive(false);
        loadingScreen.SetActive(false);
    }

    public void Hide()
    {
        mainMenuScreen.SetActive(false);
        optionsScreen.SetActive(false);
        loadingScreen.SetActive(false);
    }
}
