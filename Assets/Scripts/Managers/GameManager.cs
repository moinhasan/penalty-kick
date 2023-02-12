using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action OnGameInitialization;
    public static event Action OnGameStart;
    public static event Action OnGamePause;
    public static event Action OnGameResume;
    public static event Action OnGameRestart;
    public static event Action<int> OnGameOver;

    // In-Game Manager

    private int _score;
    public static event Action<Shot> OnShotUpdate;
    public static event Action OnGoalScored;
    public static event Action<int> OnTargetHit;

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        GameDataManager.LoadGameData(()=> InitializeGame());
    }

    private void InitializeGame()
    {
        OnGameInitialization?.Invoke();
    }

    public void StartGame()
    {
        Debug.Log("GameManager.StartGame");        
        OnGameStart?.Invoke();
    }

    public void EndGame()
    {
        OnGameOver?.Invoke(_score);
        _score = 0;
    }

    public void PauseGame()
    {
        OnGamePause?.Invoke();
    }

    public void ResumeGame()
    {
        OnGameResume?.Invoke();
    }

    public void RestartGame()
    {
        Debug.Log("GameManager.RestartGame");
        OnGameRestart?.Invoke();
    }

    // In-Game Manager

    public void ShotUpdate(Shot shot)
    {
        OnShotUpdate?.Invoke(shot);

        if (shot.CurrentState == Shot.State.Result)
        {
            if (shot.IsSuccess)
            {
                _score += shot.Score;
            }
        }
    }

    public void GoalScored()
    {
        OnGoalScored?.Invoke();
    }

    public void TargetHit(int points)
    {
        OnTargetHit?.Invoke(points);
    }

}
