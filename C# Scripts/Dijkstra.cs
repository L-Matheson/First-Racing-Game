using System;
using System.Collections.Generic;
using UnityEngine;

/*
**
   This Class is currently not working. It has been created by reading many Dijkstra algorithms and
   doing my best to create the algorithm inside of Unity. However, there is still problems to work
   out before it can be put into the game.  
**
*/
public class Dijkstra : MonoBehaviour
{
    public WaypointNode waypoint;

    private int numberOfWaypoints;
    private List<Tuple<int, int>>[] adjacencyList;

// is called when the unity enviornment is ran
    private void Start()
    {
         // Get all the waypoints in the scene and initialize their connections.
            WaypointNode[] allWaypoints = FindObjectsOfType<WaypointNode>();
            waypoint.InitializeAdjacencyList(allWaypoints);  // Initializes adjacency list based on connections
            adjacencyList = waypoint.GetAdjacencyList();    // Get the actual adjacency list

            numberOfWaypoints = adjacencyList.Length; // Set the number of waypoints (nodes) in the graph

            // prints all the connections between waypoints.
            Debug.Log("Adjacency List:");
            for (int i = 0; i < adjacencyList.Length; i++)
            {
                Debug.Log($"Waypoint {i}:");
                foreach (var connection in adjacencyList[i])
                {
                    Debug.Log($"   Connected to waypoint {connection.Item1} with distance {connection.Item2}");
                }
            }

            // Call the method to calculate the shortest path from the first waypoint (start point).
            CalculateShortestPath(0);
        
        else
        {
            // If there's no waypoint assigned, throw an error.
            Debug.LogError("Waypoint reference is not assigned.");
        }
    }

    public void CalculateShortestPath(int startWaypoint)
    {
        // Priority queue is used to get the next node with the smallest distance
        var priorityQueue = new PriorityQueue<int, int>();
        
        // This will hold the shortest distance from the startWaypoint to every other waypoint
        var shortestDistances = new int[numberOfWaypoints];

        // Initialize the distances to "infinity" (max int value) for all waypoints except the start.
        for (int i = 0; i < numberOfWaypoints; i++)
            shortestDistances[i] = int.MaxValue;

        // Start the algorithm by adding the start waypoint with distance 0 to the priority queue
        priorityQueue.Enqueue(Tuple.Create(0, startWaypoint));
        shortestDistances[startWaypoint] = 0;

        // While there are still waypoints to process
        while (priorityQueue.Count != 0)
        {
            // Dequeue the waypoint with the smallest distance
            var currentWaypoint = priorityQueue.Dequeue().Item2;

            // Go through each connected waypoint to see if we can find a shorter path
            foreach (var connection in adjacencyList[currentWaypoint])
            {
                int adjacentWaypoint = connection.Item1;
                int distanceToAdjacent = connection.Item2;

                // If we found a shorter path to this waypoint, update the distance and enqueue it
                if (shortestDistances[adjacentWaypoint] > shortestDistances[currentWaypoint] + distanceToAdjacent)
                {
                    shortestDistances[adjacentWaypoint] = shortestDistances[currentWaypoint] + distanceToAdjacent;
                    priorityQueue.Enqueue(Tuple.Create(shortestDistances[adjacentWaypoint], adjacentWaypoint));
                }
            }
        }

        // Output the shortest distances from the start waypoint to all other waypoints
        Debug.Log("Vertex Distance from Source:");
        for (int i = 0; i < numberOfWaypoints; ++i)
            Debug.Log($"Waypoint {i}: Distance {shortestDistances[i]}");
    }
}
