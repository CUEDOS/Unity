using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This might not be the best way to do things but it could help!
[RequireComponent(typeof(BasePhysics))]
public class TC_PController : MonoBehaviour
{
    public Vector3 waypoint;
    public float maxSpeed = 5f;
    public float velocityGain = 2f;
    public float headingGain;

    BasePhysics drone;
    TC2_Nav nav;


    // Start is called before the first frame update
    void Start()
    {
        drone = GetComponent<BasePhysics>();
        nav = GetComponent<TC2_Nav>();
    }

    void FixedUpdate()
    {
        waypoint = nav.nextCorner;

        drone.AddForce(velocityGain * (waypoint - transform.position));
        drone.AddHoverForce();

        if (drone.Velocity.magnitude > maxSpeed)
        {
            drone.Velocity = drone.Velocity.normalized * maxSpeed;
        }

        //Vector3 targetHeading = target - transform.position;
        //float headingError = Vector3.Angle(targetHeading, transform.forward);

        //drone.AddTorque(new Vector3(0, headingGain * headingError, 0));

    }
}
