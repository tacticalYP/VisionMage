
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class UDPPlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    private CharacterController controller;
    public UDPInputReceiver dataStream;
    
    [Header("Rotation Settings (Head)")]
    public float yawDeadzone = 0.5f;  
    public float pitchDeadzone = 0f; 
    
    public float yawSpeedLeft = 20f;  
    public float yawSpeedRight = 15f; 
    
    // WARNING: Change this back to 20 or 30! 
    // If you leave it at 25000 now that it's fixed, you will break the space-time continuum.
    public float pitchSpeed = 300f;  

    [Header("Movement Settings (Hand)")]
    public float moveSpeed = 2f;
    public float handDeadzone = 0.5f;  
    public float gravity = -9.81f;

    private float accumulatedYaw = 0f;
    private float accumulatedPitch = 0f;
    private Vector3 velocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        accumulatedYaw = transform.localEulerAngles.y;
        
        float startPitch = Camera.main.transform.localEulerAngles.x;
        if (startPitch > 180f) startPitch -= 360f;
        accumulatedPitch = startPitch;
    }

    private void Update()
    {
        if (dataStream == null) return;

        HandleRotation();
        HandleMovement();
        ApplyGravity();
    }

    private void HandleRotation()
    {
        // --- YAW LOGIC (Left / Right) ---
        if (Mathf.Abs(dataStream.yaw) > yawDeadzone)
        {
            float activeYaw = Mathf.Abs(dataStream.yaw) - yawDeadzone;
            float direction = Mathf.Sign(dataStream.yaw);
            
            float currentYawSpeed = (direction < 0) ? yawSpeedLeft : yawSpeedRight;
            accumulatedYaw += activeYaw * direction * currentYawSpeed * Time.deltaTime;
        }

        // --- PITCH LOGIC (Up / Down) ---
        if (Mathf.Abs(dataStream.pitch) > pitchDeadzone) 
        {
            float activePitch = Mathf.Abs(dataStream.pitch) - pitchDeadzone;
            float direction = Mathf.Sign(dataStream.pitch);
            
            // THE FIX IS HERE! -= instead of +=
            // This perfectly inverts the controls so you escape the clamp!
            // accumulatedPitch += activePitch * direction * pitchSpeed * Time.deltaTime;
            float curvedPitch = activePitch*activePitch * activePitch * Mathf.Sign(activePitch);
        
            accumulatedPitch += curvedPitch * direction * pitchSpeed * Time.deltaTime;
        }

        // The Clamp
        accumulatedPitch = Mathf.Clamp(accumulatedPitch, -89f, 89f);

        transform.localRotation = Quaternion.Euler(0, accumulatedYaw, 0); 
        Camera.main.transform.localRotation = Quaternion.Euler(accumulatedPitch, 0, 0); 
    }

    private void HandleMovement()
    {
        float moveX = dataStream.calcX;
        float moveY = dataStream.calcY; 

        if (Mathf.Abs(moveX) < handDeadzone) moveX = 0;
        if (Mathf.Abs(moveY) < handDeadzone) moveY = 0;

        Vector3 moveDirection = (transform.right * moveX) + (transform.forward * moveY);
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (animator != null)
        {
            animator.SetFloat("Speed", moveDirection.magnitude * moveSpeed);
        }
    }

    private void ApplyGravity()
    {   
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }
}