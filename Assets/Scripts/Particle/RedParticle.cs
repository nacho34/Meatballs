using UnityEngine;

public class RedParticle : Particle
{
    [Header("Chase Settings")]
    [SerializeField] private float chaseRange = 5f;
    [SerializeField] private float chaseStrength = 20f;
    [SerializeField] private float maxChaseSpeed = 10f;

    [Header("Idle Movement")]
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float forceToApply = 13f;

    void OnCollisionEnter2D(Collision2D collision)
    {    
        if (collision.gameObject.TryGetComponent(out GreenParticle greenParticle))
        {
            greenParticle.DestroySelf();
        }
    }

    void FixedUpdate()
    {
        if (Player.Instance == null)
            return;

        Vector2 playerPosition = Player.Instance.transform.position;
        Vector2 toPlayer = playerPosition - rb.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer <= chaseRange)
        {
            Vector2 desiredVelocity = toPlayer.normalized * maxChaseSpeed;
            Vector2 steering = desiredVelocity - rb.linearVelocity;
            rb.AddForce(steering * chaseStrength * Time.fixedDeltaTime);
        }
        else if (rb.linearVelocity.magnitude < minSpeed)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDirection * forceToApply);
        }
    }
}
