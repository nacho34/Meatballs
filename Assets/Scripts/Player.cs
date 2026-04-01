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
    public Camera mainCamera;
    private Collider2D playerCollider;
    public float acceleration = 1000f; // Acceleration value

    // Velocity limits and deceleration
    public float maxSidewaysSpeed = 50f;
    public float maxJumpSpeed = 200f;
    public float instantSideDecelerationThreshold = 0.1f;

    // Jump cooldown settings
    public float jumpAccelerationWindow = 0.5f; // seconds
    private float jumpTime = 0f;
    private bool jumpStarted = false;

    public Transform joint;
    public Bat bat;

    // Flag to prevent overlapping attack animations
    public bool isAttacking = false;

    private InputAction _move;
    private InputAction _look;

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
        playerCollider = GetComponent<Collider2D>();
        _move = playerInput.actions["Move"];
        _look = playerInput.actions["Look"];
    }

    void FixedUpdate()
    {
        // Update bat position
        if (!isAttacking)
        {
            Vector2 mousePos = _look.ReadValue<Vector2>();
            Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
            Vector2 cursorToPlayerVector = worldMousePos - transform.position;
            float angle = Mathf.Atan2(cursorToPlayerVector.y, cursorToPlayerVector.x) * Mathf.Rad2Deg;
            joint.rotation = Quaternion.Euler(0, 0, angle - 45); // Rotate bat to face cursor
        }
        
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
        return Physics2D.Raycast(transform.position, Vector2.down, playerCollider.bounds.size.y/2 + 0.1f, LayerMask.GetMask("Ground", "BrownBalls", "BlueBalls", "RedBalls", "GreenBalls", "PurpleBalls"));
    }

    // Called when the attack input is triggered
    void OnAttack(InputValue value)
    {
        if (!isAttacking)
        {
            isAttacking = true;
            bat.ResetHitList(); // Reset hit list for new swing
            StartCoroutine(AttackAnimation());
        }
    }

    // Coroutine to animate the bat swing: rotate 90 degrees and back over 0.7 seconds
    IEnumerator AttackAnimation()
    {
        float duration = 0.2f; // Half the total animation time for each phase

        // Phase 1: Rotate bat 90 degrees forward
        Quaternion startRotation = joint.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(0, 0, 90);
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            joint.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);
            yield return null;
        }
        joint.rotation = targetRotation;

        // Phase 2: Rotate bat back to original position
        startRotation = joint.rotation;
        targetRotation = startRotation * Quaternion.Euler(0, 0, -90);
        time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            joint.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);
            yield return null;
        }
        joint.rotation = targetRotation;

        isAttacking = false; // Allow new attacks after animation completes
    }
}
