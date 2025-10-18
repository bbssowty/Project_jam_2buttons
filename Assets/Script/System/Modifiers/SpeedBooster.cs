using UnityEngine;

public class SpeedBooster : MonoBehaviour
{
    [Header("Booster Settings")]
    [Tooltip("The amount to add to the player's current speed.")]
    public float speedIncrease = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out PlayerController player))
            {
                player.ApplySpeedBoost(speedIncrease);
            }
        }
    }
}