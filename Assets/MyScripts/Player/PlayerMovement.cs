// using UnityEngine;
// using UnityEngine.InputSystem;

// [RequireComponent(typeof(CharacterController))]
// public class PlayerMovement : MonoBehaviour
// {
//     [Header("Movement Settings")]
//     public float walkSpeed = 4f;
//     public float runSpeed = 6f;
//     public float rotationSpeed = 10f;
//     public float gravity = -30f;

//     [Header("References")]
//     public Transform cameraTransform;
//     public Animator animator;

//     private CharacterController controller;
//     private Vector2 moveInput;
//     private Vector3 velocity;
//     private bool isGrounded;

//     private float currentSpeed;

//     private void Awake()
//     {
//         controller = GetComponent<CharacterController>();

//         if (cameraTransform == null)
//             cameraTransform = Camera.main.transform;
//     }

//     private void Update()
//     {
//         HandleMovement();
//         ApplyGravity();
//         UpdateAnimator();
//     }

//     // Called from Input System
//     public void OnMove(InputAction.CallbackContext context)
//     {
//         moveInput = context.ReadValue<Vector2>();
//     }

//     private void HandleMovement()
//     {
//         Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);

//         if (move.magnitude < 0.1f)
//         {
//             currentSpeed = 0f;
//             return;
//         }

//         // Camera-relative movement
//         Vector3 camForward = cameraTransform.forward;
//         Vector3 camRight = cameraTransform.right;

//         camForward.y = 0;
//         camRight.y = 0;

//         camForward.Normalize();
//         camRight.Normalize();

//         Vector3 moveDirection = camForward * move.z + camRight * move.x;

//         currentSpeed = runSpeed;

//         controller.Move(moveDirection * currentSpeed * Time.deltaTime);

//         // Smooth rotation toward movement direction
//         Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
//         transform.rotation = Quaternion.Slerp(
//             transform.rotation,
//             targetRotation,
//             rotationSpeed * Time.deltaTime
//         );
//     }

//     private void ApplyGravity()
//     {   
//         // isGrounded = controller.isGrounded;

//         // if (isGrounded && velocity.y < 0)
//         //     velocity.y = -60f;

//         // velocity.y += gravity * Time.deltaTime;

//         // controller.Move(velocity * Time.deltaTime);

//         float displacementY = (velocity.y * Time.deltaTime) + (0.5f * gravity * Mathf.Pow(Time.deltaTime, 2));

//         velocity.y += gravity * Time.deltaTime;

//         controller.Move(new Vector3(0, displacementY, 0));
//     }

//     private void UpdateAnimator()
//     {
//         float animationSpeed = moveInput.magnitude * currentSpeed;
//         animator.SetFloat("Speed", animationSpeed);
//     }
// }

// using UnityEngine;
// using UnityEngine.InputSystem;

// [RequireComponent(typeof(CharacterController))]
// public class PlayerMovement : MonoBehaviour
// {
//     [Header("Movement Settings")]
//     public float walkSpeed = 4f;
//     public float runSpeed = 6f;
//     public float rotationSpeed = 10f;
//     public float gravity = -98.1f;

//     [Header("References")]
//     public Transform cameraTransform;
//     public Animator animator;

//     private CharacterController controller;
//     private Vector2 moveInput;
//     private Vector3 velocity;
//     private bool isGrounded;
//     private bool isJumping;
//     private float? jumpButtonPressed;
//     private float? lastGroundedTime;

//     [SerializeField]
//     private float jumpButtonGracePeriod;

//     private float currentSpeed;

//     private void Awake()
//     {
//         controller = GetComponent<CharacterController>();

//         if (cameraTransform == null)
//             cameraTransform = Camera.main.transform;
//     }

//     private void Update()
//     {
//         HandleMovement();
//         ApplyGravity();
//         HandleJump();
//         UpdateAnimator();
//     }

//     // Called from Input System
//     public void OnMove(InputAction.CallbackContext context)
//     {
//         moveInput = context.ReadValue<Vector2>();
//     }

//     public void OnJump(InputAction.CallbackContext context)
//     {
//         jumpButtonPressed = Time.time;
//     }

//     private void HandleMovement()
//     {
//         Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);

//         if (move.magnitude < 0.1f)
//         {
//             currentSpeed = 0f;
//             return;
//         }

//         // Camera-relative movement
//         Vector3 camForward = cameraTransform.forward;
//         Vector3 camRight = cameraTransform.right;

//         camForward.y = 0;
//         camRight.y = 0;

//         camForward.Normalize();
//         camRight.Normalize();

//         Vector3 moveDirection = camForward * move.z + camRight * move.x;

//         currentSpeed = runSpeed;

