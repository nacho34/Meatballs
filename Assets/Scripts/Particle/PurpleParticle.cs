using Unity.VisualScripting;
using UnityEngine;

public class PurpleParticle : Particle
{
    [SerializeField] private float explosionForce = 500f;
    [SerializeField] private float bounceForce = 200f;
    [SerializeField] private float explosionRadius = 3f;
    private float lastBouncedPlayerTime = 0f;
    void OnCollisionEnter2D(Collision2D collision)
    {   if(Time.time - lastBouncedPlayerTime > 0.2f){ // prevent multiple bounces in quick succession
            if(collision.gameObject.TryGetComponent(out Player player))
            {
                Vector2 forceDirection = (Vector2)(player.transform.position - transform.position).normalized;
                Rigidbody2D playerRb = collision.rigidbody;
                if (playerRb != null)
                {
                    playerRb.AddForce(forceDirection * bounceForce, ForceMode2D.Impulse);
                    lastBouncedPlayerTime = Time.time; // Update the last bounce time
                }
                return;
            }
        }

        if(collision.gameObject.TryGetComponent(out RedParticle redParticle))
        {
            ApplyExplosionForce();
            redParticle.DestroySelf();
            DestroySelf();
        }
    }
    // applies an explosion force to nearby particles when this particle is destroyed
    void ApplyExplosionForce()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D collider in colliders)
        {
            if (collider.gameObject == this.gameObject) continue;

            if (collider.TryGetComponent(out Particle particle) || collider.TryGetComponent(out Player player))
            {
                Vector2 direction = collider.transform.position - transform.position;
                float distance = direction.magnitude;

                if (distance > 0)
                {
                    direction.Normalize();
                    // Calculate perpendicular direction (90 degrees rotated)
                    Vector2 perpendicularDir = new Vector2(-direction.y, direction.x);
                    
                    // Simple linear falloff based on distance
                    float forceMultiplier = 1f - (distance / explosionRadius);
                    Vector2 force = perpendicularDir * explosionForce * forceMultiplier;

                    // Apply the force as an instant explosion impulse
                    collider.gameObject.GetComponent<Rigidbody2D>().AddForce(force, ForceMode2D.Impulse);
                }
            }
        }
    }
}
