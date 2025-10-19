using UnityEngine;
using DG.Tweening;

public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    public Transform target;              // Player transform
    public GunController gun;             // For aim offset

    [Header("Follow Settings")]
    public float smoothTime = 0.25f;
    private Vector3 velocity = Vector3.zero;

    [Header("Zoom Settings")]
    public float minZoom = 5f;
    public float maxZoom = 8f;
    public float zoomLerpSpeed = 2f;
    [Tooltip("Maximum player speed used to scale zoom.")]
    public float playerMaxSpeed = 10f;

    [Header("Aim Offset Settings")]
    public float baseOffsetDistance = 2f;
    public float minOffsetDistance = 0.5f;
    public float maxOffsetDistance = 4f;
    public float aimOffsetSmooth = 5f;

    [Header("Vertical Offset")]
    public float verticalOffset = 1f;    // Optional extra vertical offset

    private Camera cam;
    private Vector3 aimOffset = Vector3.zero;

    void Start()
    {
        cam = Camera.main;
        if (target == null)
            Debug.LogWarning("CameraFollow: Missing target reference!");
        if (gun == null)
            Debug.LogWarning("CameraFollow: Missing GunController reference!");
    }

    void LateUpdate()
    {
        if (target == null || gun == null) return;

        // --- 1. Compute player speed (Rigidbody velocity magnitude) ---
        Rigidbody2D rb = target.GetComponent<Rigidbody2D>();
        float currentSpeed = rb != null ? rb.linearVelocity.magnitude : 0f;

        // --- 2. Zoom based on speed ---
        float normalizedSpeed = Mathf.Clamp01(currentSpeed / playerMaxSpeed);
        float targetZoom = Mathf.Lerp(minZoom, maxZoom, normalizedSpeed);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomLerpSpeed);

        // --- 3. Compute aim offset (horizontal + vertical) ---
        Vector2 aimDirection = (gun._mousePosition - (Vector2)target.position).normalized;

        // Scale offset with camera size
        float scaledOffset = baseOffsetDistance * (cam.orthographicSize / minZoom);
        scaledOffset = Mathf.Clamp(scaledOffset, minOffsetDistance, maxOffsetDistance);

        // Compute desired offset
        Vector3 desiredOffset = new Vector3(
            aimDirection.x * scaledOffset,        // horizontal
            aimDirection.y * scaledOffset + verticalOffset, // vertical + extra
            0f
        );

        // Smooth aim offset
        aimOffset = Vector3.Lerp(aimOffset, desiredOffset, Time.deltaTime * aimOffsetSmooth);

        // --- 4. Move camera ---
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z) + aimOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    // Optional: shake
    public void TriggerShake(float duration, float strength)
    {
        transform.DOShakePosition(duration, strength);
    }
}
