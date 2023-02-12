using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for UI panels. Simply controls the visibility and accessibility of panels
/// </summary>
public class PanelViewController : MonoBehaviour
{
    protected enum ViewState
    {
        Hidden,
        Visible
    }
    protected ViewState viewState; 

    private float _fadeAnimationTime = 0.25f;
    private CanvasGroup _panelCanvasGroup;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        _panelCanvasGroup = gameObject.GetComponent<CanvasGroup>();
        _panelCanvasGroup.alpha = 0.0f;
        _panelCanvasGroup.interactable = false;
        _panelCanvasGroup.blocksRaycasts = false;
    }

    public virtual void HidePanel(Action action = null)
    {
        _panelCanvasGroup.interactable = false;
        _panelCanvasGroup.blocksRaycasts = false;
        _panelCanvasGroup.DOFade(0, _fadeAnimationTime).OnComplete(() => {
            action?.Invoke();
            viewState = ViewState.Hidden;
        }).SetUpdate(true);
    }

    public virtual void ShowPanel(Action action = null)
    {
        _panelCanvasGroup.interactable = true;
        _panelCanvasGroup.blocksRaycasts = true;
        _panelCanvasGroup.DOFade(1, _fadeAnimationTime).OnComplete(() => {
            action?.Invoke();
            viewState = ViewState.Visible;
        }).SetUpdate(true);
    }

}
