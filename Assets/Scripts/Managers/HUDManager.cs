using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Manages the HUD elements 
/// </summary>
public class HUDManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _speedText;
    [SerializeField] private GameObject _goalBanner;

    private int _score;

    void Awake()
    {
        GameManager.OnGameRestart += Reset;
        GameManager.OnShotUpdate += OnShotUpdate;

        _goalBanner.transform.localScale = Vector3.zero;
        _speedText.text = "";
    }

    private void OnShotUpdate(Shot shot)
    {
        // When shot starts
        if (shot.CurrentState == Shot.State.Start)
        {
            _speedText.text = string.Format("{0} km/h", Mathf.RoundToInt(shot.Speed * 3.6f)); // m/s -> km/h
        }
        else if (shot.CurrentState == Shot.State.Ready)
        {
            _speedText.text = "";
        }

        if (shot.CurrentState == Shot.State.Result)
        {
            if (shot.IsSuccess) // if goal scored
            {
                GoalBannerAnimation(); // Flash Goal text
                IncreaseScore(shot.Score); 
            }
        }
    }

    private void Reset()
    {
        // Reset Speed meter
        _speedText.text = "";

        // Reset Score
        _score = 0;
        UpdateScoreDisplay();

        // Reset Goal banner
        tween.Kill();
        _goalBanner.transform.localScale = Vector3.zero;
    }

    private void IncreaseScore(int amount)
    {
        _score += amount;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        _scoreText.text = string.Format("SCORE: {0}", _score);
    }

    Tweener tween;
    private void GoalBannerAnimation(Action OnCompleteAnim = null)
    {
        tween = _goalBanner.transform.DOScale(1f, 2f).SetEase(Ease.OutElastic).SetUpdate(true).OnComplete(() => {
            _goalBanner.transform.DOScale(0f, 1f).SetEase(Ease.InElastic).SetUpdate(true);
        });
    }
}
