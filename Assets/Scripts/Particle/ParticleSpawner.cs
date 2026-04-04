using System;
using System.Collections.Generic;
using UnityEngine;

// Each particle spawner managees a specific type of particle, handling their spawning/despawning & more
public abstract class ParticleSpawner : MonoBehaviour
{
    [SerializeField] protected GameObject particlePrefab;
    protected Particle particleType;
    protected List<Particle> particles = new List<Particle>();
    [SerializeField] protected float spawnInterval = 1f;
    [SerializeField] protected int maxParticles = 25;
    [SerializeField] protected float activeElevation = -10f;
    protected float lastSpawnTime = 0f; // keep track of this for spawn intervals
    void Start(){
        particleType = particlePrefab.GetComponent<Particle>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(Player.Instance.transform.position.y > activeElevation){
            HandleSpawning();
        }
    }
    protected void HandleSpawning()
    {
        // spawn a particle each time interval seconds have passed
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            // If we've reached max particles, remove the farthest one before spawning a new one
            if(particles.Count >= maxParticles){
                Particle farthestParticle = null;
                foreach(Particle p in particles){
                    float verticalDist = Math.Abs(Player.Instance.transform.position.y - p.transform.position.y);
                    if(farthestParticle == null || verticalDist > Math.Abs(Player.Instance.transform.position.y - farthestParticle.transform.position.y)){
                        farthestParticle = p;
                    }
                }
                RemoveParticle(farthestParticle);
            }
            // only update last spawn time if we successfully spawned
            if(SpawnParticles()){
                lastSpawnTime = Time.time;
            }
        }
    }

    protected virtual void FixedUpdate()
    {
        if(particleType.AttractionStrength > 0f){
            ApplyAttractionForces();
        }
    }

    // Applies mutual attraction forces between all particles of a shared type
    private void ApplyAttractionForces()
    {
        for (int i = 0; i < particles.Count; i++)
        {
            Particle p1 = particles[i];
            
            // Check if particle was destroyed unexpectedly
            if (p1 == null) continue;

            for (int j = i + 1; j < particles.Count; j++)
            {
                Particle p2 = particles[j];
                
                if (p2 == null) continue;

                Vector2 direction = p2.transform.position - p1.transform.position;
                float distance = direction.magnitude;

                // Using p1's properties as particles of the same spawner share the same type
                if (distance > 0f && distance <= p1.AttractionRange)
                {
                    Vector2 force = direction.normalized * p1.AttractionStrength;
                    
                    p1.ApplyAttractionForce(force);
                    p2.ApplyAttractionForce(-force);
                }
            }
        }
    }

    // returns true if successful, overriden because each spawner generates particles differently
    protected abstract bool SpawnParticles(); 
    public void SpawnParticle(Vector2 position)
    {
        GameObject newParticle = Instantiate(particlePrefab, position, Quaternion.identity);
        Particle p = newParticle.GetComponent<Particle>();
        p.spawner = this;
        particles.Add(p);
    }
    public void RemoveParticle(Particle particle)
    {
        particles.Remove(particle);
        Destroy(particle.gameObject);
    }

    protected bool IsPositionValid(Vector2 position)
    {
        // First check boundaries using ParticleManager
        if (position.x < ParticleManager.Instance.WidthBounds.x || position.x > ParticleManager.Instance.WidthBounds.y ||
            position.y < ParticleManager.Instance.HeightBounds.x || position.y > ParticleManager.Instance.HeightBounds.y)
        {
            return false;
        }

        // Second check overlapping colliders using Unity Physics2D
        // using scale to check radius/half width because we assume all particles are from shape primitives
        Collider2D overlap = Physics2D.OverlapCircle(position, particleType.transform.localScale.x * 0.4f);
        return overlap == null;
    }
}
