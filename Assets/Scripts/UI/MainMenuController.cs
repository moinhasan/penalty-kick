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

    // Start is called before the first frame update
    void Start()
    {

    }

    public void StartGame()
    {
        //ShotController.instance.StartGame();
        GameManager.Instance.StartGame();
        Debug.Log("MainMenu.StartGame");
    }

    public void GameOver(int score)
    {
        Debug.Log("MainMenu.GameOver");
        //scoreText.text = "Score: " + score;
        GameManager.Instance.EndGame(score);
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

    public void OnGameOver()
    {
        //scoreText.text = "Score: " + score;
    }

    void OnEnable()
    {
        GameManager.OnGameOver += OnGameOver; // register with game over event

    }

    void OnDisable()
    {
        GameManager.OnGameOver -= OnGameOver;

    }
}
