using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConflictDetector : MonoBehaviour
{
    public float detectionRange = 5f;
    public int conflictCounter = 0;

    // This is just to have the drone moving for example purposes
    public Vector3 velocity;

    // Give the drone all the required components - you'll want this to be done as a prefab so there's no
    // overhead to spawning your drones!
    void Start()
    {
        // We're using a sphere collider set to be a trigger to detect when another drone enters our sphere
        SphereCollider collider = GetComponent<SphereCollider>();
        if(collider == null)
        {
            // Make sure the drone has a sphere collider
            collider = gameObject.AddComponent<SphereCollider>();
        }
        // Set the range and make the collider a trigger 
        // triggers don't generate collision forces but still detect collisions
        collider.radius = detectionRange;
        collider.isTrigger = true;

        // To use the trigger we need a Kinematic RigidBody
        // this should work with the transform movement of the pursuer script
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
    }

    // OnTriggerEnter is called whenever a collider enters our trigger collider i.e. the sphere around this drone
    void OnTriggerEnter(Collider other)
    {
        if (other.transform != transform)
        {
            conflictCounter++;
        }
        //// Collider gives us access to tag which is super quick
        //if (other.tag == "Drone")
        //{
        //    conflictCounter++;
        //}
        
    }


}
