using UnityEngine;

public class AsteroidManager : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Transform[] spawnPoints; // Array of spawn points
    [SerializeField] private float minRotationSpeed = 10f;
    [SerializeField] private float maxRotationSpeed = 50f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnAsteroid), spawnInterval, spawnInterval);
    }

    private void SpawnAsteroid()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points set for AsteroidManager.");
            return;
        }

        // Select a random spawn point from the array
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject asteroid = Instantiate(asteroidPrefab, spawnPoint.position, Quaternion.identity);

        // Optionally, you can set the initial direction and speed of the asteroid here
        AsteroidEnemy asteroidEnemy = asteroid.GetComponent<AsteroidEnemy>();
        if (asteroidEnemy != null)
        {
            asteroidEnemy.InitializeMovementDirection();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta ;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, 0.5f); // Draw a small sphere at each spawn point
            }
        }
    }
}
