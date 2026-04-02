using UnityEngine;
using System.Collections;

public partial class PodiumVanish : MonoBehaviour
{
    [Header("Movement Settings")]
    public float sinkSpeed = 0.5f;
    public float sinkDistance = 5.0f;
    
    [Header("Vibration Settings")]
    public float shakeIntensity = 0.1f;
    public float shakeSpeed = 20.0f;

    private bool isSinking = false;
    private Vector3 startPosition;
    private float timer = 0f;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        if (isSinking)
        {
            PerformSink();
        }
    }

    // This is called by the Trigger script later
    public void StartSinking()
    {   
        Debug.Log("sink called");
        isSinking = true;
    }

    void PerformSink()
    {
        timer += Time.deltaTime;

        // 1. Calculate how far we have sunk so far
        // We calculate a single 'downwardOffset' based on time and speed
        float downwardOffset = sinkSpeed * timer;
        
        // 2. Calculate the Vertical (Y) target
        float newY = startPosition.y - downwardOffset;

        // 3. Calculate Vibration (Shake) relative to ZERO
        // PerlinNoise returns 0 to 1, so -0.5f gives us -0.5 to 0.5
        float shakeX = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0) - 0.5f) * shakeIntensity;
        float shakeZ = (Mathf.PerlinNoise(0, Time.time * shakeSpeed) - 0.5f) * shakeIntensity;

        // 4. Set the position REFRESHED from the startPosition every frame
        // This prevents "drifting" because we aren't adding to the current position, 
        // we are setting it based on the original anchor point.
        transform.position = new Vector3(
            startPosition.x + shakeX, 
            newY, 
            startPosition.z + shakeZ
        );

        // 5. Check if it's deep enough to vanish
        // Use the calculated newY to check the distance
        if (newY < startPosition.y - sinkDistance)
        {
            Debug.Log("Sinking Complete. Object Vanished.");
            gameObject.SetActive(false);
        }
    }
}