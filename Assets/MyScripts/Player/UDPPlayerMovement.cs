
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class UDPPlayerMovement : MonoBehaviour
{
    public float gravity = -9.81f;
    public Animator animator;
    private CharacterController controller;
    public UDPInputReceiver dataStream;
    
    [Header("Rotation Settings")]
    public float rotationSensitivity = 100f;
    public float rotationSmoothing = 0.005f;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    private Vector3 velocity;
    private float smoothYaw;
    private float smoothPitch;
    private Vector3 moveTarget;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

    }

    private void Update()
    {
        HandleMovement();
        ApplyGravity();
        UpdateAnimator();
    }

    // private void HandleMovement()
    // {
    //     if (dataStream == null) return;

    //     moveTarget = (new Vector3(dataStream.calcX, 0, dataStream.calcY))* moveSpeed * Time.deltaTime;
    //     controller.Move(moveTarget);

    //     float targetYaw = dataStream.yaw * rotationSensitivity;
    //     float targetPitch = dataStream.pitch * rotationSensitivity;
    //     Quaternion targetRotation = Quaternion.Euler(targetPitch, targetYaw, 0);

    //     float smoothingFactor = 10f; 
    //     Camera.main.transform.localRotation = Quaternion.Slerp(
    //         Camera.main.transform.localRotation, 
    //         targetRotation, 
    //         smoothingFactor * Time.deltaTime
    //     );

    //     Debug.Log($"{moveTarget.x}, {moveTarget.z}, {dataStream.yaw}, {dataStream.pitch}");
    // }

        // Add these private variables to your class to store the total rotation
    private float currentYaw;
    private float currentPitch;

    private void HandleMovement()
    {
        if (dataStream == null) return;

        moveTarget = (new Vector3(dataStream.calcX, 0, dataStream.calcY)) * moveSpeed * Time.deltaTime;
        controller.Move(moveTarget);

        currentYaw = dataStream.yaw * rotationSensitivity;
        currentPitch = dataStream.pitch * rotationSensitivity;

        currentPitch = Mathf.Clamp(currentPitch, -89f, 89f);

        Quaternion targetCameraRotation = Quaternion.Euler(currentPitch, 0, 0);

        Quaternion targetPlayerRotation = Quaternion.Euler(0, currentYaw, 0);
        
        Camera.main.transform.localRotation = Quaternion.Slerp(
            Camera.main.transform.localRotation, 
            targetCameraRotation, 
            rotationSmoothing
        );

        transform.localRotation = Quaternion.Slerp(
            transform.localRotation, 
            targetPlayerRotation, 
            rotationSmoothing
        );
    }

    private void ApplyGravity()
    {   
        float displacementY = (velocity.y * Time.deltaTime) + (0.5f * gravity * Mathf.Pow(Time.deltaTime, 2));

        velocity.y += gravity * Time.deltaTime;

        controller.Move(new Vector3(0, displacementY, 0));
    }

    private void UpdateAnimator()
    {
        float animationSpeed = moveTarget.magnitude * moveSpeed;
        animator.SetFloat("Speed", animationSpeed);
    }
}