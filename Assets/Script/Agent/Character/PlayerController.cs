using UnityEngine;
using DG.Tweening;

[System.Serializable]
public struct FeedbackSettings
{
    public float duration;
    public Vector3 punchVector;
    [Range(0, 1)]
    public float elasticity;
    public AnimationCurve easeCurve;
    [Header("Camera Shake")]
    public float cameraShakeDuration;
    public float cameraShakeStrength;
}

public class PlayerController : MonoBehaviour
{
    [Header("Core Mechanics")]
    public float jumpForce = 14f;
    [Tooltip("Set this to everything EXCEPT the 'Player' layer.")]
    public LayerMask groundLayer;
    [Tooltip("Set this to everything EXCEPT the 'Player' layer.")]
    public LayerMask wallLayer;

    [Header("Speed Settings")]
    public float baseMoveSpeed = 7f;
    public float maxMoveSpeed = 12f;
    public float speedReturnRate = 2f;
    public float moveSpeed;

    [Header("References")]
    public Transform spriteTransform;
    private CameraFollow gameCamera;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.9f, 0.2f);

    [Header("Wall Check")]
    public Transform wallCheck;
    public Vector2 wallCheckSize = new Vector2(0.2f, 0.9f);

    [Header("Wall Jump")]
    public Vector2 wallJumpForce = new Vector2(10f, 18f);

    [Header("Coyote Time")]
    public float coyoteTime = 0.15f;

    [Header("Feedback Settings")]
    public FeedbackSettings jumpFeedback;
    public FeedbackSettings landFeedback;
    public FeedbackSettings wallHitFeedback;

    [Header("Internal References")]
    public Rigidbody2D rb;

    private bool isGrounded;
    private bool wasGroundedLastFrame;
    private float timeSinceGrounded = 0f;
    private float moveDirection = 1f;
    private OrbController currentOrb = null;
    private float wallHitCooldown = 0.3f;
    private float lastWallHitTime;
    private bool isTouchingWall;
    private bool hasFlippedSinceLanding = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameCamera = Camera.main.GetComponent<CameraFollow>();
        moveSpeed = baseMoveSpeed;
    }

    void Update()
    {
        // --- Ground Check ---
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);
        if (isGrounded)
        {
            timeSinceGrounded = 0f;
            if (!wasGroundedLastFrame)
            {
                OnLand();
                hasFlippedSinceLanding = false; // reset flip flag on landing
            }
        }
        else
        {
            timeSinceGrounded += Time.deltaTime;
        }
        wasGroundedLastFrame = isGrounded;

        // --- Wall Check ---
        isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);

        // --- Wall flip when grounded and touching wall ---
        if (isGrounded && isTouchingWall && !hasFlippedSinceLanding && Time.time > lastWallHitTime + wallHitCooldown)
        {
            ReverseDirection();
            lastWallHitTime = Time.time;
            hasFlippedSinceLanding = true; // prevent repeated flips while standing
        }

        // --- Jump Input ---
        if (Input.GetButtonDown("Fire1"))
        {
            // Ground jump has priority
            if (isGrounded || timeSinceGrounded <= coyoteTime)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                OnJump();
            }
            // Wall jump only in air
            else if (isTouchingWall && !isGrounded)
            {
                PerformWallJump();
            }
            // Orb jump
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
        // --- Move player ---
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);

        // --- Gradually return speed to base ---
        if (moveSpeed > baseMoveSpeed)
        {
            moveSpeed -= speedReturnRate * Time.fixedDeltaTime;
            if (moveSpeed < baseMoveSpeed) moveSpeed = baseMoveSpeed;
        }
        else if (moveSpeed < baseMoveSpeed)
        {
            moveSpeed += speedReturnRate * Time.fixedDeltaTime;
            if (moveSpeed > baseMoveSpeed) moveSpeed = baseMoveSpeed;
        }
    }

    private void PerformWallJump()
    {
        rb.linearVelocity = new Vector2(wallJumpForce.x * -moveDirection, wallJumpForce.y);
        ReverseDirection();
        OnJump();
    }

    private void ReverseDirection()
    {
        moveDirection *= -1f;
        lastWallHitTime = Time.time;

        wallCheck.localPosition = new Vector3(-wallCheck.localPosition.x, wallCheck.localPosition.y, wallCheck.localPosition.z);
        OnWallHit();
    }

    public void ApplySpeedBoost(float amount)
    {
        moveSpeed += amount;
        moveSpeed = Mathf.Min(moveSpeed, maxMoveSpeed);
    }

    public void EnterOrbZone(OrbController orb) { currentOrb = orb; }
    public void ExitOrbZone(OrbController orb) { if (currentOrb == orb) currentOrb = null; }

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
    }

    void OnWallHit()
    {
        spriteTransform.DOKill();
        spriteTransform.localScale = Vector3.one;

        Vector3 directionalPunch = wallHitFeedback.punchVector;
        directionalPunch.x *= -moveDirection;

        spriteTransform.DOPunchScale(directionalPunch, wallHitFeedback.duration, 1, wallHitFeedback.elasticity)
                       .SetEase(wallHitFeedback.easeCurve);
    }

    void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(wallCheck.position, wallCheckSize);
        }
    }
}
