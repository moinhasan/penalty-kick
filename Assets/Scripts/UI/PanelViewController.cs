using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PanelViewController : MonoBehaviour
{
    protected enum ViewState
    {
        Hidden,
        Visible
    }
    protected ViewState viewState; 

    private float fadeAnimationTime = 0.25f;
    private CanvasGroup panelCanvasGroup;

    // Start is called before the first frame update
    protected virtual void Awake()
    {
        panelCanvasGroup = gameObject.GetComponent<CanvasGroup>();
        panelCanvasGroup.alpha = 0.0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
    }

    public virtual void HidePanel(Action action = null)
    {
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        panelCanvasGroup.DOFade(0, fadeAnimationTime).OnComplete(() => {
            action?.Invoke();
            viewState = ViewState.Hidden;
        }).SetUpdate(true);
    }

    public virtual void ShowPanel(Action action = null)
    {
        panelCanvasGroup.interactable = true;
        panelCanvasGroup.blocksRaycasts = true;
        panelCanvasGroup.DOFade(1, fadeAnimationTime).OnComplete(() => {
            action?.Invoke();
            viewState = ViewState.Visible;
        }).SetUpdate(true);
    }

}
