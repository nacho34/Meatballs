using UnityEngine;

/// <summary>
/// Represents a brown particle that allows the player to cling to it.
/// When the player collides with this particle, a force is applied to pull the player towards the particle's center.
/// </summary>
public class BrownParticle : Particle
{
    /// <summary>
    /// The strength of the force applied to pull the player towards the particle's center.
    /// Adjustable in the Unity Inspector.
    /// </summary>
    [SerializeField] public float clingStrength = 100f;
}