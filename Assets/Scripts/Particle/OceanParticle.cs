using UnityEngine;

public class OceanParticle : Particle
{
    public GameObject greenParticlePrefab;
    private Rigidbody2D rb;
    private Vector2 lastVelocity;

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
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        lastVelocity = rb.linearVelocity;
    }

    void FixedUpdate()
    {
        Vector2 accel = (rb.linearVelocity - lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = rb.linearVelocity;

        float intensity = accel.magnitude + rb.linearVelocity.magnitude;

        OceanAudioManager.Instance?.AddEnergy(intensity);
    }
}