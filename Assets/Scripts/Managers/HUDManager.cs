using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public delegate void GoalSuccessEvent();

public class HUDManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private GameObject goalBanner;

    private int score;
    private int hit;
    private int miss;

    private static HUDManager instance = null;
    public static HUDManager Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        GameManager.OnGameRestart += Reset;
        GameManager.OnShotUpdate += OnShotStateUpdate;

        if (instance == null)
        {
            instance = this;
        }
        else
        {
             Destroy(gameObject);
        }

        goalBanner.transform.localScale = Vector3.zero;
        speedText.text = "";
    }

    private void OnShotStateUpdate(Shot shot)
    {
        // When shot starts
        if (shot.CurrentState == Shot.State.Start)
        {
            speedText.text = string.Format("{0} km/h", Mathf.RoundToInt(shot.Speed * 3.6f)); // m/s -> km/h
        }
        else if (shot.CurrentState == Shot.State.Ready)
        {
            speedText.text = "";
        }

        if (shot.CurrentState == Shot.State.Result)
        {
            if (shot.IsSuccess)
            {
                GoalBannerAnimation();
                IncreaseScore(shot.Score);
            }
        }
    }

    public void GoalSuccess() {

    }

    public void TargetHitSuccess()
    {

    }

    private void Reset()
    {
        // Reset Speed meter
        speedText.text = "";

        // Reset Score
        score = 0;
        UpdateScoreDisplay();

        // Reset Goal banner
        tween.Kill();
        goalBanner.transform.localScale = Vector3.zero;
    }

    private void IncreaseScore(int amount)
    {
        score += amount;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        scoreText.text = string.Format("SCORE: {0}", score);
    }

    Tweener tween;
    private void GoalBannerAnimation(Action OnCompleteAnim = null)
    {
        tween = goalBanner.transform.DOScale(1f, 2f).SetEase(Ease.OutElastic).SetUpdate(true).OnComplete(() => {
            goalBanner.transform.DOScale(0f, 1f).SetEase(Ease.InElastic).SetUpdate(true);
        });
    }
}
