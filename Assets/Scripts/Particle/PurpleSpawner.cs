using UnityEngine;

public class PurpleSpawner : ParticleSpawner
{
    protected override bool SpawnParticles()
    {
        if (Time.time - lastSpawnTime >= spawnInterval)
        {
            Vector2 pos = new Vector2(Random.Range(ParticleManager.Instance.WidthBounds.x, ParticleManager.Instance.WidthBounds.y), 
            Random.Range(ParticleManager.Instance.HeightBounds.x, ParticleManager.Instance.HeightBounds.y));

            SpawnParticle(pos);
            lastSpawnTime = Time.time;
            return true;
        }

        return false;
    }
}
