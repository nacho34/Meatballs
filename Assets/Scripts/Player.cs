using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(SpriteRenderer))]
public class Player : MonoBehaviour
{
    Rigidbody2D m_Rigidbody;
    private PlayerInput playerInput;
    private SpriteRenderer spriteRenderer;
    public float acceleration = 1000f; // Acceleration value

    // Velocity limits and deceleration
    public float maxSidewaysSpeed = 50f;
    public float maxJumpSpeed = 200f;
    public float instantSideDecelerationThreshold = 0.1f;

    // Jump cooldown settings
    public float jumpAccelerationWindow = 0.5f; // seconds
    private float jumpTime = 0f;
    private bool jumpStarted = false;

    public Transform bat;

    // Flag to prevent overlapping attack animations
    private bool isAttacking = false;

    private InputAction _move;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        _move = playerInput.actions["Move"];
    }

    void FixedUpdate()
    {
        Vector2 moveInput = _move.ReadValue<Vector2>();

        // Observe upward acceleration request.
        bool hasVerticalMovement = moveInput.y > 0f;

        if (isGrounded())
        {
            // Reset jump state when grounded.
            jumpStarted = false;
            jumpTime = 0f;
        }

        // Start jump window when upward acceleration starts being positive.
        if (hasVerticalMovement && !jumpStarted)
        {
            jumpStarted = true;
            jumpTime = 0f;
        }

        if (jumpStarted)
        {
            jumpTime += Time.fixedDeltaTime;

            if (jumpTime > jumpAccelerationWindow)
            {
                // Disable any further positive upward acceleration after the window.
                if (moveInput.y > 0f)
                {
                    moveInput.y = 0f;
                }
            }
        }

        // Instant sideways deceleration when no horizontal input.
        if (Mathf.Abs(moveInput.x) <= instantSideDecelerationThreshold)
        {
            moveInput.x = 0f;
            m_Rigidbody.linearVelocity = new Vector2(0f, m_Rigidbody.linearVelocity.y);
        }

        m_Rigidbody.AddForce(moveInput * m_Rigidbody.mass * acceleration * Time.fixedDeltaTime, ForceMode2D.Impulse);

        // Enforce maximum velocity limits.
        Vector2 clampedVelocity = m_Rigidbody.linearVelocity;
        clampedVelocity.x = Mathf.Clamp(clampedVelocity.x, -maxSidewaysSpeed, maxSidewaysSpeed);
        clampedVelocity.y = Mathf.Min(clampedVelocity.y, maxJumpSpeed);
        m_Rigidbody.linearVelocity = clampedVelocity;
    }

    bool isGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, spriteRenderer.bounds.size.y/2, LayerMask.GetMask("Water"));
    }

    // Called when the attack input is triggered
    void OnAttack(InputValue value)
    {
        if (!isAttacking)
        {
            isAttacking = true;
            StartCoroutine(AttackAnimation());
        }
    }

    // Coroutine to animate the bat swing: rotate 90 degrees and back over 0.7 seconds
    IEnumerator AttackAnimation()
    {
        float duration = 0.35f; // Half the total animation time for each phase

        // Phase 1: Rotate bat 90 degrees forward
        Quaternion startRotation = bat.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, 90);
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            bat.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);
            yield return null;
        }
        bat.rotation = targetRotation;

        // Phase 2: Rotate bat back to original position
        startRotation = bat.rotation;
        targetRotation = startRotation * Quaternion.Euler(0, 0, -90);
        time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            bat.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);
            yield return null;
        }
        bat.rotation = targetRotation;

        isAttacking = false; // Allow new attacks after animation completes
    }
}
