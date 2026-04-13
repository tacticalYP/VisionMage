using UnityEngine;

public class FixedFollow : MonoBehaviour
{
    public Transform targetToFollow;
    public Vector3 offset = new Vector3(2f, 0f, 0f); // distance from the target

    void LateUpdate()
    {
        if (targetToFollow != null)
        {
            // Set position based on player position + fixed offset
            // We do NOT use playerTransform.right because that changes with rotation
            transform.position = targetToFollow.position + offset;
        }
    }
}