using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController instance;
    [SerializeField] private GameObject gameStartPanel;
    [SerializeField] private GameObject gamePausePanel;
    [SerializeField] private GameObject gameOverPanel;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void StartGame()
    {
        GameManager.Instance.StartGame();
        Debug.Log("MainMenu.StartGame");
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        GameManager.Instance.PauseGame();
    }

    public void ResumeGame()
    {
        GameManager.Instance.ResumeGame();
        Time.timeScale = 1;
    }

    public void RestartGame()
    {
        GameManager.Instance.RestartGame();
        Time.timeScale = 1;
    }
}
