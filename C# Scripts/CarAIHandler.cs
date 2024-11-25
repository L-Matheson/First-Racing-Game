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

        inputVector.x = TurnTowardTarget();
        inputVector.y = ApplyThrottleOrBrake(inputVector.x );

        topDownCarController.SetInputVector(inputVector);
    }

     float ApplyThrottleOrBrake(float inputX)
    {

        // applies gas depending on the amount the car needs to turn. 
        // Ensures car doesn't constantly drift around nodes of the graph
        return 1.05f - Mathf.Abs(inputX) / 1.0f;
    }

    void FollowWaypoints()
    {   
        // Base Case, checks the model has a current node it has visited. Allows for the model to see the next nodes
        if (currentWaypoint == null)
            currentWaypoint = FindClosestWayPoint();
        
        if (currentWaypoint != null)
        {
            // Locates the next node in the directed graph
            targetPosition = currentWaypoint.transform.position;
            float distanceToWaypoint = (targetPosition - transform.position).magnitude;
            if(distanceToWaypoint <= currentWaypoint.minDistanceToReachWaypoint)
            {
                //If we are close enough then go to next waypoint, if there is multiple options a random one is selected
                // This is to be expanded on, possible DFS method to be integrated here. 
                currentWaypoint = currentWaypoint.nextWayPointNode[UnityEngine.Random.Range(0, currentWaypoint.nextWayPointNode.Length)];
            }
        }
    }

    // Finds closest waypoint
    WaypointNode FindClosestWayPoint()
    {
        // Formated simalar to Python SQL Integration librarys, uses .Linq
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
        // Calculates the distance between the car and the target
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
