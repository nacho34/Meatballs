using UnityEngine;
using System.Collections.Generic;

public class TriggerGlobSound : MonoBehaviour
{
    // Collision info
    private HashSet<GameObject> activeCollisions = new HashSet<GameObject>();
    public int CollisionCount => activeCollisions.Count;
    
    protected virtual void OnTriggerEnter2D(Collider2D collider)
    {
        if (CollisionCount >= 1) { 
            activeCollisions.Add(collider.gameObject);
            return; 
        } // Only play sound on the first collision to avoid spamming
        activeCollisions.Add(collider.gameObject);
        
        if (collider.gameObject.TryGetComponent(out Particle particle) || collider.gameObject.TryGetComponent(out Player player))
        {
            GetComponent<RandomAudioPlayer>().PlayRandomOneShot();
        }
    }

    protected virtual void OnTriggerExit2D(Collider2D collider)
    {
        activeCollisions.Remove(collider.gameObject);
    }
}
