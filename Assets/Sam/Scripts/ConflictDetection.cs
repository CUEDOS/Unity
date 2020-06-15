using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConflictDetection : MonoBehaviour
{

    public float SOIRadius = 10f;

    bool initialization = true;
    private float citySize;


    void Start()
    {
        // Make sure the drone has a sphere collider
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();

        // Set the range and make the collider a trigger 
        // triggers don't generate collision forces but still detect collisions
        collider.radius = SOIRadius;
        collider.isTrigger = true;


        // To use the trigger we need a Kinematic RigidBody
        // this should work with the transform movement of the pursuer script
        //Rigidbody rb = GetComponent<Rigidbody>();
        //if (rb == null)
        //{
        //    rb = gameObject.AddComponent<Rigidbody>();
        //}
        //rb.isKinematic = true;

        citySize = SimManager.expVars["CitySize"];

    }

    void OnTriggerEnter(Collider obj)
    {
        // Collision is detected when drone is initiallised
        if (initialization)
        {
            initialization = false;
            return;
        }
        // If not in the area of interest
        if (Mathf.Abs(transform.position.x) > citySize/2 || Mathf.Abs(transform.position.z) > citySize / 2) return;

        if (obj.transform != transform.parent.transform)
        {
            ConflictManager.NewConflict(transform.parent.gameObject, obj.transform.gameObject);
        }


    }

}
