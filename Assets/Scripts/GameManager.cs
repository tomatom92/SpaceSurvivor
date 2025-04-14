using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private EnemySpawnManager enemySpawnManager;
    private PlayerShoot playerShoot;
    private PlayerMovement playerMove;
    
    public Transform[] bossMovePoints;

    private void Start()
    {
        enemySpawnManager = EnemySpawnManager.Instance;
        playerShoot = PlayerShoot.instance;
        playerMove = PlayerMovement.instance;


    }

    private void Update()
    {
        // For testing purposes, you can trigger waves manually
        if (Input.GetKeyDown(KeyCode.E))
        {
            // Initialize the grid and start the wave controller
            enemySpawnManager.InitializeGrid();
            StartCoroutine(enemySpawnManager.WaveController());
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(enemySpawnManager.SpawnBoss());
        }
    }
}
