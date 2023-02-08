using UnityEngine;

public class SimpleUIAnimation : MonoBehaviour
{
    [SerializeField] private AnimationUIBase animationUI;
    [SerializeField] private float duration = 0.2f;

    private void Awake()
    {
        animationUI.Animate(gameObject, duration);
    }
}

