using BarthaSzabolcs.Tutorial_SpriteFlash;
using System.Collections;
using UnityEngine;

public class AsteroidEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minAngle = -30f;
    [SerializeField] private float maxAngle = 30f;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float damage = 20f; // Damage dealt to player on collision

    [Header("References")]
    [SerializeField] private Transform tailFlame; // Reference to the tail flame child object
    [SerializeField] private Animator animator; // Reference to the animator component
    [SerializeField] private ColoredFlash colorFlash; // Reference to the bullet prefab

    private float currentHealth;
    private Vector2 moveDirection;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isMoving = true;
    private bool isShooting = false;
    private Coroutine shootingCoroutine;

    private void Start()
    {
        // Set up references
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();

        // Store original color for damage indication
        originalColor = spriteRenderer.color;

        // Initialize health
        currentHealth = maxHealth;

        // Randomize initial angle and set up direction
        InitializeMovementDirection();
    }

    private void Update()
    {
        if (isMoving)
        {
            // Move in the current direction
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        }
    }

    public void InitializeMovementDirection()
    {
        // Determine a random angle within the specified range
        float randomAngle = Random.Range(minAngle, maxAngle);

        // Convert to radians and calculate direction vector
        float angleRad = randomAngle * Mathf.Deg2Rad;
        moveDirection = new Vector2(Mathf.Sin(angleRad), -Mathf.Cos(angleRad)).normalized;

        // Align tail flame to the movement direction (opposite of asteroid direction)
        if (tailFlame != null)
        {
            tailFlame.rotation = Quaternion.Euler(0, 0, randomAngle + 90f);
        }
    }

    // Public method to start shooting - can be called from other scripts or events
    public void StartShooting()
    {
        if (!isShooting)
        {
            isShooting = true;
            
        }
    }

    // Public method to stop shooting
    public void StopShooting()
    {
        if (isShooting && shootingCoroutine != null)
        {
            isShooting = false;
        }
    }

    // Public method to stop movement
    public void StopMoving()
    {
        isMoving = false;
    }

    // Public method to resume movement
    public void ResumeMoving()
    {
        isMoving = true;
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;

        // Visual feedback - turn more red based on health percentage
        float healthPercentage = currentHealth / maxHealth;
        spriteRenderer.color = Color.Lerp(Color.red, originalColor, healthPercentage);

        // Check if asteroid is destroyed
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Stop all movement and shooting
        isMoving = false;
        StopShooting();

        // Trigger explosion animation
        if (animator != null)
        {
            animator.SetTrigger("Explode");
        }
        //destroy tail flame
        if (tailFlame != null)
        {
            Destroy(tailFlame.gameObject);
        }

        // Disable colliders to prevent further interactions
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }

        // Destroy asteroid after animation plays (or immediately if no animator)
        if (animator != null)
        {
            // Get animation length and destroy after it completes
            AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
            float destroyDelay = 1f; // Default delay

            foreach (AnimationClip clip in clips)
            {
                if (clip.name.Contains("asteroidExplode"))
                {
                    destroyDelay = clip.length;
                    break;
                }
            }

            Destroy(gameObject, destroyDelay);
        }
        else
        {
            // If no animator, destroy after a short delay
            Destroy(gameObject, 0.5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if collided with player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get player health component and deal damage
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            // Trigger explosion animation
            if (animator != null)
            {
                animator.SetTrigger("Explode");
            }

            // Destroy asteroid
            Destroy(gameObject, 0.5f);
        }
        if (collision.gameObject.CompareTag("PlayerBullet"))
        {
            //Debug.Log("damage");
            TakeDamage(collision.transform.GetComponent<PlayerBullet>().damage);
        }
    }
}

// Helper class for projectile movement if no Rigidbody2D is used
public class ProjectileMovement : MonoBehaviour
{
    private Vector2 direction;
    private float speed;

    public void Initialize(Vector2 dir, float spd)
    {
        direction = dir;
        speed = spd;
    }

    private void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }
}
