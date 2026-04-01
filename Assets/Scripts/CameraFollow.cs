using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The target to follow
    public float smoothSpeed = 0.125f; // Smoothing speed

    void FixedUpdate()
    {
        if (target != null)
        {
            // Calculate the desired position
            Vector3 desiredPosition = target.position;

            // Smoothly interpolate to the desired position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            // Set only the x and y positions
            Vector3 newPosition = transform.position;
            newPosition.x = smoothedPosition.x;
            newPosition.y = smoothedPosition.y;
            transform.position = newPosition;
        }
    }
}