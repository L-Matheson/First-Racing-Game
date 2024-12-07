using System;
using System.Linq;
using UnityEngine;

public class CarAIHandler : MonoBehaviour
{
    public enum AIMode { followPlayer, followWaypoints };
    [Header("AI Settings")]
    public AIMode aiMode;
    public float maxSpeed = 16;
    public bool isAvoidingCars = true;

    // Colliders
    PolygonCollider2D polygonCollider2D;

    // Component
    TopDownCarController topDownCarController;

    // local values
    Vector2 avoidanceVectorLerped = Vector3.zero;
    Vector3 targetPosition = Vector3.zero;
    Transform targetTransorm = null;

    // Waypoints
    WaypointNode currentWaypoint = null;
    WaypointNode[] allWayPoints;

    // Stuck Detection
    private float stuckDetectionTimer = 0f;
    private float stuckThresholdTime = 2f; // Time the car must be "stuck" to consider it stuck
    private float minVelocityThreshold = 0.1f; // Minimum velocity to avoid being considered stuck
    private bool isBackingUp = false;
    private float backingUpTimer = 0f;
    private float maxBackingUpTime = 0.4f; // Maximum time spent backing up

    void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
        allWayPoints = FindObjectsOfType<WaypointNode>();

        polygonCollider2D = GetComponent<PolygonCollider2D>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 inputVector = Vector2.zero;

        // like an if statement, think SQL (CASE WHEN)
        // This is controlled inside Unity. Inside the AI car, either followPlayer or followWaypoints can be checked
        switch (aiMode)
        {
            case AIMode.followPlayer:
                FollowPlayer();
                break;

            case AIMode.followWaypoints:
                FollowWaypoints();
                break;
        }

        if (isBackingUp)
        {
            // Perform backing-up maneuver if the car is stuck
            inputVector = BackUp();
        }
        else
        {
            inputVector.x = TurnTowardTarget();
            inputVector.y = ApplyThrottleOrBrake(inputVector.x);

        }

        topDownCarController.SetInputVector(inputVector);

