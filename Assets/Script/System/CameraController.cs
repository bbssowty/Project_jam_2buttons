using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // The target the camera should follow (the player)
    public Transform target;

    // How quickly the camera catches up to the target's position
    [Tooltip("A smaller value will make the camera follow more tightly.")]
    public float smoothTime = 0.25f;

    // A reference variable used by the SmoothDamp function (leave as is)
    private Vector3 velocity = Vector3.zero;

    // LateUpdate is called every frame, after all Update functions have been called.
    // This is the best place to put camera logic, as it ensures the target
    // has finished moving for the frame before the camera updates.
    void LateUpdate()
    {
        // If no target is assigned, do nothing
        if (target == null)
        {
            return;
        }

        // Determine the camera's target position. We only want to follow the
        // target's X and Y position, while keeping the camera's own Z position.
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

        // Smoothly move the camera's current position towards the target position.
        // The 'velocity' variable is modified by the function every time it's called.
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}