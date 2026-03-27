using UnityEngine;

public class BlueSpawner : ParticleSpawner
{
    private int maxSpawnAttempts = 10;
    [SerializeField] private float additionalHeight = 3f; // blue particles need to spawn a bit higher than everything else

    protected override bool SpawnParticles()
    {
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            Vector2 spawnPos = Vector2.zero;
            bool validPositionFound = false;

            for (int i = 0; i < maxSpawnAttempts; i++)
            {
                float randomX = Random.Range(ParticleManager.Instance.WidthBounds.x, ParticleManager.Instance.WidthBounds.y);
                
                // Bias towards higher elevations by square rooting the random value (0-1) before lerping.
                // sqrt(x) will curve values up, skewing the distribution heavily towards 1.0.
                float heightBias = Mathf.Pow(Random.value, 0.5f); 
                float randomY = Mathf.Lerp(ParticleManager.Instance.HeightBounds.x, ParticleManager.Instance.HeightBounds.y, heightBias);
                
                Vector2 testPos = new Vector2(randomX, randomY);

                if (IsPositionValid(testPos * Vector2.up))
                {
                    spawnPos = testPos + Vector2.up * additionalHeight; // apply the additional height
                    validPositionFound = true;
                    break;
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