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
    public Vector2 _mousePosition;



    void Update()
    {

        // --- Aiming Logic ---
        _mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);


        Vector2 lookDirection = _mousePosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f; // adjust if sprite points up
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // --- Firing Logic ---
        if (Input.GetButton("Fire1") && Time.time >= _nextFireTime)
        {
            _nextFireTime = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

        // Determine the shooting direction from firePoint
        Vector2 shootDir = firePoint.up; // Assuming firePoint points up after rotation
        Projectile projectile = proj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(shootDir);
        }
    }
}
