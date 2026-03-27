using UnityEngine;

public class BrownSpawner : ParticleSpawner
{
    [SerializeField] private float disconnectedSpawnChance = 0.5f;
    private int maxSpawnAttempts = 10;

    protected override bool SpawnParticles()
    {
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            Vector2 spawnPos = Vector2.zero;
            bool validPositionFound = false;
            bool doDisconnectedSpawn = true;
            // if there are no existing particles, we have to do a disconnected spawn
            if(particles.Count > 0)
            {
                // roll to decide if we should spawn the new particle adjacent
                float roll = Random.value;
                Debug.Log($"Spawn roll: {roll}, threshold: {disconnectedSpawnChance}");
                doDisconnectedSpawn = roll < disconnectedSpawnChance;
            }

            if (!doDisconnectedSpawn)
            {
                // Try connected spawn
                Particle targetParticle = particles[Random.Range(0, particles.Count)];
                
                for (int i = 0; i < maxSpawnAttempts; i++)
                {
                    Vector2 randomDir = Random.insideUnitCircle.normalized;
                    // Position exactly the particle width away from the chosen particle's center
                    Vector2 testPos = (Vector2)targetParticle.transform.position + randomDir * targetParticle.transform.localScale.x;

                    if (IsPositionValid(testPos))
                    {
                        spawnPos = testPos;
                        Debug.Log("Attempting connected spawn");
                        validPositionFound = true;
                        break;
                    }
                }
            }

            // Fallback to random disconnected spawn if connected spawn wasn't chosen,
            // or if it failed to find a valid spot after max attempts
            if (!validPositionFound)
            {
                for (int i = 0; i < maxSpawnAttempts; i++)
                {
                    // Generate a random position within the bounds defined in ParticleManager
                    float randomX = Random.Range(ParticleManager.Instance.WidthBounds.x, ParticleManager.Instance.WidthBounds.y);
                    float randomY = Random.Range(ParticleManager.Instance.HeightBounds.x, ParticleManager.Instance.HeightBounds.y);
                    Vector2 testPos = new Vector2(randomX, randomY);

                    if (IsPositionValid(testPos))
                    {
                        spawnPos = testPos;
                        validPositionFound = true;
                        Debug.Log("Attempting disconnected spawn");
                        break;
                    }
                }
            }

            // If we found a valid spot, spawn it
            if (validPositionFound)
            {
                SpawnParticle(spawnPos);
                lastSpawnTime = Time.time;
            }
            
            return validPositionFound;
        }

        return false;
    }
}