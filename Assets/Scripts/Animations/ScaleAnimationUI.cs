using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "UIAnimation/ Scale", fileName = "ScaleAnim")]
public class ScaleAnimationUI : AnimationUIBase
{
    [SerializeField] private float _scaleFactor = 1.2f;
    [SerializeField] private int _loop = 1;

    public override void Animate(GameObject gameObj, float duration, Action onCompleteEvent = null)
    {
        var sequence = DOTween.Sequence();
        var tween = gameObj.transform.DOScale(_scaleFactor, duration).SetLoops(_loop, LoopType.Yoyo);

        sequence.Append(tween);

        tween.OnComplete(() => { onCompleteEvent?.Invoke(); });
    }
}
