using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public delegate void GoalSuccessEvent();

public class ScoreManager : MonoBehaviour //HUD Manager
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject goalBanner;

    private int score;
    private int hit;
    private int miss;

    private static ScoreManager instance = null;
    public static ScoreManager Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        GameManager.OnGameRestart += Reset;

        if (instance == null)
        {
            instance = this;
        }
        else
        {
             Destroy(gameObject);
        }

        goalBanner.transform.localScale = Vector3.zero;
    }

    public void GoalSuccess() {

    }

    public void TargetHitSuccess()
    {

    }

    private void Reset()
    {
        score = 0;
        UpdateScoreDisplay();
        tween.Kill();
        goalBanner.transform.localScale = Vector3.zero;
    }

    public void ProcessResult(Shot shot)
    {
        if (shot.IsSuccess)
        {
            GoalBannerAnimation();
            IncreaseScore(shot.Score);
        }
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
