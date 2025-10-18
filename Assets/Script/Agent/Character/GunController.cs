using UnityEngine;

public class GunController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The bullet prefab to be fired.")]
    public GameObject projectilePrefab;
    [Tooltip("The point from which the projectile will be fired.")]
    public Transform firePoint;

    [Header("Gun Settings")]
    [Tooltip("Shots per second.")]
    public float fireRate = 2f;

    private float _nextFireTime = 0f;
    private Vector2 _mousePosition;

    void Update()
    {
        // --- Aiming Logic ---
        // Get the mouse position in world coordinates
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // --- Firing Logic ---
        // Check for left mouse button click and if the cooldown has passed
        if (Input.GetButton("Fire1") && Time.time >= _nextFireTime)
        {
            // Update the cooldown timer
            _nextFireTime = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void FixedUpdate()
    {
        Vector2 lookDirection = _mousePosition - (Vector2)transform.position;

        // The original line:
        // float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

        // The MODIFIED line with a 90-degree offset:
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f; // Adjust this value

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Shoot()
    {
        // If we have a projectile and a fire point, spawn the projectile
        if (projectilePrefab != null && firePoint != null)
        {
            Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            // We could add muzzle flash or sound effects here
        }
    }
}