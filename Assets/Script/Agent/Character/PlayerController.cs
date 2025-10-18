using UnityEngine;
using DG.Tweening; // Required for DOTween animations

/// <summary>
/// A reusable data structure to hold all feedback parameters.
/// Making it [System.Serializable] allows it to be edited in the Inspector.
/// </summary>
[System.Serializable]
public struct FeedbackSettings
{
    [Tooltip("How long the animation takes.")]
    public float duration;

    [Tooltip("The direction and intensity of the scale punch.")]
    public Vector3 punchVector;

    [Tooltip("How 'bouncy' the punch effect is. 0 is rigid, 1 is very elastic.")]
    [Range(0, 1)]
    public float elasticity;

    [Tooltip("The visual curve of the animation's easing.")]
    public AnimationCurve easeCurve;

    [Header("Camera Shake")]
    public float cameraShakeDuration;
    public float cameraShakeStrength;
}

public class PlayerController : MonoBehaviour
{
    [Header("Core Mechanics")]
    public float moveSpeed = 7f;
    public float jumpForce = 14f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("References")]
    [Tooltip("Assign the child object that holds the character's visuals here.")]
    public Transform spriteTransform;
    private CameraFollow gameCamera;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;

    [Header("Feedback Settings")]
    public FeedbackSettings jumpFeedback;
    public FeedbackSettings landFeedback;
    public FeedbackSettings wallHitFeedback;

    // Private component references and state variables
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGroundedLastFrame;
    private float moveDirection = 1f; // 1 for right, -1 for left

    void Start()
    {
        // Get component references at the start
        rb = GetComponent<Rigidbody2D>();
        // Find the main camera and get its follow script
        gameCamera = Camera.main.GetComponent<CameraFollow>();
    }

    void Update()
    {
        // Perform the ground check every frame
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // --- Landing Detection ---
        // Check if we were in the air last frame but are on the ground now
        if (isGrounded && !wasGroundedLastFrame)
        {
            OnLand();
        }
        // Update the state for the next frame's check
        wasGroundedLastFrame = isGrounded;

        // --- Jumping Input ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Apply jump force and trigger jump feedback
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            OnJump();
        }
    }

    void FixedUpdate()
    {
        // Apply constant horizontal movement in the physics update
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object we collided with is on the Wall layer
        if ((wallLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            // Reverse direction and trigger wall hit feedback
            moveDirection *= -1f;
            OnWallHit();
        }
    }

    // --- FEEDBACK METHODS ---

    void OnJump()
    {
        // Stop any currently running animations on the sprite
        spriteTransform.DOKill();
        // ** ADD THIS LINE **
        spriteTransform.localScale = Vector3.one; // Reset scale before animation

        // Use the parameters from our jumpFeedback struct
        spriteTransform.DOPunchScale(jumpFeedback.punchVector, jumpFeedback.duration, 1, jumpFeedback.elasticity)
                       .SetEase(jumpFeedback.easeCurve);
    }

    void OnLand()
    {
        spriteTransform.DOKill();
        // ** ADD THIS LINE **
        spriteTransform.localScale = Vector3.one; // Reset scale before animation

        // Use the parameters from our landFeedback struct
        spriteTransform.DOPunchScale(landFeedback.punchVector, landFeedback.duration, 1, landFeedback.elasticity)
                       .SetEase(landFeedback.easeCurve);

        // Trigger camera shake
        if (gameCamera != null)
            gameCamera.TriggerShake(landFeedback.cameraShakeDuration, landFeedback.cameraShakeStrength);
    }

    void OnWallHit()
    {
        spriteTransform.DOKill();
        // ** ADD THIS LINE **
        spriteTransform.localScale = Vector3.one; // Reset scale before animation

        // Create a temporary punch vector that respects the current movement direction
        Vector3 directionalPunch = wallHitFeedback.punchVector;
        directionalPunch.x *= -moveDirection;

        // Use the parameters from our wallHitFeedback struct
        spriteTransform.DOPunchScale(directionalPunch, wallHitFeedback.duration, 1, wallHitFeedback.elasticity)
                       .SetEase(wallHitFeedback.easeCurve);

        // Trigger camera shake
        if (gameCamera != null)
            gameCamera.TriggerShake(wallHitFeedback.cameraShakeDuration, wallHitFeedback.cameraShakeStrength);
    }
}