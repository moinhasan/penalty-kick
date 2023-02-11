using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine;

public class GameOverPanel : PanelViewController
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private RectTransform restartButtonRect;
    private float animationTime = 0.5f;

    protected override void Awake()
    {
        base.Awake();

        restartButtonRect.transform.localPosition = new Vector3(-1000f, -670f, 0f);
    }

    public override void ShowPanel(Action action = null)
    {
        base.ShowPanel(() => {
            restartButtonRect.DOAnchorPos(new Vector2(-400f, -670f), animationTime, false).SetEase(Ease.OutElastic).SetUpdate(true);
            action?.Invoke();
        });
    }

    public override void HidePanel(Action action = null)
    {
        base.HidePanel(() => {
            restartButtonRect.DOAnchorPos(new Vector2(-1000f, -670f), animationTime, false).SetEase(Ease.InOutQuint).SetUpdate(true);
            action?.Invoke();
        });
    }

    void OnGameOver(int score)
    {
        scoreText.text = string.Format("Score: {0}", score);
        if (viewState == ViewState.Hidden) ShowPanel();
    }

    void OnGameRestart()
    {
        if (viewState == ViewState.Visible) HidePanel();
    }

    void OnEnable()
    {
        GameManager.OnGameOver += OnGameOver; // register with game over event
        GameManager.OnGameRestart += OnGameRestart;
    }

    void OnDisable()
    {
        GameManager.OnGameOver -= OnGameOver;
        GameManager.OnGameRestart -= OnGameRestart;
    }
}
