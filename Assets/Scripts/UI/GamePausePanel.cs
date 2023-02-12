using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GamePausePanel : PanelViewController
{
    [SerializeField] private RectTransform _resumeButtonRect;
    [SerializeField] private RectTransform _restartButtonRect;
    private float _animationTime = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        _resumeButtonRect.transform.localPosition = new Vector3(-1000f, -680f, 0f);
        _restartButtonRect.transform.localPosition = new Vector3(-1000f, -490f, 0f);
    }

    public override void ShowPanel(Action action = null)
    {
        base.ShowPanel(() => {
            _resumeButtonRect.DOAnchorPos(new Vector2(-400f, -680f), _animationTime, false).SetEase(Ease.OutElastic).SetUpdate(true);
            _restartButtonRect.DOAnchorPos(new Vector2(-400f, -490f), _animationTime, false).SetEase(Ease.OutElastic).SetUpdate(true);
            action?.Invoke();
        });
    }

    public override void HidePanel(Action action = null)
    {
        // SetUpdate sets the update type to UpdateType.Normal and lets you choose if it should be independent from Unity's Time.timeScale
        _resumeButtonRect.DOAnchorPos(new Vector2(-1000f, -680f), _animationTime, false).SetEase(Ease.InOutQuint).SetUpdate(true);
        _restartButtonRect.DOAnchorPos(new Vector2(-1000f, -490f), _animationTime, false).SetEase(Ease.InOutQuint).SetUpdate(true);
        base.HidePanel(() => {
            Debug.Log("GamePausePanel.Resume");
            action?.Invoke();
        });
    }

    private void OnGamePause()
    {
        if (viewState == ViewState.Hidden) ShowPanel();
    }

    private void OnGameResume()
    {
        if (viewState == ViewState.Visible) HidePanel();
    }

    private void OnGameRestart()
    {
        if (viewState == ViewState.Visible) HidePanel();
    }

    void OnEnable()
    {
        GameManager.OnGamePause += OnGamePause; // register with game pause event
        GameManager.OnGameResume += OnGameResume; // register with game resume event
        GameManager.OnGameRestart += OnGameRestart; // register with game restart event
    }

    void OnDisable()
    {
        GameManager.OnGamePause -= OnGamePause; // unregister with game pause event
        GameManager.OnGameResume -= OnGameResume; // unregister with game resume event
        GameManager.OnGameRestart -= OnGameRestart; // unregister with game restart event
    }
}