        // Check if the car is stuck and trigger recovery
        CheckIfStuckAndFix();
    }

    float ApplyThrottleOrBrake(float inputX)
    {

        // applies gas depending on the amount the car needs to turn. 
        // Ensures car doesn't constantly drift around nodes of the graph
        return 1.05f - Mathf.Abs(inputX) / 1.0f;
    }

    // the out vector allows for these params to be directly altered. This means that when
    // position or otherCarRightVector is passed in, the actual params position and 
    // otherCarRightVector will be altered without having to be returned. Think 
    // in python terms, when something is declared as global, or static in java
    bool IsCarsInFrontOfAICar(out Vector3 position, out Vector3 otherCarRightVector)
    {
        // Base case, ensures the AI car does not detect itself
        polygonCollider2D.enabled = false;

        // This allows for the car to use a circle cast (think a circle sensor) to detect if something is in front of the car. 
        // The equation also allows for the inital circle cast to start slightly ahead of the car
        RaycastHit2D raycastHit2D = Physics2D.CircleCast(transform.position + transform.up * 0.5f, 1f, transform.up, 12, 1 << LayerMask.NameToLayer("Car"));

        polygonCollider2D.enabled = true;

        if (raycastHit2D.collider != null && raycastHit2D.collider.CompareTag("Player"))
        {
            // For testing purposes, just draws a red line 12 units ahead. 
            Debug.DrawRay(transform.position, transform.up * 12, Color.red);

            position = raycastHit2D.collider.transform.position;

            // Since this is a racing game, the car is almost always turning right, so there is only 
            otherCarRightVector = raycastHit2D.collider.transform.right;

            return true;
        }
        else
        {
            // for testing, a black line is drawn if there is no car detected
            Debug.DrawRay(transform.position, transform.up * 12, Color.black);
        }
        // undo the base case since the car does need to be able to hit others

        // If the code made it here, it means no cars were detected
        // So, the parameters are reset to zero
        position = Vector3.zero;
        otherCarRightVector = Vector3.zero;
        return false;
    }

    // Runs the bool to check if there is a car infront of the AI car
    // If there is, alter the steering to avoid the car
    void AvoidCars(Vector2 vectorToTarget, out Vector2 newVectorToTarget)
    {
        if (IsCarsInFrontOfAICar(out Vector3 otherCarPosition, out Vector3 otherCarRightVector))
        {
            Vector2 avoidanceVector = Vector2.zero;

            // Calculate the reflecing vector if we would hit the other car.
            avoidanceVector = Vector2.Reflect((otherCarPosition - transform.position).normalized, otherCarRightVector);

            float distanceToTarget = (targetPosition - transform.position).magnitude;

            // This controls how much the car needs to turn 
            // depending on how close it is to another car
            float driveToTargetInfluence = 6.0f / distanceToTarget;

            driveToTargetInfluence = Mathf.Clamp(driveToTargetInfluence, 0.30f, 1.0f);

            float avoidanceInfluence = 1.0f - driveToTargetInfluence;
            // allows for smoother turning
            avoidanceVectorLerped = Vector2.Lerp(avoidanceVectorLerped, avoidanceVector, Time.fixedDeltaTime * 4);

            // avoidence vector
            newVectorToTarget = vectorToTarget * driveToTargetInfluence + avoidanceVectorLerped * avoidanceInfluence;
            newVectorToTarget.Normalize();

            Debug.DrawRay(transform.position, avoidanceVector * 10, Color.green);

            Debug.DrawRay(transform.position, newVectorToTarget * 10, Color.yellow);

            return;
        }

        // Default value if no cars are hit
        newVectorToTarget = vectorToTarget;
    }

    // Sometimes the car will get stuck while traversing the graph, this will back the car up quickly
    private void CheckIfStuckAndFix()
    {
        float currentSpeed = topDownCarController.GetCurrentSpeed();

        if (currentSpeed < minVelocityThreshold)
        {
            stuckDetectionTimer += Time.fixedDeltaTime;

            if (stuckDetectionTimer >= stuckThresholdTime)
            {
                isBackingUp = true;
                backingUpTimer = 0f;
            }
        }
        else
        {
            stuckDetectionTimer = 0f;
        }
    }

    private Vector2 BackUp()
    {
        backingUpTimer += Time.fixedDeltaTime;

        if (backingUpTimer >= maxBackingUpTime)
        {
            isBackingUp = false;
            return Vector2.zero;
        }

        float steerDirection = UnityEngine.Random.Range(-1f, 1f);
        return new Vector2(1f, -1f);
    }

    

 void FollowWaypoints()
{   // base case, if there is no current node, the car will find the nearest node
    if (currentWaypoint == null)
        currentWaypoint = FindClosestWayPoint();
   
        targetPosition = currentWaypoint.transform.position;

        float distanceToWaypoint = (targetPosition - transform.position).magnitude;

        if (distanceToWaypoint <= currentWaypoint.minDistanceToReachWaypoint)
        {
            WaypointNode shortestWaypoint = null;
            float shortestDistance = float.MaxValue;
            foreach (var nextWaypoint in currentWaypoint.nextWayPointNode)
            {
                    float distanceToNext = Vector3.Distance(transform.position, nextWaypoint.transform.position);
                    if (distanceToNext < shortestDistance)
                    {
                        shortestDistance = distanceToNext;
                        shortestWaypoint = nextWaypoint;
                    }
                }

            currentWaypoint = shortestWaypoint;
        }
    
}


    WaypointNode FindClosestWayPoint()
    {
        return allWayPoints
        .OrderBy(tag => Vector3.Distance(transform.position, tag.transform.position))
        .FirstOrDefault();
    }

    void FollowPlayer()
    {
        if (targetTransorm == null)
        {
            targetTransorm = GameObject.FindGameObjectWithTag("Player").transform;
        }
        if (targetTransorm != null)
        {
            targetPosition = targetTransorm.position;
        }

    }

    float TurnTowardTarget()
    {
        Vector2 vectorToTarget = targetPosition - transform.position;
        vectorToTarget.Normalize();

        if (isAvoidingCars)
        {
            AvoidCars(vectorToTarget, out vectorToTarget);
        }

        float angleToTarget = Vector2.SignedAngle(transform.up, vectorToTarget);
        angleToTarget *= -1;

        float steerAmount = angleToTarget / 45.0f;

        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);
        return steerAmount;
    }
}
