using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{
    public static Player Instance; 
    Rigidbody2D m_Rigidbody;
    private PlayerInput playerInput;
    public Camera mainCamera;
    private Collider2D playerCollider;
    public float jumpAcceleration = 1000f; // Acceleration value
    public float walkAcceleration = 100f; // Acceleration value for horizontal movement
    public float sideDeceleration = 150f; // Deceleration value for horizontal movement when no input is present

    // Velocity limits and deceleration
    public float maxWalkingSpeed = 50f;
    public float maxJumpSpeed = 200f;
    public float maxAbsoluteVelocity = 500f;
    public float instantSideDecelerationThreshold = 0.1f;

    // Jump cooldown settings
    public float jumpAccelerationWindow = 0.5f; // seconds
    private float jumpTime = 0f;
    private bool jumpStarted = false;
    private bool canJump = false;

    public Transform joint;
    public Bat bat;

    private float MaxElevation = 0;
    public TextMeshProUGUI MaxElevationText;
    public TextMeshProUGUI CurrentElevationText;

    // Flag to prevent overlapping attack animations
    public bool isAttacking = false;
    // Brown particle clinging mechanics
    public bool isClinging = false;
    private HashSet<BrownParticle> clingingParticles = new HashSet<BrownParticle>();
    private Vector2 clingDirection;
    private float clingRadius => playerCollider.bounds.extents.y + 0.1f;    [SerializeField] private float clingStrength = 100f;    public GameObject WallPrefab;
    public GameObject BackgroundPrefab;
    private float LastSpawnedWallElevation = 20f;
    public float LeftWallX;
    public float RightWallX;
    public float BackgroundX;
    private InputAction _move;
    private InputAction _look;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
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

        // Detect and handle clinging to brown particles
        clingingParticles.Clear();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, clingRadius);
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out BrownParticle brown))
            {
                clingingParticles.Add(brown);
            }
        }

        // Calculate clinging state and direction
        if (clingingParticles.Count > 0)
        {
            Vector2 sum = Vector2.zero;
            foreach (var brown in clingingParticles)
            {
                sum += ((Vector2)(brown.transform.position - transform.position)).normalized;
            }
            clingDirection = sum / clingingParticles.Count;
            isClinging = true;
            // Apply fixed clinging force towards the average direction
            m_Rigidbody.AddForce(clingDirection * clingStrength);
        }
        else
        {
            isClinging = false;
        }

        // Observe upward acceleration request.
        bool hasVerticalMovement = moveInput.y > 0f;
        bool grounded = isGrounded();
        if (grounded)
        {
            // Reset jump state when grounded.
            jumpStarted = false;
            jumpTime = 0f;
        }

        // Start jump window when upward acceleration starts being positive.
        if (hasVerticalMovement && !jumpStarted && grounded)
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
        else{
            // If jump hasn't started, ignore any upward acceleration input.
            moveInput.y = 0f;
        }

        // Gradual sideways deceleration when no horizontal input.
        if (Mathf.Abs(moveInput.x) <= instantSideDecelerationThreshold)
        {
            moveInput.x = 0f;
            float newSidewaysSpeed = Mathf.MoveTowards(m_Rigidbody.linearVelocity.x, 0f, sideDeceleration * Time.fixedDeltaTime);
            m_Rigidbody.linearVelocity = new Vector2(newSidewaysSpeed, m_Rigidbody.linearVelocity.y);
        }

        Vector2 currentVelocity = m_Rigidbody.linearVelocity;

        if (moveInput.x != 0f && Mathf.Abs(currentVelocity.x) >= maxWalkingSpeed && Mathf.Sign(moveInput.x) == Mathf.Sign(currentVelocity.x))
        {
            moveInput.x = 0f;
        }

        if (moveInput.y > 0f && currentVelocity.y >= maxJumpSpeed)
        {
            moveInput.y = 0f;
        }

        Vector2 horizontalForce = new Vector2(moveInput.x, 0f) * m_Rigidbody.mass * walkAcceleration * Time.fixedDeltaTime;
        Vector2 verticalForce = Vector2.zero;
        if (isClinging && moveInput.y > 0f)
        {
            isClinging = false;
            // Revert fixed clinging force towards the average direction
            m_Rigidbody.AddForce(-clingDirection * clingStrength);
            verticalForce = -clingDirection * moveInput.y * m_Rigidbody.mass * jumpAcceleration * Time.fixedDeltaTime;
        } else {
            verticalForce = new Vector2(0f, moveInput.y) * m_Rigidbody.mass * jumpAcceleration * Time.fixedDeltaTime;
        }

        m_Rigidbody.AddForce(horizontalForce, ForceMode2D.Impulse);
        m_Rigidbody.AddForce(verticalForce, ForceMode2D.Impulse);

        // Enforce maximum velocity limits.
        Vector2 clampedVelocity = m_Rigidbody.linearVelocity;
        if (clampedVelocity.magnitude > maxAbsoluteVelocity)
        {
            clampedVelocity = clampedVelocity.normalized * maxAbsoluteVelocity;
        }
        m_Rigidbody.linearVelocity = clampedVelocity;
        // Keep score on the UI 
        UpdateElevation();
        SpawnNewWalls();
    }

    bool isGrounded()
    {
        if (isClinging) return true;
        Vector2 origin = playerCollider.bounds.center;
        float rayDistance = playerCollider.bounds.extents.y + 0.1f;
        int groundMask = LayerMask.GetMask("Ground", "BrownBalls", "BlueBalls", "RedBalls", "GreenBalls", "PurpleBalls");

        Vector2[] rayDirections = new Vector2[]
        {
            Vector2.down,
            Quaternion.Euler(0f, 0f, -22.5f) * Vector2.down,
            Quaternion.Euler(0f, 0f, 22.5f) * Vector2.down,
            Quaternion.Euler(0f, 0f, -45f) * Vector2.down,
            Quaternion.Euler(0f, 0f, 45f) * Vector2.down
        };

        foreach (Vector2 rayDirection in rayDirections)
        {
            if (Physics2D.Raycast(origin, rayDirection, rayDistance, groundMask))
            {
                return true;
            }
        }

        return false;
    }

    // Called when the attack input is triggered
    void OnAttack(InputValue value)
    {
        if (!isAttacking)
        {
            Vector2 mousePos = _look.ReadValue<Vector2>();
            Vector3 worldMousePos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
            Vector2 cursorToPlayerVector = worldMousePos - transform.position;
            bat.SendDirection = cursorToPlayerVector.normalized; // Set the direction for the bat to send the ball
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

    void UpdateElevation()
    {
        float elevation = transform.position.y;
        if (elevation > MaxElevation)
        {
            MaxElevation = elevation;
            MaxElevationText.text = $"Max Elevation: {MaxElevation:F2}";
        }
        CurrentElevationText.text = $"Current Elevation: {elevation:F2}";
    }

    void SpawnNewWalls()
    {
        // if player is at the last spawned wall elevation, spawn new walls 50 units above
        if (transform.position.y > LastSpawnedWallElevation)
        {
            Debug.Log("Spawning new walls at elevation: " + (LastSpawnedWallElevation + 50f));
            Instantiate(WallPrefab, new Vector3(LeftWallX, LastSpawnedWallElevation + 50, 0), Quaternion.identity);
            Instantiate(WallPrefab, new Vector3(RightWallX, LastSpawnedWallElevation + 50, 0), Quaternion.identity);
            Instantiate(BackgroundPrefab, new Vector3(BackgroundX, LastSpawnedWallElevation + 50, 0), Quaternion.identity);
            LastSpawnedWallElevation += 50f;
        }
    }
}
