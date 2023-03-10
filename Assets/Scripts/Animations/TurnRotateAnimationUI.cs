using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "UIAnimation/ Turn Rotate", fileName = "TurnRotateAnim")]
public class TurnRotateAnimationUI : AnimationUIBase
{
    [SerializeField] private float _angle = -30f;
    [SerializeField] private int _loop = 1;

    public override void Animate(GameObject gameObj, float duration, Action onCompleteEvent = null)
    {
        var sequence = DOTween.Sequence();
        
        Tween tween = gameObj.transform.DORotate(Vector3.forward * _angle, duration/2f)
            .SetLoops(_loop, LoopType.Yoyo);
        
        sequence.Append(tween);
        sequence.OnComplete(() =>
        {
            gameObj.transform.rotation = Quaternion.identity;
            onCompleteEvent?.Invoke();
        });
    }
}
