using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("UI References")]
    [SerializeField] private FloatingJoystick joystick;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Auto-find joystick if not assigned
        if (joystick == null)
        {
            joystick = FindObjectOfType<FloatingJoystick>();
            if (joystick == null)
            {
                Debug.LogError("FloatingJoystick not found! Please assign it in the inspector.");
            }
        }
    }

    void Update()
    {
        HandleMovementInput();
        UpdateAnimation();
    }

    void HandleMovementInput()
    {
        // Get input from joystick
        if (joystick != null)
        {
            moveInput = joystick.Direction;
        }
        else
        {
            moveInput = Vector2.zero;
        }
        
        // Apply movement
        rb.velocity = moveInput * moveSpeed;
    }

    void UpdateAnimation()
    {
        // Determine if player is moving
        bool isWalking = rb.velocity.magnitude > 0.1f;
        
        // Update animator
        animator.SetBool("IsWalking", isWalking);

        if (isWalking)
        {
            // Update current movement direction
            animator.SetFloat("InputX", moveInput.x);
            animator.SetFloat("InputY", moveInput.y);
        }
        else
        {
            // Update last movement direction (for idle facing)
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
    }
}