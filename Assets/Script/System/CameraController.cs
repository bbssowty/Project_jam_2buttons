using UnityEngine;
using DG.Tweening; // Import the DOTween namespace

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    // --- PUBLIC FEEDBACK METHOD ---

    /// <summary>
    /// Triggers a camera shake.
    /// </summary>
    /// <param name="duration">How long the shake should last.</param>
    /// <param name="strength">How intense the shake should be.</param>
    public void TriggerShake(float duration, float strength)
    {
        // DOShakePosition is a fantastic built-in DOTween function
        // It's non-additive, so it won't mess up our follow logic.
        transform.DOShakePosition(duration, strength);
    }
}