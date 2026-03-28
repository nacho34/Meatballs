using UnityEngine;

public class GreenSpawner : ParticleSpawner
{
    [SerializeField] private float disconnectedSpawnChance = 0.5f;
    [SerializeField] private float lowerElevationBiasPower = 2f;
    private int maxSpawnAttempts = 10;

    protected override bool SpawnParticles()
    {
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            Vector2 spawnPos = Vector2.zero;
            bool validPositionFound = false;
            bool doDisconnectedSpawn = true;
            
            if (particles.Count > 0)
            {
                doDisconnectedSpawn = Random.value < disconnectedSpawnChance;
            }

            if (!doDisconnectedSpawn)
            {
                // Try connected spawn
                Particle targetParticle = particles[Random.Range(0, particles.Count)];
                
                for (int i = 0; i < maxSpawnAttempts; i++)
                {
                    // Bias towards upward growth: angle between -18 and 198 degrees
                    // mostly the upper semicircle, allowing slight downward growth for organic shapes
                    float angle = Random.Range(-Mathf.PI * 0.1f, Mathf.PI * 1.1f);
                    Vector2 randomDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                    
                    Vector2 testPos = (Vector2)targetParticle.transform.position + randomDir * targetParticle.transform.localScale.x;

                    if (IsPositionValid(testPos))
                    {
                        spawnPos = testPos;
                        validPositionFound = true;
                        break;
                    }
                }
            }

            // Fallback to random disconnected spawn if connected spawn wasn't chosen, or if it failed
            if (!validPositionFound)
            {
                for (int i = 0; i < maxSpawnAttempts; i++)
                {
                    float randomX = Random.Range(ParticleManager.Instance.WidthBounds.x, ParticleManager.Instance.WidthBounds.y);
                    
                    // Bias towards lower elevations by applying a power curve.
                    // pow(x, 2) curves values down, skewing the distribution towards 0.0.
                    float heightBias = Mathf.Pow(Random.value, lowerElevationBiasPower); 
                    float randomY = Mathf.Lerp(ParticleManager.Instance.HeightBounds.x, ParticleManager.Instance.HeightBounds.y, heightBias);
                    
                    Vector2 testPos = new Vector2(randomX, randomY);

                    if (IsPositionValid(testPos))
                    {
                        spawnPos = testPos;
                        validPositionFound = true;
                        break;
                    }
                }
            }

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

