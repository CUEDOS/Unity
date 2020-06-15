using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;


public class Flock5 : MonoBehaviour
{
    public float SOIRadius = 25f;
    //float conflictRadius = 10f;

    Vector3 totalForce;

    Vector3 alignmentForce;
    Vector3 cohesionForce;
    Vector3 separationForce;
    Vector3 migrationForce;
    Vector3 orthSepForce;

    public float alignmentForceMag;
    public float cohesionForceMag;
    public float separationForceMag;
    public float orthSepForceMag;

    public float alignmentGain;
    public float cohesionGain;
    public float separationGain;
    public float migrationGain;
    public float orthSepGain;

    public float a;           // Repulsion force magnitude 20
    public float c;              // Repulsion force peak width 2.5

    public float speed;
    public float maxForce;
    public float flockingInterval;

    Rigidbody rb;

    private List<GameObject> neighbours;
    private List<GameObject> conflicts;

    public GameObject destination;
    LayerMask droneMask;
    LayerMask obstacleMask;
    public Collider[] neighbourCol;
    public bool destroyOnArrival;

    private Vector3 workingVec;
    private Vector3 neightbourVel;
    private float diff;

    private void Start()
    {
        droneMask = LayerMask.GetMask("Drones");
        obstacleMask = LayerMask.GetMask("Obstacles");

        neighbours = new List<GameObject>();

        conflicts = new List<GameObject>();


        rb = GetComponentInParent<Rigidbody>();

        InvokeRepeating("Flocking", 1f, flockingInterval);  //1s delay, repeat every 1s
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        CollisionManager.NewConflict(gameObject, collision.gameObject);
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, destination.transform.position) < 10f && destroyOnArrival) gameObject.Destroy();

        migrationForce = Vector3.Normalize(destination.transform.position - transform.position) * maxForce;

        // Add Flocking Force
        //totalForce = alignmentForce * alignmentGain + cohesionForce * cohesionGain + separationForce * separationGain + migrationForce * migrationGain;
        totalForce = alignmentForce * alignmentGain + cohesionForce * cohesionGain + separationForce * separationGain + orthSepForce * orthSepGain;
        float forceLeft = maxForce - totalForce.magnitude;

        if (forceLeft > 0) totalForce += migrationForce.normalized * migrationGain * forceLeft;

        rb.AddForce(totalForce.normalized * Mathf.Clamp(totalForce.magnitude, 0f, maxForce));

        // Cap speed to max speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, speed);

    }

    void Flocking()
    {
        neighbours.Clear();

        neighbourCol = Physics.OverlapCapsule(transform.position, rb.velocity.normalized * SOIRadius + transform.position, SOIRadius); // Returns colliders

        foreach (Collider neighbour in neighbourCol)
        {
            if (neighbour.transform != transform && neighbour.tag == "UAV")
            {
                neighbours.Add(neighbour.gameObject);
            }

        }

        if (neighbours.Count > 0)
        {
            // Total force
            totalForce = Vector3.zero;

            // Alignment & Orthoganol Sep

            //Alignment
            Vector3 VelocitySum = rb.velocity;
            int num = 1;

            // Orthoganal
            orthSepForce = Vector3.zero;

            // Cohesion to low variance

            Vector3 POI = Vector3.zero;


            foreach (var neighbour in neighbours)
            {
                neightbourVel = neighbour.GetComponent<Rigidbody>().velocity;
                VelocitySum += neightbourVel;
                num += 1;

                orthSepForce += a * Mathf.Exp(-Mathf.Pow(Vector3.Distance(transform.position + rb.velocity, neighbour.transform.position + neightbourVel), 2f) / 
                    (2 * Mathf.Pow(c, 2f))) * Vector3.Cross(rb.velocity, neightbourVel).normalized;

                diff = (rb.velocity - neightbourVel).magnitude;

                POI += (neighbour.transform.position - transform.position) / diff;
                

            }

            
            alignmentForce = VelocitySum / num;
            alignmentForceMag = alignmentForce.magnitude * alignmentGain;

            orthSepForceMag = orthSepForce.magnitude * orthSepGain;


            cohesionForce = POI / num;
            cohesionForceMag = cohesionForce.magnitude * cohesionGain;


            // Separation

            separationForce = Vector3.zero;

            foreach (Collider col in neighbourCol)
            {
                Vector3 sepVec = col.ClosestPoint(transform.position) - transform.position;
                float dist = sepVec.magnitude;
                Vector3 repDir = -sepVec.normalized;

                separationForce += a * Mathf.Exp(-Mathf.Pow(dist, 2) / (2 * Mathf.Pow(c, 2))) * repDir;

            }

            separationForceMag = separationForce.magnitude * separationGain;

        }
        else
        {
            alignmentForce = Vector3.zero;
            cohesionForce = Vector3.zero;
            separationForce = Vector3.zero;
            orthSepForce = Vector3.zero;
        }
    }



    private void OnDrawGizmosSelected()
    {

        //if (neighbours.Count > 0)
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawWireSphere(transform.position, SOIRadius);
        //}
        //else
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireSphere(transform.position, SOIRadius);
        //}
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + totalForce.normalized * 2);
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + alignmentForce.normalized * alignmentForceMag);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + cohesionForce.normalized * cohesionForceMag);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + separationForce.normalized * separationForceMag);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + orthSepForce.normalized * orthSepForceMag);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + migrationForce);


    }
}
