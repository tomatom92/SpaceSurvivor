using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyGridManager : MonoBehaviour
{
    [Header("Grid Configuration")]
    public int columns = 8;
    public int rows = 5;
    public float horizontalSpacing = 1.5f;
    public float verticalSpacing = 1.5f;
    public Color gridColor = Color.yellow;

    [Header("Padding Configuration")]
    public float horizontalPadding = 0.5f;
    public float verticalPadding = 0.5f;

    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Animation Configuration")]
    public float animationDuration = 2.0f;

    // 2D array to store enemy references
    private GameObject[,] enemyGrid;

    public static EnemyGridManager instance;

    private void OnDrawGizmos()
    {
        VisualizeGrid();
    }

    private void VisualizeGrid()
    {
        Vector2 startPosition = CalculateGridStartPosition();
        Gizmos.color = gridColor;

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                Vector2 cellPosition = startPosition + new Vector2(
                    col * (horizontalSpacing + horizontalPadding),
                    -row * (verticalSpacing + verticalPadding)
                );

                Gizmos.DrawWireCube(cellPosition, new Vector2(
                    horizontalSpacing * 0.8f,
                    verticalSpacing * 0.8f
                ));
            }
        }
    }

    private Vector2 CalculateGridStartPosition()
    {
        float gridWidth = (columns - 1) * (horizontalSpacing + horizontalPadding);
        float gridHeight = (rows - 1) * (verticalSpacing + verticalPadding);

        return (Vector2)transform.position - new Vector2(
            gridWidth / 2,
            -gridHeight / 2
        );
    }

    private Vector2 CalculateOffScreenStartPosition()
    {
        // Calculate an off-screen position above the top of the screen
        Vector2 screenTopCenter = new Vector2(Screen.width / 2, Screen.height);
        Vector2 worldTopCenter = Camera.main.ScreenToWorldPoint(screenTopCenter);
        return new Vector2(worldTopCenter.x, worldTopCenter.y + 2.0f); // Adjust the offset as needed
    }

    public GameObject CreateEnemy(int col, int row, GameObject enemyPrefab = null)
    {
        if (!IsValidGridPosition(col, row))
        {
            Debug.LogWarning($"Invalid grid position: ({col}, {row})");
            return null;
        }

        if (enemyGrid[col, row] != null)
        {
            Debug.LogWarning($"Grid position ({col}, {row}) is already occupied");
            return null;
        }

        if (enemyPrefab == null)
        {
            if (enemyPrefabs.Length == 0)
            {
                Debug.LogError("No enemy prefabs assigned!");
                return null;
            }
            enemyPrefab = enemyPrefabs[0];
        }

        Vector2 offScreenPosition = CalculateOffScreenStartPosition();
        GameObject enemyInstance = Instantiate(
            enemyPrefab,
            offScreenPosition,
            Quaternion.identity,
            transform
        );

        enemyGrid[col, row] = enemyInstance;

        return enemyInstance;
    }

    public void SpawnWave(int[] enemyTypesPerRow)
    {
        if (enemyTypesPerRow.Length != rows)
        {
            Debug.LogError("Enemy types array must match the number of rows");
            return;
        }

        for (int row = 0; row < rows; row++)
        {
            int prefabIndex = Mathf.Clamp(enemyTypesPerRow[row], 0, enemyPrefabs.Length - 1);
            GameObject prefab = enemyPrefabs[prefabIndex];

            for (int col = 0; col < columns; col++)
            {
                CreateEnemy(col, row, prefab);
            }
        }
    }

    public void RemoveEnemy(int col, int row)
    {
        if (IsValidGridPosition(col, row))
        {
            if (enemyGrid[col, row] != null)
            {
                Destroy(enemyGrid[col, row]);
                enemyGrid[col, row] = null;
            }
        }
    }

    private bool IsValidGridPosition(int col, int row)
    {
        return col >= 0 && col < columns && row >= 0 && row < rows;
    }

    public List<GameObject> GetActiveEnemies()
    {
        List<GameObject> activeEnemies = new List<GameObject>();

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (enemyGrid[col, row] != null)
                {
                    activeEnemies.Add(enemyGrid[col, row]);
                }
            }
        }

        return activeEnemies;
    }

    private void Awake()
    {
        instance = this;
    }
    public void InitializeGrid(int columns, int rows)
    {
        this.columns = columns;
        this.rows = rows;
        enemyGrid = new GameObject[columns, rows];
    }

    void Start()
    {
        
    }

    public void AnimateEnemiesToGrid()
    {
        StartCoroutine(AnimateEnemiesToGridCoroutine());
    }

    private IEnumerator AnimateEnemiesToGridCoroutine()
    {
        Vector2 startPosition = CalculateGridStartPosition();

        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows; row++)
            {
                if (enemyGrid[col, row] != null)
                {
                    Vector2 targetPosition = startPosition + new Vector2(
                        col * (horizontalSpacing + horizontalPadding),
                        -row * (verticalSpacing + verticalPadding)
                    );

                    StartCoroutine(MoveEnemyToPosition(enemyGrid[col, row], targetPosition));
                }
            }
        }

        yield return new WaitForSeconds(animationDuration);
    }

    private IEnumerator MoveEnemyToPosition(GameObject enemy, Vector2 targetPosition)
    {
        Vector2 startPosition = enemy.transform.position;
        float elapsedTime = 0;

        while (elapsedTime < animationDuration)
        {
            enemy.transform.position = Vector2.Lerp(startPosition, targetPosition, elapsedTime / animationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        enemy.transform.position = targetPosition;
    }
}
