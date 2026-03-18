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

        if (joystick == null)
        {
            joystick = FindObjectOfType<FloatingJoystick>();
        }

        animator.SetFloat("LastInputX", 0f);
        animator.SetFloat("LastInputY", -1f);
    }

    void Update()
    {
        if (PauseController.IsGamePaused)
        {
            moveInput = Vector2.zero;
        }
        else
        {
            moveInput = joystick != null ? joystick.Direction : Vector2.zero;
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        // Physics calculations should always happen in FixedUpdate
        rb.velocity = moveInput * moveSpeed;
    }

    void UpdateAnimation()
    {
        // Check movement based on input magnitude for better accuracy in animations
        bool isWalking = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool("IsWalking", isWalking);

        if (isWalking)
        {
            animator.SetFloat("InputX", moveInput.x);
            animator.SetFloat("InputY", moveInput.y);
            
            // Store the direction for idle states
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
    }
}