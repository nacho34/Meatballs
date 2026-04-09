using UnityEngine;

public class OceanParticle : Particle
{
    public GameObject greenParticlePrefab;
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent(out BrownParticle brownParticle))
        {
            brownParticle.DestroySelf();
        }
        else if(collision.gameObject.TryGetComponent(out GreenParticle greenParticle))
        {
            if (greenParticle.ReadyToTransformBlues())
            {
                Vector3 position = transform.position;
                Destroy(this.gameObject);
                GameObject newGreenParticle = Instantiate(greenParticlePrefab, position, Quaternion.identity);
                newGreenParticle.GetComponent<GreenParticle>().SetCreationTime(Time.time);
            }
        }
    }
}