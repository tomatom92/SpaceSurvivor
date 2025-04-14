using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float maxHealth = 1f;
    EnemyAI enemyAI;
    float currentHealth;

    Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyAI = GetComponent<EnemyAI>();
    }
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        //Debug.Log("damaged");
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void Die()
    {
        //Debug.Log("dead");
        //GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        enemyAI.enabled = false;
        animator.Play("enemyDeath");
        ScoreManager.instance.AddScore(100);


        Destroy(gameObject, 0.6f);

    }

}
