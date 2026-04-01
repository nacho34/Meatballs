using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class Bat : MonoBehaviour
{
    public Player player; // Reference to the player GameObject
    public float hitForce = 10f; // Adjustable force amount
    public Vector2 SendDirection;
    private Collider2D batCollider;
    private List<GameObject> hitObjects = new List<GameObject>();

    void Awake()
    {
        batCollider = GetComponent<Collider2D>();
    }

    public void ResetHitList()
    {
        hitObjects.Clear();
    }

    void FixedUpdate()
    {
        if (player == null || batCollider == null || !player.isAttacking)
        {
            return;
        }

        Bounds bounds = batCollider.bounds;
        Collider2D[] collisions = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);

        foreach (Collider2D collision in collisions)
        {
            if (collision == null || collision == batCollider || hitObjects.Contains(collision.gameObject))
            {
                continue;
            }

            int layer = collision.gameObject.layer;
            if (LayerMask.LayerToName(layer).Contains("Balls")) // Assuming layers end with "Balls"
            {
                hitObjects.Add(collision.gameObject); // Add to hit list

                // Apply force to the ball in the direction of the ball (from bat to ball)
                Rigidbody2D ballRb = collision.gameObject.GetComponent<Rigidbody2D>();
                if (ballRb != null)
                {
                    ballRb.AddForce(SendDirection * hitForce, ForceMode2D.Impulse);
                }
            }
        } 
    }
}
