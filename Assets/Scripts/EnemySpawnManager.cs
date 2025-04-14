using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector2 gridSize = new Vector2(8, 5);
    [SerializeField] private float horizontalSpacing = 1.5f;
    [SerializeField] private float verticalSpacing = 1.2f;
    [SerializeField] private Vector2 gridOffset = new Vector2(0, 4);
    [SerializeField] private bool showGizmos = true;

    [Header("Wave Settings")]
    [SerializeField] private float timeBetweenWaves = 5f;
    [SerializeField] private int wavesPerLevel = 5;
    [SerializeField] private float difficultyScalingFactor = 0.1f;
    [SerializeField] private int currentWave = 0;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private GameObject eliteEnemyPrefab;
    [SerializeField] private GameObject[] bossPrefabs;
    [SerializeField] private Transform[] bossMovePoints;

    [Header("Formation Patterns")]
    [SerializeField] private FormationPattern[] formationPatterns;

    private Vector2[,] spawnGrid;
    private List<GameObject> activeEnemies = new List<GameObject>();
    [HideInInspector] public float currentDifficulty = 1f;
    private bool isSpawningWave = false;
    private int currentLevel = 1;
    public static EnemySpawnManager Instance { get; private set; }

    [System.Serializable]
    public class FormationPattern
    {
        public string patternName;
        public int minWaveToAppear = 0;
        public bool[,] grid;
    }
    public enum EnemyType
    {
        drone,
        bomber,
        striker,
        segment,
        elite
    }

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of EnemySpawnManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    public void InitializeGrid()
    {
        int width = (int)gridSize.x;
        int height = (int)gridSize.y;
        spawnGrid = new Vector2[width, height];

        // Calculate positions for each cell in the grid
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xPos = (x - (width - 1) / 2f) * horizontalSpacing + gridOffset.x;
                float yPos = (y - (height - 1) / 2f) * verticalSpacing + gridOffset.y;
                spawnGrid[x, y] = new Vector2(xPos, yPos);
            }
        }
    }

    public IEnumerator WaveController()
    {
        // Wait a moment before first wave
        yield return new WaitForSeconds(2f);

        while (true)
        {
            // Check if we should spawn a new wave
            if (!isSpawningWave && activeEnemies.Count == 0)
            {
                currentWave++;

                // Check if we should spawn a boss
                if (currentWave % wavesPerLevel == 0)
                {
                    StartCoroutine(SpawnBoss());
                }
                else
                {
                    StartCoroutine(SpawnWave());
                }

                // Increase difficulty
                currentDifficulty += difficultyScalingFactor;
                Debug.Log("Current Wave: " + currentWave + ", Difficulty: " + currentDifficulty);

            }

            // Clean up destroyed enemies from our active list
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null)
                    activeEnemies.RemoveAt(i);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator SpawnWave()
    {
        isSpawningWave = true;

        // Choose a formation pattern based on current wave
        FormationPattern pattern = ChooseFormationPattern();

        // If no valid pattern, create a random one
        if (pattern == null)
        {
            pattern = GenerateRandomPattern();
        }

        // Spawn enemies according to pattern
        int width = Mathf.Min((int)gridSize.x, pattern.grid.GetLength(0));
        int height = Mathf.Min((int)gridSize.y, pattern.grid.GetLength(1));

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (pattern.grid[x, y])
                {
                    // Chance to spawn elite based on difficulty
                    bool spawnElite = Random.value < (0.05f * currentDifficulty);

                    GameObject enemyPrefab;
                    if (spawnElite && eliteEnemyPrefab != null)
                    {
                        enemyPrefab = eliteEnemyPrefab;
                    }
                    else
                    {
                        // Choose enemy type based on current wave/difficulty
                        int enemyTypeIndex = Mathf.Min(
                            Mathf.FloorToInt((currentWave - 1) / 2),
                            enemyPrefabs.Length - 1
                        );

                        // Add randomness - sometimes spawn easier or harder enemies
                        enemyTypeIndex += Random.Range(-1, 2);
                        enemyTypeIndex = Mathf.Clamp(enemyTypeIndex, 0, enemyPrefabs.Length - 1);

                        enemyPrefab = enemyPrefabs[enemyTypeIndex];
                    }

                    // Spawn the enemy
                    Vector3 spawnPosition = new Vector3(spawnGrid[x, y].x, spawnGrid[x, y].y + 5f, 0);
                    GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

                    // Add movement component or animation to make enemies fly into position
                    EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
                    if (movement != null)
                    {
                        movement.MoveToPosition(new Vector3(spawnGrid[x, y].x, spawnGrid[x, y].y, 0));
                    }
                    else
                    {
                        // If no movement component, add a simple one
                        StartCoroutine(AnimateEnemyToPosition(enemy, spawnGrid[x, y]));
                    }

                    // Add to active enemies list
                    activeEnemies.Add(enemy);

                    // Configure enemy based on difficulty
                    EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                    if (enemyAI != null)
                    {
                        // Set properties based on current wave/difficulty
                        ConfigureEnemyForWave(enemyAI);
                    }

                    // Small delay between spawns for visual interest
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        isSpawningWave = false;
    }

    public IEnumerator SpawnBoss()
    {
        isSpawningWave = true;

        // Select appropriate boss based on current level
        int bossIndex = Mathf.Min(currentLevel - 1, bossPrefabs.Length - 1);
        GameObject bossPrefab = bossPrefabs[bossIndex];

        // Dramatic pause before boss
        yield return new WaitForSeconds(1.5f);

        // Display "Boss Approaching" message
        // TODO: Add UI reference and show message

        yield return new WaitForSeconds(2f);

        // Spawn boss at top center
        Vector3 spawnPosition = new Vector3(0, 6, 0);
        GameObject boss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);

        // Add to active enemies
        activeEnemies.Add(boss);

        // Configure boss difficulty
        BossAI bossAI = boss.GetComponent<BossAI>();
        if (bossAI != null)
        {
            bossAI.SetDifficulty(currentDifficulty);
            bossAI.SetMovePoints(bossMovePoints);

        }


        // Advance to next level after boss
        currentLevel++;

        isSpawningWave = false;
    }
    public void ResetWaves()
    {
        StopAllCoroutines();
        currentWave = 0;
        currentDifficulty = 1f;
        currentLevel = 1;
        activeEnemies.Clear();
        StartCoroutine(WaveController());
    }


    private FormationPattern ChooseFormationPattern()
    {
        // Filter patterns that are valid for current wave
        List<FormationPattern> validPatterns = new List<FormationPattern>();

        foreach (FormationPattern pattern in formationPatterns)
        {
            if (pattern.minWaveToAppear <= currentWave)
            {
                validPatterns.Add(pattern);
            }
        }

        // If we have valid patterns, pick one at random
        if (validPatterns.Count > 0)
        {
            return validPatterns[Random.Range(0, validPatterns.Count)];
        }

        return null;
    }

    private FormationPattern GenerateRandomPattern()
    {
        FormationPattern pattern = new FormationPattern();
        pattern.patternName = "Random Pattern";
        pattern.grid = new bool[(int)gridSize.x, (int)gridSize.y];

        // Calculate how many enemies to spawn based on difficulty
        int baseEnemyCount = 5;
        int additionalEnemies = Mathf.FloorToInt(currentWave * 1.5f);
        int totalEnemies = Mathf.Min(baseEnemyCount + additionalEnemies, (int)(gridSize.x * gridSize.y * 0.7f));

        // Place enemies randomly
        for (int i = 0; i < totalEnemies; i++)
        {
            int x = Random.Range(0, (int)gridSize.x);
            int y = Random.Range(0, (int)gridSize.y);

            // Try to find an empty cell
            int attempts = 0;
            while (pattern.grid[x, y] && attempts < 10)
            {
                x = Random.Range(0, (int)gridSize.x);
                y = Random.Range(0, (int)gridSize.y);
                attempts++;
            }

            pattern.grid[x, y] = true;
        }

        return pattern;
    }

    private void ConfigureEnemyForWave(EnemyAI enemyAI)
    {
        // Example configurations based on wave number

        // Adjust shooting pattern
        if (currentWave <= 2)
        {
            // Early waves - basic patterns
            enemyAI.shootingPattern = EnemyAI.ShootingPattern.Straight;
        }
        else if (currentWave <= 5)
        {
            // Medium waves - more varied patterns
            int patternIndex = Random.Range(0, 3);
            switch (patternIndex)
            {
                case 0: enemyAI.shootingPattern = EnemyAI.ShootingPattern.Straight; break;
                case 1: enemyAI.shootingPattern = EnemyAI.ShootingPattern.Aimed; break;
                case 2: enemyAI.shootingPattern = EnemyAI.ShootingPattern.Spread; break;
            }
        }
        else
        {
            // Later waves - all patterns possible
            enemyAI.shootingPattern = (EnemyAI.ShootingPattern)Random.Range(0, 6);
        }

        // Adjust attack delay based on difficulty
        float baseDelay = 2.0f;
        float minDelay = 0.5f;
        float adjustedDelay = baseDelay - (currentDifficulty * 0.2f);
        enemyAI.attackDelay = Mathf.Max(minDelay, adjustedDelay);
    }

    private IEnumerator AnimateEnemyToPosition(GameObject enemy, Vector2 targetPosition)
    {
        Vector3 startPosition = enemy.transform.position;
        Vector3 endPosition = new Vector3(targetPosition.x, targetPosition.y, 0);
        float duration = 1.0f;
        float elapsed = 0;

        while (elapsed < duration && enemy != null)
        {
            // Smooth movement with slight bounce at the end
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t); // Smoothstep interpolation

            if (enemy != null)
            {
                enemy.transform.position = Vector3.Lerp(startPosition, endPosition, t);

                // Add slight oscillation for visual interest
                if (t > 0.8f)
                {
                    float oscillation = Mathf.Sin((t - 0.8f) * 50) * 0.05f * (1 - t);
                    enemy.transform.position += new Vector3(oscillation, 0, 0);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is exact
        if (enemy != null)
        {
            enemy.transform.position = endPosition;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || spawnGrid == null)
            return;

        // Draw the spawn grid
        Gizmos.color = Color.yellow;

        for (int x = 0; x < spawnGrid.GetLength(0); x++)
        {
            for (int y = 0; y < spawnGrid.GetLength(1); y++)
            {
                Gizmos.DrawWireSphere(spawnGrid[x, y], 0.2f);
            }
        }

        // Draw grid connections
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

        // Horizontal connections
        for (int x = 0; x < spawnGrid.GetLength(0) - 1; x++)
        {
            for (int y = 0; y < spawnGrid.GetLength(1); y++)
            {
                Gizmos.DrawLine(spawnGrid[x, y], spawnGrid[x + 1, y]);
            }
        }

        // Vertical connections
        for (int x = 0; x < spawnGrid.GetLength(0); x++)
        {
            for (int y = 0; y < spawnGrid.GetLength(1) - 1; y++)
            {
                Gizmos.DrawLine(spawnGrid[x, y], spawnGrid[x, y + 1]);
            }
        }
    }
}