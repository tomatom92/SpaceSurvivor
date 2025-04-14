using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Shooting Parameters")]
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] public float attackDelay = 0.4f;
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float burstInterval = 0.1f;

    [Header("Shooting Patterns")]
    [SerializeField] public ShootingPattern shootingPattern = ShootingPattern.Aimed;
    [SerializeField] private int spreadBulletCount = 3;
    [SerializeField] private float spreadAngle = 30f;
    [SerializeField] private float spiralSpeed = 120f;

    [Header("Behavior")]
    [SerializeField] private bool trackPlayer = true;


    private Vector3 playerLocation;
    private float attackCooldown = 0;
    private bool isBurstShooting = false;
    private EnemyState currentState;

    public enum EnemyState
    {
        still,
        attacking,
        initialized
    }
    

    public enum ShootingPattern
    {
        Straight,
        Aimed,
        Spread,
        Spiral,
        Burst,
        Random
    }

    private void Start()
    {
        // If no fire point is assigned, use the enemy's position
        if (firePoint == null)
            firePoint = transform;

        // Random initial cooldown to avoid all enemies shooting at once
        attackCooldown = Random.Range(0f, attackDelay);

        // Slightly randomize parameters for variety
        attackDelay *= Random.Range(0.8f, 1.2f);
        bulletSpeed *= Random.Range(0.9f, 1.1f);
    }

    private void Update()
    {
        // State machine
        switch (currentState)
        {
            case EnemyState.still:
                break;
            case EnemyState.attacking:
                break;
            case EnemyState.initialized:
                break;
        }

        // Update player location
        if (PlayerMovement.instance != null)
            playerLocation = PlayerMovement.instance.transform.position;

        // Attack cooldown
        if (attackCooldown <= 0 && !isBurstShooting)
        {
            // Choose attack style based on pattern
            switch (shootingPattern)
            {
                case ShootingPattern.Straight:
                    EnemyShootStraight();
                    break;
                case ShootingPattern.Aimed:
                    EnemyShootAimed();
                    break;
                case ShootingPattern.Spread:
                    EnemyShootSpread();
                    break;
                case ShootingPattern.Spiral:
                    StartCoroutine(EnemyShootSpiral());
                    break;
                case ShootingPattern.Burst:
                    StartCoroutine(EnemyShootBurst());
                    break;
                case ShootingPattern.Random:
                    // Choose a random pattern
                    int randomPattern = Random.Range(0, 5);
                    switch (randomPattern)
                    {
                        case 0: EnemyShootStraight(); break;
                        case 1: EnemyShootAimed(); break;
                        case 2: EnemyShootSpread(); break;
                        case 3: StartCoroutine(EnemyShootSpiral()); break;
                        case 4: StartCoroutine(EnemyShootBurst()); break;
                    }
                    break;
            }

            // Reset cooldown timer
            attackCooldown = attackDelay;
        }

        attackCooldown -= Time.deltaTime;
    }

    // Simple straight down shooting
    void EnemyShootStraight()
    {
        GameObject projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D projRB = projectile.GetComponent<Rigidbody2D>();
        projRB.AddForce(Vector2.down * bulletSpeed, ForceMode2D.Impulse);
        Destroy(projectile, 2f);
    }

    // Shoot aimed at player
    void EnemyShootAimed()
    {
        GameObject projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D projRB = projectile.GetComponent<Rigidbody2D>();

        if (trackPlayer && PlayerMovement.instance != null)
        {
            // Calculate direction to player
            Vector2 direction = (playerLocation - firePoint.position).normalized;

            // Apply force in that direction
            projRB.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);

            // Rotate bullet to face direction (if bullet has a sprite that needs to point in direction of travel)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);
        }
        else
        {
            // Default aiming with slight tracking
            Vector2 direction = new Vector2(
                (playerLocation.x - firePoint.position.x) * 0.1f,
                -1f
            ).normalized;

            projRB.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
        }

        Destroy(projectile, 2f);
    }

    // Shoot in a spread pattern
    void EnemyShootSpread()
    {
        float angleStep = spreadAngle / (spreadBulletCount - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < spreadBulletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);

            GameObject projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projRB = projectile.GetComponent<Rigidbody2D>();

            // Calculate direction with angle offset
            Vector2 baseDirection = Vector2.down;
            Vector2 direction = RotateVector(baseDirection, currentAngle);

            projRB.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);

            // Rotate bullet sprite
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);

            Destroy(projectile, 2f);
        }
    }

    // Shoot in a spiral pattern
    IEnumerator EnemyShootSpiral()
    {
        isBurstShooting = true;
        float angle = 0;

        for (int i = 0; i < 8; i++)
        {
            GameObject projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projRB = projectile.GetComponent<Rigidbody2D>();

            // Calculate direction with rotating angle
            Vector2 baseDirection = Vector2.down;
            Vector2 direction = RotateVector(baseDirection, angle);

            projRB.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);

            // Rotate bullet sprite
            float bulletAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.AngleAxis(bulletAngle + 90f, Vector3.forward);

            Destroy(projectile, 2f);

            // Increment angle for next shot
            angle += spiralSpeed / 8;

            yield return new WaitForSeconds(0.08f);
        }

        isBurstShooting = false;
    }

    // Shoot multiple bullets in quick succession
    IEnumerator EnemyShootBurst()
    {
        isBurstShooting = true;

        for (int i = 0; i < burstCount; i++)
        {
            GameObject projectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D projRB = projectile.GetComponent<Rigidbody2D>();

            if (trackPlayer)
            {
                // Recalculate player direction on each burst for more accurate tracking
                Vector2 direction = (playerLocation - firePoint.position).normalized;
                projRB.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);

                // Rotate bullet to face direction
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);
            }
            else
            {
                // Simple direction with slight aim
                Vector2 direction = new Vector2(
                    (playerLocation.x - firePoint.position.x) * 0.1f,
                    -1f
                ).normalized;

                projRB.AddForce(direction * bulletSpeed, ForceMode2D.Impulse);
            }

            Destroy(projectile, 2f);

            yield return new WaitForSeconds(burstInterval);
        }

        isBurstShooting = false;
    }

    // Helper method to rotate a vector by an angle
    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }
}