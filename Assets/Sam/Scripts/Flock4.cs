using Microsoft.VisualBasic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Flock4 : MonoBehaviour
{
    float SOIRadius = 25f;
    float conflictRadius = 10f;

    Vector3 totalForce;

    Vector3 alignmentForce;
    Vector3 cohesionForce;
    Vector3 separationForce;
    Vector3 migrationForce;

    float alignmentForceMag;
    float cohesionForceMag;
    float separationForceMag;

    float alignmentGain = 0.1f;
    float cohesionGain = 0.1f;
    float separationGain = 1f;
    float migrationGain = 1f;

    float a = 20;           // Repulsion force magnitude
    float c = 2.5f;              // Repulsion force peak width

    public float speed;
    public float maxForce;

    Rigidbody rb;

    private List<GameObject> neighbours;
    private List<GameObject> conflicts;

    public GameObject destination;
    public LayerMask droneMask;
    public LayerMask obstacleMask;
    public Collider[] neighbourCol;

    private Vector3 workingVec;



    private void Start()
    {
        //droneMask = LayerMask.GetMask("Drones");

        neighbours = new List<GameObject>();

        conflicts = new List<GameObject>();



        rb = GetComponentInParent<Rigidbody>();

        InvokeRepeating("Flocking", 1f, 0.1f);  //1s delay, repeat every 1s

    }

    void FixedUpdate()
    {
        conflicts.RemoveAll(p => p == null);
        conflicts.RemoveAll(p => Vector3.Distance(p.transform.position, transform.position) > conflictRadius);

        migrationForce = Vector3.Normalize(destination.transform.position - transform.position) * maxForce;

        // Add Flocking Force
        //totalForce = alignmentForce * alignmentGain + cohesionForce * cohesionGain + separationForce * separationGain + migrationForce * migrationGain;
        totalForce = alignmentForce * alignmentGain + cohesionForce * cohesionGain + separationForce * separationGain;
        float forceLeft = maxForce - totalForce.magnitude;

        if (forceLeft > 0) totalForce += migrationForce.normalized * migrationGain * forceLeft; 

        rb.AddForce(totalForce.normalized * Mathf.Clamp(totalForce.magnitude, 0f, maxForce));

        // Conflict detection
        foreach (GameObject drone in neighbours)
        {
            // Within conflictRadius and not already in the known conflict catalogue
            if (Vector3.Distance(transform.position, drone.transform.position) < conflictRadius && !conflicts.Contains(drone))
            {
                conflicts.Add(drone);
                //ConflictManager.NewConflict(transform.parent.gameObject, drone.transform.parent.gameObject);
            }
        }

    }

    void Flocking()
    {
        neighbours.Clear();

        neighbourCol = Physics.OverlapCapsule(transform.position, rb.velocity.normalized * SOIRadius + transform.position, SOIRadius, droneMask);

        foreach (Collider drone in neighbourCol)
        {
            workingVec = drone.transform.position - transform.position;
            if (drone.name != name && !Physics.Raycast(transform.position, workingVec, Vector3.Magnitude(workingVec), ~droneMask))
            {
                //Debug.DrawRay(transform.position, drone.transform.position - transform.position, Color.red, 0.1f);
                neighbours.Add(drone.gameObject);
            }

        }

        if (neighbours.Count > -1)
        {
            totalForce = Vector3.zero;

            // Alignment

            Vector3 VelocitySum = rb.velocity;
            int num = 1;

            for (int i = 0; i < neighbours.Count; i++)
            {

                VelocitySum += neighbours[i].GetComponent<Rigidbody>().velocity;
                num += 1;

            }

            alignmentForce = VelocitySum / num;
            alignmentForceMag = alignmentForce.magnitude * alignmentGain;

            //if (alignmentForceMag > maxForce)
            //{
            //    alignmentForce = alignmentForce.normalized * maxForce;
            //    alignmentForceMag = alignmentForce.magnitude * alignmentGain;
            //}

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

            //if (cohesionForceMag > maxForce)
            //{
            //    cohesionForce = cohesionForce.normalized * maxForce;
            //    cohesionForceMag = cohesionForce.magnitude * cohesionGain;
            //}

            // Separation

        
            neighbourCol = Physics.OverlapCapsule(transform.position, rb.velocity.normalized * SOIRadius + transform.position, SOIRadius);


            separationForce = Vector3.zero;

            foreach (Collider col in neighbourCol)
            {
                Vector3 sepVec = col.ClosestPoint(transform.position) - transform.position;
                float dist = sepVec.magnitude;
                Vector3 repDir = -sepVec.normalized;

                separationForce += a * Mathf.Exp(-Mathf.Pow(dist, 2) / (2 * Mathf.Pow(c, 2))) * repDir;

            }

            separationForceMag = separationForce.magnitude * separationGain;

            //if (separationForceMag > maxForce)
            //{
            //    separationForce = separationForce.normalized * maxForce;
            //    separationForceMag = separationForce.magnitude * separationGain;
            //}

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
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + migrationForce.normalized * migrationForce.magnitude);

    }
}
