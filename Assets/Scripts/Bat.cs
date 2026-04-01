using UnityEngine;
using System.Collections.Generic;

public class Bat : MonoBehaviour
{
    public Player player; // Reference to the player GameObject
    public float hitForce = 10f; // Adjustable force amount
    private List<GameObject> hitObjects = new List<GameObject>();

    public void ResetHitList()
    {
        hitObjects.Clear();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (player.isAttacking && !hitObjects.Contains(collision.gameObject)) // Check if the player is currently attacking and not already hit
        {
            hitObjects.Add(collision.gameObject); // Add to hit list
            
            // Check if colliding with any ball layer
            int layer = collision.gameObject.layer;
            if (LayerMask.LayerToName(layer).Contains("Balls")) // Assuming layers end with "Balls"
            {
                // Get direction from bat to ball
                Vector2 direction = (collision.transform.position - player.transform.position).normalized;
                
                // Apply force to the ball in the direction of the ball (from bat to ball)
                Rigidbody2D ballRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (ballRb != null)
                {
                    ballRb.AddForce(direction * hitForce, ForceMode2D.Impulse);
                }
            }
        }
    }
}
