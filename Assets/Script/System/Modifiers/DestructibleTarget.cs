using UnityEngine;

public class DestructibleTarget : MonoBehaviour
{
    [Header("Target Settings")]
    public float maxHealth = 3f;            // How many hits it can take
    private float currentHealth;

    [Header("Destruction")]
    public GameObject onDestroyPrefab;      // Optional: spawn on destruction
    public bool destroyOnImpact = true;     // Destroy immediately on any projectile hit

    void Start()
    {
        currentHealth = maxHealth;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Projectile projectile = collision.gameObject.GetComponent<Projectile>();
        if (projectile != null)
        {
            TakeDamage(projectile.damage);

            if (projectile.destroyOnImpact)
                Destroy(projectile.gameObject);
        }
    }

    private void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
            DestroyTarget();
    }

    private void DestroyTarget()
    {
        if (onDestroyPrefab != null)
            Instantiate(onDestroyPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
