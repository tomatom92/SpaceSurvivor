using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject[] enemies; // Array of enemy prefabs for this wave
        public int count;            // Number of enemies to spawn
        public float spawnInterval;  // Time between each enemy spawn
    }

    [Header("Wave Settings")]
    public Wave[] waves;             // Array of waves
    public Transform[] spawnPoints;  // Locations where enemies can spawn

    [Header("Post-Wave Action")]
    public GameObject doorToOpen;    // Example: A door to open after all waves are defeated

    private int currentWaveIndex = 0;
    private int enemiesRemaining;    // Number of enemies still alive in the current wave

    void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            currentWaveIndex = i;
            enemiesRemaining = waves[i].count;

            // Spawn enemies for the current wave
            for (int j = 0; j < waves[i].count; j++)
            {
                //SpawnEnemy(waves[i].enemies[Random.Range(0, waves[i].enemies.Length)]);
                yield return new WaitForSeconds(waves[i].spawnInterval);
            }

            // Wait until all enemies in the current wave are defeated
            yield return new WaitUntil(() => enemiesRemaining <= 0);
            Debug.Log($"Wave {i + 1} defeated!");
        }

        // All waves are defeated
        Debug.Log("All waves defeated!");
        OnAllWavesDefeated();
    }

    void OnAllWavesDefeated()
    {
        // Trigger post-wave action (e.g., open a door)
        if (doorToOpen != null)
        {
            doorToOpen.SetActive(false); // Example: Disable the door to "open" it
            Debug.Log("Door opened!");
        }

        // You can add more actions here, like spawning a boss or ending the level
    }
}
