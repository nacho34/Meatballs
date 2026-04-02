using UnityEngine;

public class RedParticle : Particle
{
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private float forceToApply = 13f;
    void OnCollisionEnter2D(Collision2D collision)
    {    
        if(collision.gameObject.TryGetComponent(out GreenParticle greenParticle))
        {
            greenParticle.DestroySelf();
        }
    }
    void Update()
    {
        if( rb.linearVelocity.magnitude < minSpeed)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            rb.AddForce(randomDirection * forceToApply);
        }
    }
}
