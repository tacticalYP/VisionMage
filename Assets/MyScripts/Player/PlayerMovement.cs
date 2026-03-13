using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 4f;
    public float runSpeed = 6f;
    public float rotationSpeed = 10f;
    public float gravity = -30f;

    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isGrounded;

    private float currentSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        HandleMovement();
        ApplyGravity();
        UpdateAnimator();
    }

    // Called from Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);

        if (move.magnitude < 0.1f)
        {
            currentSpeed = 0f;
            return;
        }

        // Camera-relative movement
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * move.z + camRight * move.x;

        currentSpeed = runSpeed;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Smooth rotation toward movement direction
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void ApplyGravity()
    {   
        // isGrounded = controller.isGrounded;

        // if (isGrounded && velocity.y < 0)
        //     velocity.y = -60f;

        // velocity.y += gravity * Time.deltaTime;

        // controller.Move(velocity * Time.deltaTime);

        float displacementY = (velocity.y * Time.deltaTime) + (0.5f * gravity * Mathf.Pow(Time.deltaTime, 2));

        velocity.y += gravity * Time.deltaTime;

        controller.Move(new Vector3(0, displacementY, 0));
    }

    private void UpdateAnimator()
    {
        float animationSpeed = moveInput.magnitude * currentSpeed;
        animator.SetFloat("Speed", animationSpeed);
    }
}