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
    [Tooltip("An empty object positioned where the ground check should occur.")]
    public Transform groundCheck;
    [Tooltip("The size of the box used to detect ground.")]
    public Vector2 groundCheckSize = new Vector2(0.9f, 0.2f);

    [Header("Wall Check")]
    [Tooltip("An empty object positioned where the wall check should occur.")]
    public Transform wallCheck;
    [Tooltip("The size of the box used to detect walls.")]
    public Vector2 wallCheckSize = new Vector2(0.2f, 0.9f);

    [Header("Feedback Settings")]
    public FeedbackSettings jumpFeedback;
    public FeedbackSettings landFeedback;
    public FeedbackSettings wallHitFeedback;

    // Private component references and state variables
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGroundedLastFrame;
    private float moveDirection = 1f;
    private OrbController currentOrb = null;

    // Cooldown to prevent rapid direction changes when stuck to a wall
    private float wallHitCooldown = 0.2f;
    private float lastWallHitTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameCamera = Camera.main.GetComponent<CameraFollow>();
    }

    void Update()
    {
        // Ground check is performed at the position of the 'groundCheck' transform
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);

        if (isGrounded && !wasGroundedLastFrame)
        {
            OnLand();
        }
        wasGroundedLastFrame = isGrounded;

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                OnJump();
            }
            else if (currentOrb != null)
            {
                currentOrb.Activate(rb);
                OnJump();
                currentOrb = null;
            }
        }
    }

    void FixedUpdate()
    {
        // --- Active Wall Check with Cooldown ---
        bool isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0, wallLayer);


        // --- ADD THIS LINE ---
        Debug.Log("Is Touching Wall: " + isTouchingWall);
        // Only allow a direction change if enough time has passed since the last one
        if (isTouchingWall && Time.time > lastWallHitTime + wallHitCooldown)
        {
            moveDirection *= -1f;
            OnWallHit();
            lastWallHitTime = Time.time; // Record the time of this hit

            // Flip the wall check position to the other side
            wallCheck.localPosition = new Vector3(-wallCheck.localPosition.x, wallCheck.localPosition.y, wallCheck.localPosition.z);
        }

        // Apply constant movement
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
    }

    // ORB INTERACTION METHODS
    public void EnterOrbZone(OrbController orb) { currentOrb = orb; }
    public void ExitOrbZone(OrbController orb) { if (currentOrb == orb) { currentOrb = null; } }

    // FEEDBACK METHODS
    void OnJump()
    {
        spriteTransform.DOKill();
        spriteTransform.localScale = Vector3.one;
        spriteTransform.DOPunchScale(jumpFeedback.punchVector, jumpFeedback.duration, 1, jumpFeedback.elasticity)
                       .SetEase(jumpFeedback.easeCurve);
    }

    void OnLand()
    {
        spriteTransform.DOKill();
        spriteTransform.localScale = Vector3.one;
        spriteTransform.DOPunchScale(landFeedback.punchVector, landFeedback.duration, 1, landFeedback.elasticity)
                       .SetEase(landFeedback.easeCurve);

        if (gameCamera != null)
            gameCamera.TriggerShake(landFeedback.cameraShakeDuration, landFeedback.cameraShakeStrength);
    }

    void OnWallHit()
    {
        spriteTransform.DOKill();
        spriteTransform.localScale = Vector3.one;

        Vector3 directionalPunch = wallHitFeedback.punchVector;
        directionalPunch.x *= -moveDirection;

        spriteTransform.DOPunchScale(directionalPunch, wallHitFeedback.duration, 1, wallHitFeedback.elasticity)
                       .SetEase(wallHitFeedback.easeCurve);

        if (gameCamera != null)
            gameCamera.TriggerShake(wallHitFeedback.cameraShakeDuration, wallHitFeedback.cameraShakeStrength);
    }

    // VISUAL DEBUGGING
    void OnDrawGizmos()
    {
        // Ground Check Gizmo
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }

        // Wall Check Gizmo
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
    }
}