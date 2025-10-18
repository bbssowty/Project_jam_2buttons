using UnityEngine;
using DG.Tweening;

public class OrbController : MonoBehaviour
{
    [Header("Orb Settings")]
    [Tooltip("The upward force applied to the player when this orb is used.")]
    public float orbJumpForce = 20f;

    [Header("Feedback")]
    public float animDuration = 0.3f;

    // The 'isUsed' boolean has been removed.

    // Called by the player to activate the orb's effect
    public void Activate(Rigidbody2D playerRb)
    {
        // The check 'if (isUsed) return;' has been removed.

        // Apply the upward force to the player
        playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, orbJumpForce);

        // --- Feedback ---
        // Play a quick animation. The .OnComplete() part that deactivates
        // the orb has been removed.
        transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), animDuration, 1, 0.5f);
    }

    // When an object enters the trigger zone...
    private void OnTriggerEnter2D(Collider2D other)
    {
        // ...check if it's the player.
        if (other.CompareTag("Player"))
        {
            // Get the PlayerController component and tell it this orb is available
            other.GetComponent<PlayerController>()?.EnterOrbZone(this);
        }
    }

    // When an object exits the trigger zone...
    private void OnTriggerExit2D(Collider2D other)
    {
        // ...check if it's the player.
        if (other.CompareTag("Player"))
        {
            // Tell the PlayerController that this orb is no longer available
            other.GetComponent<PlayerController>()?.ExitOrbZone(this);
        }
    }
}