using System;
using UnityEngine;

public class TopDownCarController : MonoBehaviour
{
    [Header("Car settings")]
    public float driftFactor = 0.95f;
    public float accelerationFactor = 30.0f;
    public float turnFactor = 3.5f;
    public float maxSpeed = 20f;

    float accelerationInput = 0;
    float steeringInput = 0; 
    float velocityVsUp = 0;
    float rotationAngle = 0;
    Rigidbody2D carRigidbody2D;

    void Awake() {
        // Simplely grabs the Rigidbody2D component from Unity
        carRigidbody2D = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

     void FixedUpdate() {
        ApplyEngineForce();

        KillOrthogonalVelocity();

        ApplySteering();
    }

     void ApplySteering()
    {
        // limit cars ability to turn when not moving
        float minSpeedBeforeAllowTurningFactor = (carRigidbody2D.linearVelocity.magnitude / 8);
        minSpeedBeforeAllowTurningFactor = Mathf.Clamp01(minSpeedBeforeAllowTurningFactor);

        // updates rotation upon turning
        rotationAngle -= steeringInput * turnFactor;
        // apply steering by rotating the car object
        carRigidbody2D.MoveRotation(rotationAngle);
    }

    public void SetInputVector(Vector2 inputVector){
        steeringInput = inputVector.x;
        accelerationInput = inputVector.y;
    }

void KillOrthogonalVelocity()
    {
        Vector2 forwardVelocity = transform.up * Vector2.Dot(carRigidbody2D.linearVelocity, transform.up);
        Vector2 rightVelocity = transform.right * Vector2.Dot(carRigidbody2D.linearVelocity, transform.right);

        carRigidbody2D.linearVelocity = forwardVelocity + rightVelocity * driftFactor;
    }

// Controls the "Engine" of the car. Movement forward and backwards
     void ApplyEngineForce() {
    velocityVsUp = Vector2.Dot(transform.up, carRigidbody2D.linearVelocity);

    // Prevent exceeding forward max speed
    if (velocityVsUp > maxSpeed && accelerationInput > 0)
        return;

    // Prevent exceeding reverse max speed
    if (velocityVsUp < -maxSpeed * 0.5f && accelerationInput < 0)
        return;

    if (carRigidbody2D.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed && accelerationInput > 0)
        return;

    // Slow down when no acceleration is provided
    if (accelerationInput == 0)
        carRigidbody2D.linearDamping  = Mathf.Lerp(carRigidbody2D.linearDamping , 3.0f, Time.fixedDeltaTime * 3);
    else
        carRigidbody2D.linearDamping  = 0;

    // Apply the force with flipped logic
    Vector2 engineForceVector = transform.up * accelerationInput * accelerationFactor;
    carRigidbody2D.AddForce(engineForceVector, ForceMode2D.Force);
}

    
}
