using UnityEngine;

public class BlueParticle : Particle
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out BrownParticle brownParticle))
        {
            brownParticle.DestroySelf();
        }
        else if(collision.gameObject.TryGetComponent(out GreenParticle greenParticle))
        {
            DestroySelf();
            ParticleManager.Instance.GreenSpawner.SpawnParticle(transform.position);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
           DestroySelf();
        }
    }
}