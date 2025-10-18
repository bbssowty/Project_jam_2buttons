using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 15f;          // Movement speed
    public float lifetime = 3f;        // Lifetime before auto-destroy
    public float damage = 1f;          // Optional damage
    public int maxBounces = 3;         // How many times it can bounce
    public bool destroyOnImpact = true;

    private Vector2 direction;
    private int currentBounces = 0;

    public void Initialize(Vector2 shootDirection)
    {
        direction = shootDirection.normalized;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore the player
        if (collision.gameObject.CompareTag("Player")) return;

        // Reflect the direction based on the collision normal
        Vector2 normal = collision.contacts[0].normal;
        direction = Vector2.Reflect(direction, normal).normalized;

        currentBounces++;

        // Destroy if exceeded max bounces or on impact
        if (currentBounces > maxBounces || destroyOnImpact)
        {
            Destroy(gameObject);
        }
    }
}
