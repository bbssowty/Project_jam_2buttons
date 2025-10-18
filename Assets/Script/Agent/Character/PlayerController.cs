using UnityEngine;
using DG.Tweening;
using System.Collections;

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
    public float moveSpeed = 7f;
    public float jumpForce = 14f;
    [Tooltip("Set this to everything EXCEPT the 'Player' layer.")]
    public LayerMask groundLayer;
    [Tooltip("Set this to everything EXCEPT the 'Player' layer.")]
    public LayerMask wallLayer;

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
    public float wallJumpWindow = 0.2f;
    public Vector2 wallJumpForce = new Vector2(10f, 18f);

    [Header("Feedback Settings")]
    public FeedbackSettings jumpFeedback;
    public FeedbackSettings landFeedback;
    public FeedbackSettings wallHitFeedback;

    public Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGroundedLastFrame;
    private float moveDirection = 1f;
    private OrbController currentOrb = null;
    private float wallHitCooldown = 0.3f;
    private float lastWallHitTime;
    private bool _canWallJump = false;
    private Coroutine _wallHitCoroutine;
    private float baseMoveSpeed;
    private Coroutine speedBoostCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameCamera = Camera.main.GetComponent<CameraFollow>();
        baseMoveSpeed = moveSpeed;
    }

    void Update()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);

        if (isGrounded && !wasGroundedLastFrame) OnLand();
        wasGroundedLastFrame = isGrounded;

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                OnJump();
            }
            else if (_canWallJump)
            {
                PerformWallJump();
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
        bool isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0, wallLayer);

        // --- AIRBORNE WALL CHECK ---
        if (!isGrounded && isTouchingWall && Time.time > lastWallHitTime + wallHitCooldown)
        {
            // Stop any previous coroutine and start a new one.
            if (_wallHitCoroutine != null) StopCoroutine(_wallHitCoroutine);
            _wallHitCoroutine = StartCoroutine(WallHitSequence());

            // CRUCIAL FIX: Record the hit time HERE to prevent the coroutine from being spammed.
            lastWallHitTime = Time.time;
        }

        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
    }

    // --- GROUNDED WALL CHECK ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isGrounded)
        {
            if ((wallLayer.value & (1 << collision.gameObject.layer)) > 0 && Time.time > lastWallHitTime + wallHitCooldown)
            {
                ContactPoint2D contact = collision.contacts[0];
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    ReverseDirection();
                }
            }
        }
    }

    private void PerformWallJump()
    {
        _canWallJump = false;

        rb.linearVelocity = new Vector2(wallJumpForce.x * -moveDirection, wallJumpForce.y);
        ReverseDirection();
        OnJump(); // Play jump feedback

        if (_wallHitCoroutine != null)
        {
            StopCoroutine(_wallHitCoroutine);
            _wallHitCoroutine = null;
        }
    }

    private IEnumerator WallHitSequence()
    {
        _canWallJump = true;
        yield return new WaitForSeconds(wallJumpWindow);

        if (_canWallJump)
        {
            _canWallJump = false;
            ReverseDirection(); // Automatically reverse if player didn't jump.
        }
        _wallHitCoroutine = null;
    }

    /// <summary>
    /// Handles the logic for reversing direction and playing feedback.
    /// </summary>
    private void ReverseDirection()
    {
        moveDirection *= -1f;
        lastWallHitTime = Time.time; // Update cooldown timer
        wallCheck.localPosition = new Vector3(-wallCheck.localPosition.x, wallCheck.localPosition.y, wallCheck.localPosition.z);
        OnWallHit(); // Play feedback
    }

    // --- GAMEPLAY MODIFIERS ---
    public void ApplySpeedBoost(float amount)
    {
        moveSpeed += amount;
    }

    public void EnterOrbZone(OrbController orb) { currentOrb = orb; }
    public void ExitOrbZone(OrbController orb) { if (currentOrb == orb) { currentOrb = null; } }

    // --- FEEDBACK METHODS ---
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

    // --- VISUAL DEBUGGING ---
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