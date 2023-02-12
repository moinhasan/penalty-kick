using UnityEngine;

public class SimpleUIAnimation : MonoBehaviour
{
    [SerializeField] private AnimationUIBase _animationUI;
    [SerializeField] private float _duration = 0.2f;

    private void Awake()
    {
        _animationUI.Animate(gameObject, _duration);
    }
}