//         if (controller.isGrounded)
//         {
//             controller.Move(moveDirection * currentSpeed * Time.deltaTime);
//         }

//         if(isGrounded == false)
//         {
//             controller.Move(moveDirection * walkSpeed * Time.deltaTime);
//         }

//         // Smooth rotation toward movement direction
//         Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
//         transform.rotation = Quaternion.Slerp(
//             transform.rotation,
//             targetRotation,
//             rotationSpeed * Time.deltaTime
//         );
//     }

//     private void ApplyGravity()
//     {   
//         // isGrounded = controller.isGrounded;

//         // if (isGrounded && velocity.y < 0)
//         //     velocity.y = -60f;

//         // velocity.y += gravity * Time.deltaTime;

//         // controller.Move(velocity * Time.deltaTime);

//         float displacementY = (velocity.y * Time.deltaTime) + (0.5f * gravity * Mathf.Pow(Time.deltaTime, 2));

//         velocity.y += gravity * Time.deltaTime;

//         controller.Move(new Vector3(0, displacementY, 0));
//     }

//     void HandleJump()
//     {
//         if (controller.isGrounded)
//         {
//             lastGroundedTime = Time.time;
//         }

//         if(Time.time - lastGroundedTime <= jumpButtonGracePeriod)
//         {   
//             velocity.y = -0.5f;
//             animator.SetBool("IsGrounded", true);
//             isGrounded = true;
//             animator.SetBool("IsJumping", false);
//             isJumping = false;
//             animator.SetBool("IsFalling", false);

//             if(Time.time - jumpButtonPressed <= jumpButtonGracePeriod)
//             {
//                 velocity.y = runSpeed;
//                 animator.SetBool("IsJumping", true);
//                 isJumping = true;
//                 jumpButtonPressed = null;
//                 lastGroundedTime = null;
//             }
            
//         }
//         else
//         {
//             animator.SetBool("IsGrounded", false);
//             isGrounded = false;

//             if((isJumping && velocity.y < 0) || velocity.y < -100)
//             {
//                 animator.SetBool("IsFalling", true);
//             }
//         }
//     }

//     private void UpdateAnimator()
//     {
//         float animationSpeed = moveInput.magnitude * currentSpeed;
//         animator.SetFloat("Speed", animationSpeed);
//     }
// }


using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 35f;
    public float runSpeed = 60f;
    public float rotationSpeed = 40f;
    public float jumpHeight = 10f;
    public float gravity = -98.1f;

    [Header("References")]
    public Transform cameraTransform;
    public Animator animator;

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;
    private bool jumpRequested = false;
    private bool hasLanded = false;

    private float currentSpeed;

    public PodiumVanish gateScript;
    public PodiumVanish podiumScript;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    private void Update()
    {   
        isGrounded = controller.isGrounded;

        HandleMovement();
        ApplyGravity();
        HandleJump();
        UpdateAnimator();
    }

    // Called from Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && isJumping == false)
        {
            jumpRequested = true;
        }
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

        if (controller.isGrounded)
        {
            controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        }

        if(isGrounded == false)
        {
            controller.Move(moveDirection * walkSpeed * Time.deltaTime);
        }

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

    void HandleJump()
    {
        if (controller.isGrounded)
        {
            // Reset vertical velocity when grounded
            if (velocity.y < 0) velocity.y = -2f; 

            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsJumping", false);
            isJumping = false;
            animator.SetBool("IsFalling", false);

            if (jumpRequested)
            {
                // Physics formula for jump velocity: sqrt(jumpHeight * -2 * gravity)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                animator.SetBool("IsJumping", true);
                isJumping = true;
                animator.SetBool("IsGrounded", false);
                jumpRequested = false;
            }
        }
        else
        {
            animator.SetBool("IsGrounded", false);
            if ((isJumping && velocity.y<0) || velocity.y < -100) animator.SetBool("IsFalling", true);
        }

        // Consume the jump request so we don't jump again until next press
        // jumpRequested = false;
    }

    private void UpdateAnimator()
    {
        float animationSpeed = moveInput.magnitude * currentSpeed;
        animator.SetFloat("Speed", animationSpeed);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if the object we hit is the Terrain and we haven't landed before
        if (!hasLanded && hit.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Player has touched the earth for the first time!");
            hasLanded = true; // Set to true so this never runs again
            gateScript.StartSinking();
            podiumScript.StartSinking();
        }
    }

    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {   
        
        // if (sinkObj != null)
        // {   
            Debug.Log("scene change called");
            podiumScript = GameObject.Find("podium").GetComponent<PodiumVanish>();
            gateScript = GameObject.Find("gate").GetComponent<PodiumVanish>();
        // }
    }
}