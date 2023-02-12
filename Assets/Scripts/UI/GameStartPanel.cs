using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartPanel : PanelViewController
{
    private void OnGameStart()
    {
        if(viewState == ViewState.Visible) HidePanel();
    }

    private void OnGameInitialization()
    {
        if (viewState == ViewState.Hidden) ShowPanel();
    }

    void OnEnable()
    {
        GameManager.OnGameInitialization += OnGameInitialization;
        GameManager.OnGameStart += OnGameStart; // register with game start event
    }

    void OnDisable()
    {
        GameManager.OnGameInitialization -= OnGameInitialization;
        GameManager.OnGameStart -= OnGameStart;
    }
}
