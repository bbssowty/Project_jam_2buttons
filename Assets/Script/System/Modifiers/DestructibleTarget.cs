using UnityEngine;

public class DestructibleTarget : MonoBehaviour
{
    [Header("Target Settings")]
    public float maxHealth = 3f;            // How many hits it can take
    private float currentHealth;

    [Header("Destruction")]
    public GameObject onDestroyPrefab;      // Optional: spawn on destruction
    public bool destroyOnImpact = true;     // Destroy immediately on any projectile hit

    [Header("Audio")]
    [Tooltip("Sound to play when the target is destroyed.")]
    public AudioClip destroySound;
    [Tooltip("Volume for the destruction sound.")]
    [Range(0f, 1f)]
    public float destroyVolume = 1f;

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
        // Play destruction sound even after destroying this object
        if (destroySound != null)
        {
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = transform.position;

            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = destroySound;
            source.volume = destroyVolume;
            source.Play();

            Destroy(tempAudio, destroySound.length); // Destroy after the clip finishes
        }

        // Spawn optional prefab
        if (onDestroyPrefab != null)
            Instantiate(onDestroyPrefab, transform.position, Quaternion.identity);

        // Destroy this target object
        Destroy(gameObject);
    }
}
