
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target; // The shoulder/head pivot point
    public UDPInputReceiver inputSource;

    [Header("Positioning")]
    public Vector3 offset = new Vector3(0.6f, 0.2f, -3.0f);
    
    [Header("Smoothing")]
    [Range(0.01f, 1.0f)]
    public float rotationSmoothTime = 1f; // Time to reach target rotation
    public float positionSmoothTime = 1f; // Time to reach target position

    // Internal state for smoothing
    private Vector3 currentVelocity = Vector3.zero;
    private Quaternion targetRotation;
    private Quaternion currentRotationVelocity; 
    
    void LateUpdate()
    {
        if (inputSource == null || target == null) return;

        // Convert Raw UDP Input to Target Angles
        float targetYaw = Mathf.Clamp(inputSource.yaw * 100f,-180f, 180f); 
        float targetPitch = Mathf.Clamp(inputSource.pitch * 20f, -40f, 60f);

        //Create the Desired Rotation
        Quaternion desiredRotation = Quaternion.Euler(targetPitch, targetYaw, 0);

        // Smooth the Rotation 
        transform.rotation = SmoothDampQuaternion(transform.rotation, desiredRotation, ref currentRotationVelocity, rotationSmoothTime);

        // Calculate Smooth Position
        // Rotate the offset by our CURRENT smoothed rotation
        Vector3 targetPosition = target.position + (transform.rotation * offset);
        
        // Smoothly move the camera to that position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, positionSmoothTime);
    }

    // Helper function to smooth quaternions (not built-in to Unity)
    public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref Quaternion deriv, float time)
    {
        if (Time.deltaTime < Mathf.Epsilon) return current;
        float dot = Quaternion.Dot(current, target);
        float multiplier = dot > 0f ? 1f : -1f;
        target.x *= multiplier;
        target.y *= multiplier;
        target.z *= multiplier;
        target.w *= multiplier;
        Vector4 result = new Vector4(
            Mathf.SmoothDamp(current.x, target.x, ref deriv.x, time),
            Mathf.SmoothDamp(current.y, target.y, ref deriv.y, time),
            Mathf.SmoothDamp(current.z, target.z, ref deriv.z, time),
            Mathf.SmoothDamp(current.w, target.w, ref deriv.w, time)
        ).normalized;
        
        deriv.x = (result.x - current.x) / Time.deltaTime;
        deriv.y = (result.y - current.y) / Time.deltaTime;
        deriv.z = (result.z - current.z) / Time.deltaTime;
        deriv.w = (result.w - current.w) / Time.deltaTime;
        return new Quaternion(result.x, result.y, result.z, result.w);
    }
}