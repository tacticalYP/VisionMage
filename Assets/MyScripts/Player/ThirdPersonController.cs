using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("References")]
    public UDPInputReceiver inputSource;
    public Transform cameraTransform;
    private CharacterController controller;

    [Header("Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float cameraDistance = 3.0f;
    public float verticalOffset = 1.5f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // If camera isn't assigned, grab the main camera
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        HandleCamera();
        HandleMovement();
    }

    private void HandleCamera()
    {
        // Calculate Rotation from UDP Pitch/Yaw
        Quaternion rotation = Quaternion.Euler(inputSource.pitch, inputSource.yaw, 0);

        //Calculate Position: Character Pos + Offset - (Direction * Distance)
        Vector3 targetPosition = transform.position + Vector3.up * verticalOffset;
        Vector3 offset = rotation * new Vector3(0, 0, -cameraDistance);
        
        cameraTransform.position = targetPosition + offset;
        cameraTransform.rotation = rotation;
    }

    private void HandleMovement()
    {
        // Get raw inputs from your UDP script
        float x = inputSource.calcX;
        float y = inputSource.calcY;

        // Calculate direction relative to the Camera's forward
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // Flatten directions so the character doesn't fly up/down
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Final movement vector
        Vector3 moveDir = (camForward * y + camRight * x).normalized;

        // if (moveDir.magnitude >= 0.1f)
        // {
            // Rotate character to face the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Move the character
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
        // }

        // Apply gravity
        controller.Move(new Vector3(0, -9.81f, 0) * Time.deltaTime);
    }
}