using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public abstract class Particle : MonoBehaviour
{
    // how quickly and from how far apart particles of the same type attract each other
    public float AttractionRange;
    public float AttractionStrength;
    public ParticleSpawner spawner { get; set; }
    
    private Rigidbody2D _rb;
    public Rigidbody2D rb 
    { 
        get 
        { 
            if (_rb == null) _rb = GetComponent<Rigidbody2D>(); 
            return _rb; 
        } 
    }

    public void ApplyAttractionForce(Vector2 force)
    {
        rb.AddForce(force);
    }

    public void DestroySelf(){
        if (spawner != null)
            spawner.RemoveParticle(this);
    }
}
