using BarthaSzabolcs.Tutorial_SpriteFlash;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private int scoreValue = 1000;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float phaseTransitionTime = 2f;
    [SerializeField] private Transform[] movePoints;
    [SerializeField] private bool useCustomMovePoints = false;

    [Header("Attack Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject specialBulletPrefab;
    [SerializeField] private Transform[] firePoints;
    [SerializeField] private Transform specialFirePoint;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float specialBulletSpeed = 10f;
    [SerializeField] private int spreadShotAmount = 4;

    [Header("Phase Settings")]
    [SerializeField] private int totalPhases = 3;
    [SerializeField] private float phaseHealthThreshold = 0.33f; // Percentage of health to trigger new phase


    [Header("effects")]
    [SerializeField] private ColoredFlash flashEffect;
    [SerializeField] private GameObject deathExplosion;



    // Internal variables
    private UpgradeBehaviour upgradeBehaviour; // Reference to the UpgradeBehaviour script

    [SerializeField] private int currentPhase = 1;
    private float difficultyModifier = 1f;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isInvulnerable = false;
    private bool isDead = false;
    private Camera mainCamera;
    private float screenWidth;
    private float screenHeight;
    private Vector3 playerPosition;

    // Attack coroutines
    private Coroutine currentAttackCoroutine;

    void Start()
    {
        upgradeBehaviour = UpgradeBehaviour.instance;
        currentHealth = maxHealth;
        mainCamera = Camera.main;

        // Calculate screen boundaries for movement
        if (mainCamera != null)
        {
            float camDistance = Mathf.Abs(transform.position.z - mainCamera.transform.position.z);
            screenWidth = mainCamera.ViewportToWorldPoint(new Vector3(1, 0, camDistance)).x;
            screenHeight = mainCamera.ViewportToWorldPoint(new Vector3(0, 1, camDistance)).y;
        }

        // If no move points assigned, create default pattern
        if (!useCustomMovePoints || movePoints == null || movePoints.Length == 0)
        {
            GenerateDefaultMovePoints();
        }

        // If no fire points assigned, use the boss position
        if (firePoints == null || firePoints.Length == 0)
        {
            firePoints = new Transform[1];
            firePoints[0] = transform;
        }

        // Start entrance sequence
        StartCoroutine(EntranceSequence());
    }

    void Update()
    {
        if (isDead)
            return;

        // Find player position if available
        if (PlayerMovement.instance != null)
        {
            playerPosition = PlayerMovement.instance.transform.position;
        }

        // Handle movement if we're currently moving
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // Check if reached destination
            if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
            {
                isMoving = false;
            }
        }
    }

    public void SetMovePoints(Transform[] newMovePoints)
    {
        movePoints = newMovePoints;
    }

    public void SetDifficulty(float difficulty)
    {
        difficultyModifier = difficulty;

        // Adjust stats based on difficulty
        maxHealth *= difficultyModifier;
        currentHealth = maxHealth;
        moveSpeed += (difficultyModifier - 1) * 0.5f;
        bulletSpeed += (difficultyModifier - 1) * 3f;
    }

    private void GenerateDefaultMovePoints()
    {
        // Create an array of 5 move points for a simple pattern
        movePoints = new Transform[5];

        for (int i = 0; i < 5; i++)
        {
            GameObject point = new GameObject("MovePoint_" + i);
            point.transform.parent = transform.parent;

            // Set position based on index
            switch (i)
            {
                case 0: // Center top
                    point.transform.position = new Vector3(0, 4, 0);
                    break;
                case 1: // Left top
                    point.transform.position = new Vector3(-screenWidth + 2, 4, 0);
                    break;
                case 2: // Right top
                    point.transform.position = new Vector3(screenWidth - 2, 4, 0);
                    break;
                case 3: // Center screen
                    point.transform.position = new Vector3(0, 2, 0);
                    break;
                case 4: // Back to top
                    point.transform.position = new Vector3(0, 4, 0);
                    break;
            }

            movePoints[i] = point.transform;
        }
    }

    private IEnumerator EntranceSequence()
    {
        // Start off-screen
        transform.position = new Vector3(screenWidth/2, screenHeight + 3, 0);

        // Move to first position
        MoveToPosition(movePoints[0].position);

        // Wait until movement is complete
        while (isMoving)
        {
            yield return null;
        }

        // Start attack patterns
        StartPhase(1);
        Debug.Log("Boss Entrance Complete");
    }

    private void StartPhase(int phase)
    {
        currentPhase = Mathf.Clamp(phase, 1, totalPhases);

        // Stop any current attack pattern
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
        }

        // Start new attack pattern based on phase
        switch (currentPhase)
        {
            case 1:
                currentAttackCoroutine = StartCoroutine(Phase1Attacks());
                break;
            case 2:
                currentAttackCoroutine = StartCoroutine(Phase2Attacks());
                break;
            case 3:
                currentAttackCoroutine = StartCoroutine(Phase3Attacks());
                break;
        }
    }

    private IEnumerator Phase1Attacks()
    {
        while (currentPhase == 1 && !isDead)
        {
            // Simple side-to-side movement with basic attacks
            MoveToPosition(movePoints[1].position);
            while (isMoving) yield return null;

            // Fire spread shot
            FireSpreadShot(spreadShotAmount, 30f);
            yield return new WaitForSeconds(1f);

            MoveToPosition(movePoints[2].position);
            while (isMoving) yield return null;

            // Fire spread shot
            FireSpreadShot(spreadShotAmount, 30f);
            yield return new WaitForSeconds(1f);

            // Occasionally fire aimed shot
            if (Random.value < 0.8f)
            {
                FireAimedShot();
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator Phase2Attacks()
    {
        // Perform phase transition effect
        yield return StartCoroutine(PhaseTransitionEffect());

        while (currentPhase == 2 && !isDead)
        {
            // Move to center
            MoveToPosition(movePoints[3].position);
            while (isMoving) yield return null;

            // Fire spiral pattern
            yield return StartCoroutine(FireSpiralPattern(10, 5f));

            // Move to random side
            MoveToPosition(Random.value < 0.5f ? movePoints[1].position : movePoints[2].position);
            while (isMoving) yield return null;

            // Fire aimed burst at player
            yield return StartCoroutine(FireBurst(5, 0.15f));

            // Ensure the boss keeps moving and attacking
            yield return new WaitForSeconds(0.5f);
            //Debug.Log("currentPhase: " + currentPhase + " isMoving: " + isMoving + " isDead: " + isDead);
        }
    }

    private IEnumerator Phase3Attacks()
    {
        // Perform phase transition effect
        yield return StartCoroutine(PhaseTransitionEffect());

        while (currentPhase == 3 && !isDead)
        {
            // Aggressive attack pattern

            // Random movement
            int randomPoint = Random.Range(0, movePoints.Length);
            MoveToPosition(movePoints[randomPoint].position);

            // Continue attacking while moving
            StartCoroutine(FireSpiralPattern(5, 2f));

            yield return new WaitForSeconds(0.8f);

            // Fire aimed shots more frequently
            FireAimedShot();

            yield return new WaitForSeconds(0.5f);

            // Special attack - cross pattern
            FireCrossShot();

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator PhaseTransitionEffect()
    {
        // Make boss temporarily invulnerable
        isInvulnerable = true;

        // Flash effect
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Color originalColor = renderer.color;

        for (int i = 0; i < 5; i++)
        {
            renderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            renderer.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }

        // Move to center for dramatic effect
        MoveToPosition(new Vector3(0, 4f, 0));
        while (isMoving) yield return null;

        // Explosion effect
        // TODO: Add particle effects here

        yield return new WaitForSeconds(phaseTransitionTime);

        // End invulnerability
        isInvulnerable = false;
    }

    private void MoveToPosition(Vector3 position)
    {
        targetPosition = position;
        isMoving = true;
    }

    // Fire basic bullets in a spread pattern
    private void FireSpreadShot(int bulletCount, float spreadAngle)
    {
        float angleStep = spreadAngle / (bulletCount - 1);
        float startAngle = -spreadAngle / 2;

        for (int i = 0; i < bulletCount; i++)
        {
            foreach (Transform firePoint in firePoints)
            {
                float angle = startAngle + (angleStep * i);
                Vector2 direction = RotateVector(Vector2.down, angle);

                FireBullet(firePoint.position, direction, bulletPrefab, bulletSpeed * 0.8f);
            }
        }
    }

    // Fire a bullet aimed at the player
    private void FireAimedShot()
    {
        foreach (Transform firePoint in firePoints)
        {
            if (PlayerMovement.instance != null)
            {
                Vector2 direction = (playerPosition - firePoint.position).normalized;
                FireBullet(firePoint.position, direction, bulletPrefab, bulletSpeed * 1.2f);
            }
            else
            {
                // Default downward if no player
                FireBullet(firePoint.position, Vector2.down, bulletPrefab, bulletSpeed);
            }
        }
    }

    // Fire a cross-shaped pattern
    private void FireCrossShot()
    {
        Vector2[] directions = new Vector2[]
        {
            Vector2.up,
            Vector2.right,
            Vector2.down,
            Vector2.left
        };

        foreach (Transform firePoint in firePoints)
        {
            foreach (Vector2 dir in directions)
            {
                FireBullet(firePoint.position, dir, specialBulletPrefab, specialBulletSpeed);
            }
        }
    }

    // Fire a burst of bullets at the player
    private IEnumerator FireBurst(int count, float delay)
    {
        for (int i = 0; i < count; i++)
        {
            FireAimedShot();
            yield return new WaitForSeconds(delay);
        }
    }

    // Fire bullets in a spiral pattern
    private IEnumerator FireSpiralPattern(int iterations, float speed)
    {
        float angle = 0;

        for (int i = 0; i < iterations; i++)
        {
            foreach (Transform firePoint in firePoints)
            {
                Vector2 direction = RotateVector(Vector2.down, angle);
                FireBullet(firePoint.position, direction, bulletPrefab, bulletSpeed);
            }

            angle += speed * 10f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Helper method to fire a single bullet
    private void FireBullet(Vector3 position, Vector2 direction, GameObject bulletPrefab, float speed)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.velocity = direction * speed;

            // Rotate bullet to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.AngleAxis(angle + 90f, Vector3.forward);
        }

        // Destroy bullet after a few seconds to prevent build-up
        Destroy(bullet, 3f);
    }

    // Helper method to rotate a vector
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

    // Called when boss takes damage
    public void TakeDamage(float damage)
    {
        
        if (isInvulnerable || isDead)
            return;

        currentHealth -= damage;

        // Flash effect on damage
        if (flashEffect != null)
        {
            flashEffect.Flash(Color.white);
        }
        Debug.Log($"boss took damage current health: {currentHealth}");
        // Check if we should transition to next phase
        float healthPercentage = currentHealth / maxHealth;
        int expectedPhase = totalPhases - Mathf.FloorToInt(healthPercentage / phaseHealthThreshold);
        expectedPhase = Mathf.Clamp(expectedPhase, 1, totalPhases);

        if (expectedPhase > currentPhase)
        {
            StartPhase(expectedPhase);
        }

        // Check if boss is defeated
        if (currentHealth <= 0)
        {
            StartCoroutine(DeathSequence());
        }
    }

    private IEnumerator DeathSequence()
    {
        isDead = true;
        isInvulnerable = true;

        // Stop any current attack coroutine
        if (currentAttackCoroutine != null)
        {
            StopCoroutine(currentAttackCoroutine);
        }

        // Disable collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // death efect
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            // Final explosion effect
            // TODO: Add particle effects and sound here
            for (int i = 0; i < 5; i++)
            {
                flashEffect.Flash(Color.white);
                yield return new WaitForSeconds(0.1f);
            }

            
            for (int i = 0; i < 4; i++)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0);
                GameObject explosion = Instantiate(deathExplosion, transform.position + randomOffset, Quaternion.identity);
                Destroy(explosion, 1f);
            }
            


            // Grant score to player
            ScoreManager scoreManager = ScoreManager.instance;
            if (scoreManager != null)
            {
                scoreManager.AddScore(scoreValue);
            }

            upgradeBehaviour.ShowUpgradesAfterBossDefeat();




            // Destroy boss
            Destroy(gameObject,1f);
            
        }
        
    }

    // Optional: Draw gizmos for move points in editor
    private void OnDrawGizmosSelected()
    {
        if (movePoints != null)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < movePoints.Length; i++)
            {
                if (movePoints[i] != null)
                {
                    Gizmos.DrawWireSphere(movePoints[i].position, 0.3f);

                    // Draw lines between points
                    if (i < movePoints.Length - 1 && movePoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(movePoints[i].position, movePoints[i + 1].position);
                    }
                }
            }
        }
    }
}