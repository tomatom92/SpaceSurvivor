using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;
    //references
    Animator animator;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb;
    [SerializeField] Transform minBounds;
    [SerializeField] Transform maxBounds;
    //input
    [SerializeField] private PlayerInputSubscription GetInput;
    private Vector2 PlayerMove;
    private float moveDirection;

    //char stats
    public float moveSpeed = 5f;
    public bool isDead;

    //animation
    private string currentAnimation = "";

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        instance = this;
        
    }
    private void Start()
    {
        ChangeAnimation("idle");
    }

    private void Update()
    {
        if(isDead)
            return;
        // Get movement input from the new input system
        PlayerMove = GetInput.MoveInput; // Ensure GetInput is set up correctly
        moveDirection = GetInput.MoveInput.x; // Get the horizontal input value


    }
    public bool IsMovingLeft()
    {
        return moveDirection < 0;
    }

    public bool IsMovingRight()
    {
        return moveDirection > 0;
    }

    private void FixedUpdate()
    {
        // Move player using Rigidbody velocity
        rb.velocity = PlayerMove * moveSpeed;

        // Clamp position within bounds
        float clampedX = Mathf.Clamp(rb.position.x, minBounds.position.x, maxBounds.position.x);
        float clampedY = Mathf.Clamp(rb.position.y, minBounds.position.y, maxBounds.position.y);

        // Apply clamped position
        rb.position = new Vector2(clampedX, clampedY);

        CheckAnimation();
    
    }
    
    private void ChangeAnimation(string animation, float crossfade = 0.2f)
    {
        if (currentAnimation != animation)
        {
            currentAnimation = animation;
            animator.CrossFade(animation, crossfade);
        }
    }
    private void CheckAnimation()
    {
        //if doing animation then dont do the others
        if (currentAnimation == "")
            return;

        if (GetInput.MoveInput.x > 0) //right -->
        {
            ChangeAnimation("right");
        }
        else if (GetInput.MoveInput.x < 0) //left <--
        {
            ChangeAnimation("left");

        }else
        {
            ChangeAnimation("idle");
        }

    }
}
