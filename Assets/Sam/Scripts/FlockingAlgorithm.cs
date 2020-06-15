using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(BasePhysics))]
public class FlockingAlgorithm : MonoBehaviour
{
    //-------------Curve Plotting---------------
    //// To plot curves from debugger
    //public AnimationCurve plot = new AnimationCurve();

    ////In update()

    //float value = ...;
    //plot.AddKey(Time.realtimeSinceStartup, value);

    public float SOIRadius = 20f;

    public GameObject[] drones;
    public List<GameObject> neighbours;

    BasePhysics agent;
    TC2_Nav nav;

    private Vector3 waypoint;

    private Vector3 totalForce;

    private Vector3 migrationForce;
    private Vector3 alignmentForce;
    private Vector3 cohesionForce;
    private Vector3 separationForce;

    public float migrationForceMag;
    public float alignmentForceMag;
    public float cohesionForceMag;
    public float separationForceMag;

    public float alignmentGain = 0;
    public float cohesionGain = 0;
    public float separationGain = 1;
    public float migrationGain = 1;

    public float a = 5;           // Repulsion force magnitude
    public int c = 1;              // Repulsion force peak width
    public float maxVelocity = 2;
    public float maxForce = 10;



    private void Start()
    {
        agent = GetComponent<BasePhysics>();

        nav = GetComponent<TC2_Nav>();

        drones = GameObject.FindGameObjectsWithTag("Drone");

        InvokeRepeating("Flocking", 1f, 1f);  //1s delay, repeat every 1s

    }

    void FixedUpdate()
    {

        // Migration

        waypoint = nav.nextCorner;

        migrationForce = waypoint - transform.position;
        migrationForceMag = migrationForce.magnitude * migrationGain;

        if (migrationForceMag > maxForce)
        {
            migrationForce = migrationForce.normalized * maxForce;
            migrationForceMag = migrationForce.magnitude * migrationGain;
        }


        totalForce = alignmentForce * alignmentGain + cohesionForce * cohesionGain + separationForce * separationGain + migrationForce * migrationGain;

        agent.AddForce(totalForce);
        agent.AddHoverForce();

        if (agent.Velocity.magnitude > maxVelocity)
        {
            agent.Velocity = agent.Velocity.normalized * maxVelocity;
        }


    }

    void Flocking()
    {
        neighbours.Clear();

        foreach (GameObject drone in drones)
        {
            if (Vector3.Distance(drone.transform.position, transform.position) < SOIRadius && drone.name != name)
            {
                neighbours.Add(drone);
            }
        }

        if (neighbours.Count > 0)
        {
            totalForce = Vector3.zero;

            // Alignment

            Vector3 VelocitySum = agent.Velocity;
            int num = 1;

            for (int i = 0; i < neighbours.Count; i++)
            {

                VelocitySum += neighbours[i].GetComponent<BasePhysics>().Velocity;
                num += 1;

            }

            alignmentForce = VelocitySum / num;
            alignmentForceMag = alignmentForce.magnitude * alignmentGain;

            if (alignmentForceMag > maxForce)
            {
                alignmentForce = alignmentForce.normalized * maxForce;
                alignmentForceMag = alignmentForce.magnitude * alignmentGain;
            }

            // Cohesion

            Vector3 CoM = transform.position;
            num = 1;

            for (int i = 0; i < neighbours.Count; i++)
            {

                CoM += neighbours[i].transform.position;
                num += 1;

            }

            CoM = CoM / num;
            cohesionForce = CoM - transform.position;
            cohesionForceMag = cohesionForce.magnitude * cohesionGain;

            if (cohesionForceMag > maxForce)
            {
                cohesionForce = cohesionForce.normalized * maxForce;
                cohesionForceMag = cohesionForce.magnitude * cohesionGain;
            }

            // Separation

            separationForce = Vector3.zero;

            for (int i = 0; i < neighbours.Count; i++)
            {
                Vector3 sepVec = neighbours[i].transform.position - transform.position;
                float dist = sepVec.magnitude;
                Vector3 repDir = -sepVec.normalized;

                separationForce += a * Mathf.Exp(-Mathf.Pow(dist, 2) / (2 * Mathf.Pow(c, 2))) * repDir;

            }

            separationForceMag = separationForce.magnitude * separationGain;

            if (separationForceMag > maxForce)
            {
                separationForce = separationForce.normalized * maxForce;
                separationForceMag = separationForce.magnitude * separationGain;
            }


        }
        else
        {
            alignmentForce = Vector3.zero;
            cohesionForce = Vector3.zero;
            separationForce = Vector3.zero;
        }
    }



    private void OnDrawGizmosSelected()
    {
        
        if (neighbours.Count > 0)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, SOIRadius);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, SOIRadius);
        }
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, transform.position + totalForce.normalized * 2);
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + alignmentForce);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + cohesionForce);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + separationForce);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + migrationForce);


    }

}
