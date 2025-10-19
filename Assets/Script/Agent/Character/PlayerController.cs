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
    public float jumpForce = 14f;
    [Tooltip("Set this to everything EXCEPT the 'Player' layer.")]
    public LayerMask groundLayer;
    [Tooltip("Set this to everything EXCEPT the 'Player' layer.")]
    public LayerMask wallLayer;

    [Header("Speed Settings")]
    public float baseMoveSpeed = 7f;       // default walking speed
    public float maxMoveSpeed = 12f;       // maximum speed
    public float speedReturnRate = 2f;     // units per second to return to base speed
    public float moveSpeed;                // current move speed

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
    [Tooltip("Tolerance time after leaving ground to allow wall jump")]
    public float groundedTolerance = 0.1f;

    [Header("Coyote Time")]
    [Tooltip("Grace period after leaving ground where you can still jump")]
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
    private bool _canWallJump = false;
    private Coroutine _wallHitCoroutine;

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
                OnLand();
        }
        else
        {
            timeSinceGrounded += Time.deltaTime;
        }
        wasGroundedLastFrame = isGrounded;

        // --- Jump Input ---
        if (Input.GetButtonDown("Fire1"))
        {
            if (isGrounded || timeSinceGrounded <= coyoteTime)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                OnJump();
            }
            else if (_canWallJump && timeSinceGrounded > groundedTolerance)
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

        // --- Wall check ---
        bool isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);

        if (isTouchingWall && Time.time > lastWallHitTime + wallHitCooldown)
        {
            // Only reverse if moving toward the wall
            RaycastHit2D hit = Physics2D.BoxCast(
                wallCheck.position,
                wallCheckSize,
                0f,
                Vector2.right * moveDirection,
                0.05f,
                wallLayer
            );

            if (hit.collider != null)
            {
                if (_wallHitCoroutine != null) StopCoroutine(_wallHitCoroutine);
                _wallHitCoroutine = StartCoroutine(WallHitSequence());
                lastWallHitTime = Time.time;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ((wallLayer.value & (1 << collision.gameObject.layer)) == 0) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Only consider mostly horizontal collisions
            if (Mathf.Abs(contact.normal.x) > 0.7f)
            {
                // Only reverse if moving toward the wall
                if (Mathf.Sign(moveDirection) != Mathf.Sign(contact.normal.x))
                {
                    if (Time.time > lastWallHitTime + wallHitCooldown)
                    {
                        ReverseDirection();
                        lastWallHitTime = Time.time;
                        break; // Only reverse once per collision
                    }
                }
            }
        }
    }

    private void PerformWallJump()
    {
        _canWallJump = false;

        rb.linearVelocity = new Vector2(wallJumpForce.x * -moveDirection, wallJumpForce.y);
        ReverseDirection();
        OnJump();

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

        // Confirm still touching wall before reversing
        bool isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0f, wallLayer);
        if (_canWallJump && isTouchingWall)
        {
            _canWallJump = false;

            RaycastHit2D hit = Physics2D.BoxCast(
                wallCheck.position,
                wallCheckSize,
                0f,
                Vector2.right * moveDirection,
                0.05f,
                wallLayer
            );

            if (hit.collider != null)
            {
                ReverseDirection();
                lastWallHitTime = Time.time;
            }
        }

        _wallHitCoroutine = null;
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