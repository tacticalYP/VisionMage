using UnityEngine;
using UnityEngine.SceneManagement;
using System;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class ThirdPersonControllerr : MonoBehaviour
{
    public Transform cameraTransform;
    public UDPInputReceiver inputSource;
    
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;
    public float gravity = -98.1f;

    private CharacterController controller;
    public Animator animator;
    private Vector3 velocity;
    private bool hasLanded = false;
    private PodiumVanish gateScript;
    private PodiumVanish podiumScript;
    // private int temp = 0;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    void Update()
    {
        if (inputSource == null) return;

        //Get Camera Directions (flattened to ignore tilt)
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calculate Direction relative to Camera
        // X = horizontal, Y = vertical/forward
        // if (inputSource.CurrentState == 1 && temp==0)
        // {
        //     long receiveTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //     Debug.Log(receiveTime);
        //     Debug.Log(inputSource.sentTimeStamp);
        //     temp=1;
        // }
        // else if(inputSource.CurrentState == 0)
        // {
        //     temp=0;
        // }
        Vector3 moveDirection = (forward * inputSource.calcY) + (right * inputSource.calcX);

        // Rotate Character to face Movement Direction
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Handle Movement & Gravity
        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        
        velocity.y += gravity * Time.deltaTime;
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
        controller.Move(velocity * Time.deltaTime);

        //Animation (Assuming a parameter named "Speed")
        float currentInputMagnitude = new Vector2(inputSource.calcX, inputSource.calcY).magnitude;
        animator.SetFloat("Speed", currentInputMagnitude);
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

            // Stop the player on same place
            animator.Play("Idle",0);
            animator.speed = 0;
            this.enabled = false;
        }
    }

    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {   
        if (scene.name == "BossRoom")
        {   
            podiumScript = GameObject.Find("podium").GetComponent<PodiumVanish>();
            gateScript = GameObject.Find("gate").GetComponent<PodiumVanish>();
        }
    }
}