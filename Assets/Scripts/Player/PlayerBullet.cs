using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] public float damage = 1;
    Animator animator;
    Rigidbody2D rb;
    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Player":
                return;
            case "Enemy":
                collision.GetComponent<EnemyHealth>().TakeDamage(damage);
                Destroy(gameObject);
                break;
            case "Boss":
                collision.GetComponent<BossAI>().TakeDamage(damage);
                animator.Play("Explode");
                rb.velocity = Vector2.zero; // Stop the bullet's movement
                Destroy(gameObject, 0.5f);
                break;
        }
    }
}
