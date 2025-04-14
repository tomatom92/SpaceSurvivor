using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [HideInInspector] public Animator animator;
    [HideInInspector] public SpriteRenderer sr;

    [SerializeField] protected Sprite bullet;
    [SerializeField] protected float health;
    [SerializeField] protected float damage;
    [SerializeField] protected float speed;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }
}
