using System;
using System.Collections.Generic;
using UnityEngine;

public class WaypointNode : MonoBehaviour
{
    [Header("This points to the next node")]
    public float maxSpeed = 0;
    public float minDistanceToReachWaypoint = 5;

    public WaypointNode[] nextWayPointNode;

    // Array to store the distances to the next waypoints
    public float[] distancesToNextNodes;

    // Private adjacency list
    private List<Tuple<int, int>>[] adj;

    private void Awake()
    {
        // Initialize the distances array to match the length of nextWayPointNode
        distancesToNextNodes = new float[nextWayPointNode.Length];
        // Calculate the distances
        for (int i = 0; i < nextWayPointNode.Length; i++)
        {
            if (nextWayPointNode[i] != null)
            {
                distancesToNextNodes[i] = Vector3.Distance(transform.position, nextWayPointNode[i].transform.position);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < nextWayPointNode.Length; i++)
        {
            if (nextWayPointNode[i] != null)
            {
                Gizmos.DrawLine(transform.position, nextWayPointNode[i].transform.position);
            }
        }
    }

    // Creates an Adj matrix fo 
    public void InitializeAdjacencyList(WaypointNode[] allWaypoints)
    {
        // Initialize the adj
        adj = new List<Tuple<int, int>>[allWaypoints.Length];

        for (int i = 0; i < allWaypoints.Length; i++)
        {
            adj[i] = new List<Tuple<int, int>>(); // Initialize each list

            WaypointNode currentNode = allWaypoints[i];
            for (int j = 0; j < currentNode.nextWayPointNode.Length; j++)
            {
                if (currentNode.nextWayPointNode[j] != null)
                {
                    int neighborIndex = Array.IndexOf(allWaypoints, currentNode.nextWayPointNode[j]);
                    if (neighborIndex != -1)
                    {
                        adj[i].Add(Tuple.Create(neighborIndex, Mathf.RoundToInt(currentNode.distancesToNextNodes[j])));
                    }
                }
            }
        }
    }

    // allows the list to be grabbed by other classes
    public List<Tuple<int, int>>[] GetAdjacencyList()
    {
        return adj;
    }
}
