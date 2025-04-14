using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] public float damage = 1;

    // Update is called once per frame


    private void OnTriggerEnter2D(Collider2D collision)
    {
        switch (collision.tag)
        {
            case "Player":
                collision.GetComponent<PlayerHealth>().TakeDamage(damage);
                Destroy(gameObject);
                break;
            case "Enemy":
                return;
            case "Boss":
                return;

        }
    }
}
