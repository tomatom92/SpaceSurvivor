using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float wobbleAmount = 0.2f;
    [SerializeField] private float wobbleSpeed = 2f;

    private Vector3 targetPosition;
    private Vector3 initialPosition;
    private bool isMovingToPosition = false;
    private float wobbleOffset;

    private void Start()
    {
        // Generate a random wobble offset so enemies don't all wobble in sync
        wobbleOffset = Random.Range(0f, 2f * Mathf.PI);
        initialPosition = transform.position;
        targetPosition = initialPosition;
    }

    private void Update()
    {
        if (isMovingToPosition)
        {
            // Move towards target position
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // Check if we've reached the target
            if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isMovingToPosition = false;
                initialPosition = transform.position;
            }
        }
        else
        {
            // Apply wobble effect when in position
            float wobbleX = Mathf.Sin(Time.time * wobbleSpeed + wobbleOffset) * wobbleAmount;
            transform.position = initialPosition + new Vector3(wobbleX, 0, 0);
        }
    }

    public void MoveToPosition(Vector3 position)
    {
        targetPosition = position;
        isMovingToPosition = true;
    }

    // Can be called to make enemies move in formation
    public void MoveFormationBy(Vector3 delta)
    {
        initialPosition += delta;
        targetPosition += delta;
    }
}