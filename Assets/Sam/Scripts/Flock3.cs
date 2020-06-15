using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Flock3 : MonoBehaviour
{
    float SOIRadius = 25f;
    float conflictRadius = 10f;

    Vector3 totalForce;

    Vector3 alignmentForce;
    Vector3 cohesionForce;
    Vector3 separationForce;

    float alignmentForceMag;
    float cohesionForceMag;
    float separationForceMag;

    float alignmentGain = 1;
    float cohesionGain = 0;
    float separationGain = 1;

    float a = 5;           // Repulsion force magnitude
    int c = 1;              // Repulsion force peak width

    public float maxForce;

    Rigidbody rb;

    private List<GameObject> neighbours;
    private List<GameObject> conflicts;

    private Pooler pools;

    private void Start()
    {
    
        neighbours = new List<GameObject>();

        conflicts = new List<GameObject>();

        rb = GetComponentInParent<Rigidbody>();

        InvokeRepeating("Flocking", 1f, 1f);  //1s delay, repeat every 1s

        pools = Pooler.Instance;

    }

    void FixedUpdate()
    { 
        neighbours = pools.activeAgents;
        conflicts.RemoveAll(p => p == null);
        conflicts.RemoveAll(p => Vector3.Distance(p.transform.position, transform.position) > conflictRadius);

        // Add Flocking Force
        totalForce = alignmentForce * alignmentGain + cohesionForce * cohesionGain + separationForce * separationGain;
        rb.AddForce(totalForce.normalized * Mathf.Clamp(totalForce.magnitude, 0, maxForce));

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

        foreach (GameObject drone in pools.activeAgents)
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

            Vector3 VelocitySum = rb.velocity;
            int num = 1;

            for (int i = 0; i < neighbours.Count; i++)
            {

                VelocitySum += neighbours[i].GetComponentInParent<Rigidbody>().velocity;
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



    //private void OnDrawGizmosSelected()
    //{

    //    if (neighbours.Count > 0)
    //    {
    //        Gizmos.color = Color.red;
    //        Gizmos.DrawWireSphere(transform.position, SOIRadius);
    //    }
    //    else
    //    {
    //        Gizmos.color = Color.green;
    //        Gizmos.DrawWireSphere(transform.position, SOIRadius);
    //    }
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawLine(transform.position, transform.position + totalForce.normalized * 2);
    //    Gizmos.color = Color.black;
    //    Gizmos.DrawLine(transform.position, transform.position + alignmentForce.normalized * alignmentForceMag);
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawLine(transform.position, transform.position + cohesionForce.normalized * cohesionForceMag);
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawLine(transform.position, transform.position + separationForce.normalized * separationForceMag);


    //}

}
