using System;
using System.Linq;
using UnityEngine;

public class CarAIHandler : MonoBehaviour
{
    public enum AIMode {followPlayer, followWaypoints};
    [Header("AI Settings")]
    public AIMode aiMode;
    public float maxSpeed = 16;


    //Component
    TopDownCarController topDownCarController;

    //local values
    Vector3 targetPosition = Vector3.zero;
    Transform targetTransorm = null;

    //Waypoints
    WaypointNode currentWaypoint = null;
    WaypointNode[] allWayPoints;
 
    void Awake() {
        topDownCarController = GetComponent<TopDownCarController>();
        allWayPoints = FindObjectsOfType<WaypointNode>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 inputVector = Vector2.zero;

        // like an if statement, think SQL
        switch (aiMode)
        {
            case AIMode.followPlayer:
                FollowPlayer();
                break;
            
            case AIMode.followWaypoints:
                FollowWaypoints();
                break;
        }

        inputVector.x = TurnTowardTarget();
        inputVector.y = ApplyThrottleOrBrake(inputVector.x );

        topDownCarController.SetInputVector(inputVector);

    }

     float ApplyThrottleOrBrake(float inputX)
    {

        // apply gas depending on the amount the car needs to turn
        return 1.05f - Mathf.Abs(inputX) / 1.0f;
        
    }

    void FollowWaypoints()
    {
        if (currentWaypoint == null)
            currentWaypoint = FindClosestWayPoint();
        
        if (currentWaypoint != null)
        {
            targetPosition = currentWaypoint.transform.position;

            float distanceToWaypoint = (targetPosition - transform.position).magnitude;
            if(distanceToWaypoint <= currentWaypoint.minDistanceToReachWaypoint)
            {
                //If we are close enough then go to next waypoint, if there is multiple options a random one is selected

                currentWaypoint = currentWaypoint.nextWayPointNode[UnityEngine.Random.Range(0, currentWaypoint.nextWayPointNode.Length)];


            }
        }
    }

    // finds closest waypoint
    WaypointNode FindClosestWayPoint()
    {
        // Simular to Python SQL Integration librarys
        return allWayPoints
        .OrderBy(tag => Vector3.Distance(transform.position, tag.transform.position))
        .FirstOrDefault();
    }

    void FollowPlayer()
    {
        //Locates the player
        if (targetTransorm == null){
            targetTransorm = GameObject.FindGameObjectWithTag("Player").transform;
        }
          if (targetTransorm != null){
            targetPosition = targetTransorm.position;
        }

    }

    float TurnTowardTarget()
    {
        Vector2 vectorToTarget = targetPosition - transform.position;
        vectorToTarget.Normalize();

        //calculates the angle towards the target
        float angleToTarget = Vector2.SignedAngle(transform.up, vectorToTarget);
        angleToTarget *= -1;

    //  keeps steering smooth, makes the model not shaky
        float steerAmount = angleToTarget / 45.0f;

        // only allows steering values from -1 to 1
        steerAmount = Mathf.Clamp(steerAmount, -1.0f, 1.0f);
        return steerAmount;


    }
}
