using UnityEngine;

public class GreenParticle : Particle
{
    private float creationTime;

    public void SetCreationTime(float time)
    {
        creationTime = time;
    }

    public bool ReadyToTransformBlues()
    {
        // wait half a second before transforming blues to give them a chance to move away and not immediately chain collide with the new green particle
        return Time.time - creationTime >= .5f;
    }
}
