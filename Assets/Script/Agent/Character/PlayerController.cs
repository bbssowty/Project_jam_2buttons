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
    private OrbController currentOrb = null; // Tracks the orb we are currently in

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

        // --- Modified Jump Logic ---
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                // Standard ground jump
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                OnJump();
            }
            else if (currentOrb != null) // If not grounded, check if we are in an orb's zone
            {
                // Orb jump!
                currentOrb.Activate(rb); // Tell the orb to activate and pass our Rigidbody
                OnJump(); // We can reuse the same jump feedback
                currentOrb = null; // The orb is used, so clear the reference
            }
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

    // --- ORB INTERACTION METHODS ---

    /// <summary>
    /// Called by an orb when the player enters its trigger.
    /// </summary>
    public void EnterOrbZone(OrbController orb)
    {
        currentOrb = orb;
    }

    /// <summary>
    /// Called by an orb when the player exits its trigger.
    /// </summary>
    public void ExitOrbZone(OrbController orb)
    {
        // Only clear the currentOrb if it's the one we are actually exiting.
        // This prevents bugs if trigger zones overlap.
        if (currentOrb == orb)
        {
            currentOrb = null;
        }
    }

    // --- FEEDBACK METHODS ---

    void OnJump()
    {
        spriteTransform.DOKill();
        spriteTransform.localScale = Vector3.one; // Reset scale
        spriteTransform.DOPunchScale(jumpFeedback.punchVector, jumpFeedback.duration, 1, jumpFeedback.elasticity)
                       .SetEase(jumpFeedback.easeCurve);
    }

    void OnLand()
    {
        spriteTransform.DOKill();
        spriteTransform.localScale = Vector3.one; // Reset scale
        spriteTransform.DOPunchScale(landFeedback.punchVector, landFeedback.duration, 1, landFeedback.elasticity)
                       .SetEase(landFeedback.easeCurve);

        if (gameCamera != null)
            gameCamera.TriggerShake(landFeedback.cameraShakeDuration, landFeedback.cameraShakeStrength);
    }

    void OnWallHit()
    {
        spriteTransform.DOKill();
        spriteTransform.localScale = Vector3.one; // Reset scale

        Vector3 directionalPunch = wallHitFeedback.punchVector;
        directionalPunch.x *= -moveDirection;

        spriteTransform.DOPunchScale(directionalPunch, wallHitFeedback.duration, 1, wallHitFeedback.elasticity)
                       .SetEase(wallHitFeedback.easeCurve);

        if (gameCamera != null)
            gameCamera.TriggerShake(wallHitFeedback.cameraShakeDuration, wallHitFeedback.cameraShakeStrength);
    }
}