using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Pursuer))]
public class SimpleCityNav : MonoBehaviour
{
    // Variables 
    
    Pursuer pursuer;
    Rigidbody rb;

    Vector3 target;
    Vector3 startTakeOffPoint;
    Vector3 destinationLandingPoint;

    [Header ("Magnitude multiplied by max acceleration")]
    public float forceMag;
    private float speed;
    private float takeOffAcceleration = 1f;
    private float landingAcceleration = 0.1f;
    private float maxAcceleration = 50f;

    private bool returning = false;
    private bool simpleNav = true;


    private delegate void Stage();
    private Stage currentStage;

    private List<Stage> methods;
    private int methodInd = 0;

    private int wpIndex;
    private List<Vector3> finalPath;
    Vector3 direction;
    Vector3 velocity;

    void Start()
    {
        pursuer = GetComponent<Pursuer>();
        rb = GetComponent<Rigidbody>();
        rb.AddForce(Vector3.forward);
        speed = SimManager.expVars["Speed"];

        startTakeOffPoint = transform.position + Vector3.up * 5f;

        if (simpleNav)
        {
            methods = new List<Stage> { Initialise, WaitForPath, FollowPath };
        }
        else
        {
            methods = new List<Stage> { Initialise, WaitForPath, TakeOff, FollowPath, FindLanding, Landing};
        }



        target = AssignTarget();

        currentStage = NextMethod();
    }
    void FixedUpdate()
    {
        if (currentStage != null)
        {
            currentStage.Invoke();
        }

    }


    private Vector3 AssignTarget()
    {
        List<GameObject> Destinations = new List<GameObject>();

        // Finding all the destinations on the map using the tag 'Destinations' and adding it to a list for random selection
        foreach (GameObject dest in GameObject.FindGameObjectsWithTag("Destinations"))
        {
            Destinations.Add(dest);
        }
        int index = UnityEngine.Random.Range(0, Destinations.Count);

        return(Destinations[index].transform.position + Vector3.up * 10);
    }

    private void Initialise()
    {
        // pursuer plans path from 5m above current pos
        pursuer.pathPostTakeOff = true;
        // pursuer doesn't use internal movement method
        pursuer.movement = false;

        if (returning) pursuer.MoveTo(startTakeOffPoint); else pursuer.MoveTo(target);
        currentStage = NextMethod();
    }
    private void WaitForPath()
    {
        if(pursuer.GetCurCondition() == "Movement")
        {
            finalPath = pursuer.GetFinalPath();
            currentStage = NextMethod();
        }
        // If failed, try again
        if(pursuer.GetCurCondition() == "WaitingForRequest")
        {
            methodInd = 0;
            currentStage = NextMethod();
        }
    }

    private void TakeOff()
    {
        if ((startTakeOffPoint + Vector3.up * 5).y < transform.position.y)
        {
            currentStage = NextMethod();
            return;
        }
        direction = (startTakeOffPoint + Vector3.up * 5 - transform.position).normalized;
        rb.AddForce(takeOffAcceleration * direction);

    }

    private void FollowPath()
    {
        float distance = Vector3.Distance(transform.position, finalPath[wpIndex]);
        if (distance < 5f)
        {
            wpIndex++;

            if (wpIndex == finalPath.Count)
            {
                pursuer.ResetCondition();

                if (simpleNav && returning) DestroyImmediate(gameObject);

                currentStage = NextMethod();
                wpIndex = 0;
            }
            return;
        }

        direction = (finalPath[wpIndex] - transform.position).normalized;

        // Force is appllied proportional to the magnitude of the current velocity vector and the direction to the next waypoint. 
        // If the drone is travelling in the opposite direction, maximum force is applied. If the drone is travvelling in the correct direction with the correct speed, the force is zero.
        // To check tests, use TC4.

        velocity = rb.velocity;
        forceMag = 1 - Mathf.Pow((Vector3.Dot(velocity, direction) / velocity.magnitude + 1) / 2, 2) * rb.velocity.magnitude / speed;
        if (float.IsNaN(forceMag))
        {
            rb.AddForce(direction);
        }
        else
        {
            rb.AddForce(maxAcceleration * direction * forceMag, ForceMode.Acceleration);
        }
    }

    private void FindLanding()
    {
        RaycastHit Hit;
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out Hit))
        {
            destinationLandingPoint = transform.position + transform.TransformDirection(Vector3.down) * Hit.distance ;
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * Hit.distance, Color.yellow);
            currentStage = NextMethod();
            Debug.Log(destinationLandingPoint);
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 10, Color.white);
        }
    }
    
    private void Landing()
    {
        // Checking if landed
        if ((destinationLandingPoint + Vector3.up * 0.5f).y > transform.position.y)
        {
            if (returning)
            {
                DestroyImmediate(gameObject);
                return;
            }
            
            currentStage = NextMethod();
            return;
        }
        direction = (destinationLandingPoint + Vector3.up * 0.5f - transform.position).normalized;
        rb.AddForce(landingAcceleration * direction);
    }

    private Stage NextMethod()
    {
        methodInd++;
        if (methodInd <= methods.Count)
        {
            return methods[methodInd-1];
        }
        else
        {
            methodInd = 1;
            returning = true;
            return methods[0];
        }
    }

}