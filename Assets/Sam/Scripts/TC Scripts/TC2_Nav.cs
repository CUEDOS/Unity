using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TC2_Nav : MonoBehaviour
{
    private GameObject targets;
    private Transform target;   
    private int Tcurr = 0;       // Current target
    private int Tnum;            // Number of targets
    public Vector3 nextCorner;
    private float distanceToWP;

    private NavMeshAgent navMeshAgent;

    public float requiredDistance = 5f;

    // A point of potential confusion is the difference between waypoint and target, these are the definitions:
    // Target is the transform that the agent wants to make its way towards
    // Waypoints (or in this script prodominantlely the next waypoint) is the next transform the agent will go to on its way to the target.

    //                     Obstacle
    //   Target --- > x       [ ]     . + < --- Agent 
    //                   `.   [ ]   .'
    //                     `. [ ] .'
    //                        `W'
    //                         ^
    //                         ¦
    //                      Waypoint

    void Start()
    {
        // Initialize the navmesh agent and waypoint manager
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        targets = GameObject.FindGameObjectWithTag("Targets");

        // Find the number of targets and loads the current target
        Tnum = targets.transform.childCount;
        target = targets.transform.GetChild(Tcurr);

        // Set the destination to the next target.
        navMeshAgent.SetDestination(target.position);

        // Must disable navmesh from moving the agent 
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;

    }

    void Update()
    {
        // Check how close to the current target we are
        distanceToWP = Vector3.Distance(transform.position, targets.transform.GetChild(Tcurr).position);

        // If we are sufficiently close
        if (distanceToWP <= requiredDistance)
        {

            // Check if we have reached the end of the list (going both ways)
            if (Tcurr == Tnum)
            {
                // Move the counter back to where it was so we don't exceed the list's indecies
                Tcurr = 0;
                target = targets.transform.GetChild(Tcurr);
            }
            else
            {
                Tcurr += 1;
                target = targets.transform.GetChild(Tcurr);
            }

        }

        // Set the destination to the next target.
        navMeshAgent.SetDestination(target.position);

        // Finds the next waypoint and passes it to the waypoint manager
        nextCorner = navMeshAgent.path.corners[1];

        // Updates navmesh location as it doesn't inherently record
        navMeshAgent.nextPosition = transform.position;


    }

    private void OnDrawGizmosSelected()
    {
        try
        {
            Gizmos.DrawWireSphere(target.position, requiredDistance);
        }
        catch (System.Exception)
        {

            return;
        }
        
    }

}
