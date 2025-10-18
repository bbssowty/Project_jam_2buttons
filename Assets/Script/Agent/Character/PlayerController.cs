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

    [Header("Feedback Settings")]
    public FeedbackSettings jumpFeedback;
    public FeedbackSettings landFeedback;
    public FeedbackSettings wallHitFeedback;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool wasGroundedLastFrame;
    private float moveDirection = 1f;
    private OrbController currentOrb = null;
    private float wallHitCooldown = 0.2f;
    private float lastWallHitTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        gameCamera = Camera.main.GetComponent<CameraFollow>();
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
        // --- AIRBORNE WALL CHECK ---
        if (!isGrounded)
        {
            bool isTouchingWall = Physics2D.OverlapBox(wallCheck.position, wallCheckSize, 0, wallLayer);
            if (isTouchingWall && Time.time > lastWallHitTime + wallHitCooldown)
            {
                HandleWallHit();
            }
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
                    HandleWallHit();
                }
            }
        }
    }

    private void HandleWallHit()
    {
        moveDirection *= -1f;
        OnWallHit();
        lastWallHitTime = Time.time;
        wallCheck.localPosition = new Vector3(-wallCheck.localPosition.x, wallCheck.localPosition.y, wallCheck.localPosition.z);
    }


    // --- ORB INTERACTION METHODS ---
    public void EnterOrbZone(OrbController orb)
    {
        currentOrb = orb;
    }

    public void ExitOrbZone(OrbController orb)
    {
        if (currentOrb == orb)
        {
            currentOrb = null;
        }
    }

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

    // --- VISUAL DEBUGGING ---
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