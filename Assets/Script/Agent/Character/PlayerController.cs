using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Public variables
    public float moveSpeed = 7f;
    public float jumpForce = 14f;
    public LayerMask groundLayer;
    public LayerMask wallLayer; // New LayerMask for walls

    // Ground check setup
    public Transform groundCheck;
    public float checkRadius = 0.2f;

    // Components and state tracking
    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveDirection = 1f; // 1 for right, -1 for left

    // Called once when the script instance is being loaded
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Called every frame
    void Update()
    {
        // --- Ground Check ---
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // --- Jumping ---
        // Player can still control the jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    // Called every fixed frame-rate frame
    void FixedUpdate()
    {
        // --- Constant Movement ---
        // The horizontal input is now replaced by our moveDirection variable
        rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
    }

    // This function is called whenever this collider/rigidbody has begun touching another rigidbody/collider
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object we collided with is on the Wall layer
        if ((wallLayer.value & (1 << collision.gameObject.layer)) > 0)
        {
            // If it is a wall, flip the move direction
            moveDirection *= -1f;
        }
    }
}