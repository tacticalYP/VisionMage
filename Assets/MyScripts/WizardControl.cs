using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

public class WizardControl : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float mouseSensitivity = 100f;
    public Transform cameraPivot;

    private CharacterController controller;
    private InputSystem_Actions inputActions;
    private Animator animator;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private float xRotation = 0f;

    private float gravity = -9.81f;
    private float verticalVelocity;
    [SerializeField] private float walkSpeed = 40f;
    [SerializeField] private float runSpeed = 100f;
    [SerializeField] private float jumpHeight = 20f;
    private bool isSprinting;


    void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        inputActions.Player.Sprint.performed += ctx => isSprinting = true;
        inputActions.Player.Sprint.canceled += ctx => isSprinting = false;

        inputActions.Player.Jump.performed += ctx => Jump();
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Move();
        RotateCamera();
        // HandleMovement();
        // HandleGravity();
        animator.SetFloat("MoveX", moveInput.x);
        animator.SetFloat("MoveY", moveInput.y);
        animator.SetFloat("Speed", moveInput.magnitude);
        animator.SetBool("IsGrounded", controller.isGrounded);
        animator.SetBool("IsSprinting", isSprinting);
        animator.SetFloat("VerticalVelocity", verticalVelocity);
        
    }

    void Move()
    {

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -40f;
        }

        Vector3 move = new Vector3(moveInput.x, 0f, moveInput.y);
        move = transform.TransformDirection(move);

        float currentSpeed = isSprinting ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // controller.Move(move * moveSpeed * Time.deltaTime);

        verticalVelocity += gravity * Time.deltaTime;
        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        // animator.SetBool("IsGrounded", controller.isGrounded);

        // Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        // controller.Move(move * moveSpeed * Time.deltaTime);

        // if(moveInput.x!=0 || moveInput.y!=0){
        //     UnityEngine.Debug.Log("transform");
        //     UnityEngine.Debug.Log($"{transform.right}, {transform.forward}");
        //     UnityEngine.Debug.Log($"{moveInput.x}, {moveInput.y}");
        //     UnityEngine.Debug.Log($"{Time.deltaTime}");
        //     UnityEngine.Debug.Log($"{move * moveSpeed * Time.deltaTime}");
        // }

        
        
    }

    void Jump()
    {
        UnityEngine.Debug.Log($"{controller.isGrounded}");
        // if(controller.isGrounded==true)
        // {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump");
            UnityEngine.Debug.Log("Jump1");
        // }
        UnityEngine.Debug.Log("Jump2");
    }

    void RotateCamera()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}