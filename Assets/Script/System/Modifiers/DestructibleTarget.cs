using UnityEngine;

public class DestructibleTarget : MonoBehaviour
{
    [Header("Target Settings")]
    public float maxHealth = 3f;
    private float currentHealth;

    [Header("Destruction")]
    public GameObject onDestroyPrefab;
    public bool destroyOnImpact = true;

    [Header("Audio")]
    [Tooltip("Sound to play when the target is destroyed.")]
    public AudioClip destroySound;
    [Tooltip("Volume for the destruction sound.")]
    [Range(0f, 1f)]
    public float destroyVolume = 1f;

    void OnEnable()
    {
        // Reset health whenever enabled
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
            DisableTarget();
    }

    private void DisableTarget()
    {
        // Play destruction sound
        if (destroySound != null)
        {
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = transform.position;

            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = destroySound;
            source.volume = destroyVolume;
            source.Play();

            Destroy(tempAudio, destroySound.length);
        }

        // Spawn optional prefab
        if (onDestroyPrefab != null)
            Instantiate(onDestroyPrefab, transform.position, Quaternion.identity);

        // Disable the target instead of destroying it
        gameObject.SetActive(false);
    }

    // Public method to reset the target
    public void ResetTarget()
    {
        currentHealth = maxHealth;
        gameObject.SetActive(true);
    }
}
