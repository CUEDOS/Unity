using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TC_WaypointFollower : MonoBehaviour
{
    public float requiredDistance = 0.25f;

    [Tooltip("Stop getting waypoints upon reaching the end of the list")]
    public bool stopAtEnd = false;

    [Tooltip("Waypoints create a loop, reaching the end of the list will start again. If false, waypoints will be followed in reverse upon reaching the end.")]
    public bool waypointsAreLoop = true;

    public GameObject waypointManager;

    WayPointManager wayPoints;
    int currentWP = 0;
    bool goingBack = false;
    bool done = false;

    void Start()
    {
        // Get the waypoint manager from the scene
        wayPoints = waypointManager.GetComponent<WayPointManager>();

        // Make sure we found a manager
        if (wayPoints == null)
        {
            Debug.LogError("No waypoint manager found.");
        }
    }

    void FixedUpdate()
    {
        if (!done)
        {
            // Check how close to the current target we are
            float distanceToWP = Vector3.Distance(transform.position, wayPoints.WayPoints[currentWP]);

            // If we are sufficiently close
            if (distanceToWP <= requiredDistance)
            {
                // Add or subtract one to the target waypoint
                currentWP += goingBack ? -1 : 1;

                // Check if we have reached the end of the list (going both ways)
                if (currentWP >= wayPoints.WayPoints.Count || currentWP < 0)
                {
                    // Move the counter back to where it was so we don't exceed the list's indecies
                    currentWP -= goingBack ? -1 : 1;

                    // If we want to stop at the end of the list
                    if (stopAtEnd)
                    {
                        // Set done and this whole thing will never be called again
                        done = true;
                    }
                    // Otherwise, if the waypoints are NOT a loop
                    else if (!waypointsAreLoop)
                    {
                        // We want to reverse the direction of travel through the waypoints list
                        goingBack = !goingBack;
                    }
                    else
                    {
                        // Reaching here means the waypoints make a loop so just start at the beginning again
                        currentWP = 0;
                    }
                }
            }
        }
    }

    // The key to this class, this will pass the current waypoint to a controller
    // NOTE: As this is a position it can also be passed as the target for a NavAgent
    public Vector3 GetCurrentWayPoint()
    {
        return wayPoints.WayPoints[currentWP];
    }
}
