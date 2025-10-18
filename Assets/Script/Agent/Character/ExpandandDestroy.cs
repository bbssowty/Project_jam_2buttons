using UnityEngine;
using DG.Tweening;

public class ExpandAndDestroy : MonoBehaviour
{
    [Header("Expansion Settings")]
    public Vector3 startScale = Vector3.zero;
    public Vector3 endScale = Vector3.one;
    public float duration = 0.5f;
    public AnimationCurve easeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Optional Delay Before Destroy")]
    public float delayBeforeDestroy = 0f;

    private void Start()
    {
        // Set initial scale
        transform.localScale = startScale;

        // Animate scale using DOTween
        transform.DOScale(endScale, duration)
                 .SetEase(easeCurve)
                 .OnComplete(() =>
                 {
                     // Optional delay before destroy
                     if (delayBeforeDestroy > 0f)
                     {
                         DOVirtual.DelayedCall(delayBeforeDestroy, () => Destroy(gameObject));
                     }
                     else
                     {
                         Destroy(gameObject);
                     }
                 });
    }
}
