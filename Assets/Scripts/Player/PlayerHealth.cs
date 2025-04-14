using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] float maxHealth;
    [SerializeField] public float lives = 1;
    float currentHealth;

    [SerializeField] Image healthBar; // Reference to the health bar UI element
    [SerializeField] float shakeDuration = 0.1f;
    [SerializeField] float shakeMagnitude = 5f;

    [Header("Death Animation Settings")]
    [SerializeField] GameObject smallExplosionPrefab; // Small explosion prefab
    [SerializeField] GameObject finalExplosionPrefab; // Final large explosion prefab
    [SerializeField] float deathAnimationDuration = 1.5f; // How long the death animation plays
    [SerializeField] int smallExplosionCount = 6; // Number of small explosions
    [SerializeField] float rockingIntensity = 15f; // How dramatically the ship rocks
    [SerializeField] float rockingSpeed = 15f; // Speed of rocking
    [SerializeField] float explosionScaleMin = 0.3f; // Minimum scale for small explosions
    [SerializeField] float explosionScaleMax = 0.7f; // Maximum scale for small explosions
    public GameObject thrusterObject; // Reference to the thruster object

    private bool isDying = false;
    private SpriteRenderer spriteRenderer;

    public static PlayerHealth instance;
    private void Awake()
    {
        instance = this;
        // Ensure the health bar is set to the full size at the start
        if (healthBar != null)
        {
            healthBar.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        spriteRenderer = GetComponent<SpriteRenderer>();
        DontDestroyOnLoad(gameObject); // Keep this object alive across scenes
    }

    public void TakeDamage(float damage)
    {
        if (isDying) return; // Prevent taking damage during death animation

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            StartCoroutine(PlayDeathAnimation());
        }
        else
        {
            UpdateHealthBar();
            StartCoroutine(ShakeHealthBar());
        }
    }

    public void IncreaseMaxHealth(float additionalHealth)
    {
        maxHealth += additionalHealth;
        currentHealth += additionalHealth; // Optionally heal the player to the new max health
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        float healthPercentage = currentHealth / maxHealth;
        healthBar.transform.localScale = new Vector3(healthPercentage, 1, 1);
    }

    IEnumerator ShakeHealthBar()
    {
        Vector3 originalPosition = healthBar.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            healthBar.transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        healthBar.transform.localPosition = originalPosition;
    }

    IEnumerator PlayDeathAnimation()
    {
        isDying = true;
        Debug.Log("Player is dying");

        // Disable player controls/movement here
        DisablePlayerControls();

        // Original rotation and position for reference
        Quaternion originalRotation = transform.rotation;
        Vector3 originalScale = transform.localScale;

        // Small explosions and rocking phase
        float elapsedTime = 0f;
        float explosionInterval = deathAnimationDuration / smallExplosionCount;
        float nextExplosionTime = 0f;

        // Flickering and color change variables
        float flickerSpeed = 20f;
        Color originalColor = spriteRenderer.color;
        Color damageColor = Color.red;

        while (elapsedTime < deathAnimationDuration)
        {
            // Create small random explosions at intervals
            if (elapsedTime >= nextExplosionTime)
            {
                CreateSmallExplosion();
                nextExplosionTime += explosionInterval;
            }

            // Rock the ship side to side (intensifies over time)
            float rockProgress = elapsedTime / deathAnimationDuration;
            float rockIntensity = rockingIntensity * rockProgress;
            float rockAngle = Mathf.Sin(elapsedTime * rockingSpeed) * rockIntensity;
            transform.rotation = originalRotation * Quaternion.Euler(0, 0, rockAngle);

            // Flicker the sprite
            float flicker = Mathf.PingPong(elapsedTime * flickerSpeed, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, damageColor, flicker);

            // Scale starts to pulse/fluctuate
            float scaleFluctuation = 1f + Mathf.Sin(elapsedTime * 15f) * 0.1f * rockProgress;
            transform.localScale = originalScale * scaleFluctuation;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Final explosion and destroy player
        if (finalExplosionPrefab != null)
        {
            GameObject finalExplosion = Instantiate(finalExplosionPrefab, transform.position, Quaternion.identity);
            // Optional: Scale up the final explosion
            finalExplosion.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);

            // You might want to play a sound effect here
            // AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        }

        // Hide player sprite
        spriteRenderer.enabled = false;

        // Wait a moment before destroying or respawning
        yield return new WaitForSeconds(0.5f);

        // Trigger game over or respawn logic
        HandlePlayerDeath();
    }

    void CreateSmallExplosion()
    {
        if (smallExplosionPrefab == null) return;

        // Generate random position within the bounds of the player sprite
        Bounds spriteBounds = spriteRenderer.bounds;
        float randomX = Random.Range(-spriteBounds.extents.x * 0.7f, spriteBounds.extents.x * 0.7f);
        float randomY = Random.Range(-spriteBounds.extents.y * 0.7f, spriteBounds.extents.y * 0.7f);

        Vector3 explosionPos = transform.position + new Vector3(randomX, randomY, 0);
        GameObject smallExplosion = Instantiate(smallExplosionPrefab, explosionPos, Quaternion.identity);

        // Randomize the scale of the small explosion
        float randomScale = Random.Range(explosionScaleMin, explosionScaleMax);
        smallExplosion.transform.localScale = new Vector3(randomScale, randomScale, randomScale);

        // Automatically destroy the small explosion after it plays
        Destroy(smallExplosion, 1f);
    }

    void DisablePlayerControls()
    {
        // Disable player movement and shooting components
        if (GetComponent<Rigidbody2D>() != null)
        {
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            GetComponent<Rigidbody2D>().angularVelocity = 0;
        }
        GetComponent<PlayerShoot>().enabled = false;

        // disable player controller scripts
        PlayerMovement.instance.isDead = true; 

        // Disable colliders to avoid further collisions during death
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders)
        {
            collider.enabled = false;
        }
    }

    void HandlePlayerDeath()
    {
        lives--;

        // destroy thruster object
        Destroy(thrusterObject);

        // This method should be implemented according to your game's needs
        // Examples:
        // 1. Show game over screen
        if (lives <= 0)
        {
            // Show game over screen
            GameOverManager.Instance.ShowGameOverPanel();
        }
        else
        {
            // 2. Respawn player
            Debug.Log("respawn player");
            //RespawnPlayer();
        }


        // 3. Subtract lives
        

        Debug.Log("Player is dead - trigger game over or respawn logic");

        // For testing, destroy the game object
        // In a real game, you might want to handle this differently

    }
}