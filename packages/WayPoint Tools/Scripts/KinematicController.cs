using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Auto adds the waypoint follower script - we'll need it so might as well enforce it
[RequireComponent(typeof(WayPointTracker))]
public class KinematicController : MonoBehaviour
{
    [Header("Moves the GameObject towards the next waypoint at maximum speed")]
    WayPointTracker wayPointTracker;
    [Tooltip("Movement speed (m/s)")]
    public float speed;
    [Tooltip("Sets orientation speed (degrees/s), does not affect movement")]
    public float rotationSpeed;
    [Tooltip("Toggles the orientation control for this controller")]
    public bool useLookPoints = true;

    [HideInInspector]
    public Vector3 targetPosition;
    [HideInInspector]
    public Vector3 targetLookPoint;


    // Start is called before the first frame update
    void Start()
    {
        // Get the waypoint tracker which keeps track of our current target waypoint
        wayPointTracker = GetComponent<WayPointTracker>();
        // No need to check for a null because RequireComponent ensures we always have a WayPointTracker
    }

    // We could have this in Update or FixedUpdate, doesn't really matter
    void Update()
    {
        // Get target position from waypoint tracker
        targetPosition = wayPointTracker.GetCurrentWayPoint();
        

        // Move towards target waypoint with velocity that has a magnitude of 'speed'
        // Don't forget to change to Time.fixedDeltaTime if moving to FixedUpdate
        transform.position += Time.deltaTime * speed * (targetPosition - transform.position).normalized;

        if (useLookPoints)
        {
            // Get the look point from the tracker
            targetLookPoint = wayPointTracker.GetCurrentLookPoint();
            // Look orientation
            Quaternion targetRotation = Quaternion.LookRotation(targetLookPoint - transform.position);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
