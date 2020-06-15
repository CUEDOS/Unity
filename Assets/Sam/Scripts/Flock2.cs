using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Flock2 : MonoBehaviour
{
    //-------------Curve Plotting---------------
    //// To plot curves from debugger
    //public AnimationCurve plot = new AnimationCurve();

    ////In update()

    //float value = ...;
    //plot.AddKey(Time.realtimeSinceStartup, value);

    public float SOIRadius = 25f;
    public float conflictRadius = 10f;

    private Vector3 totalForce;

    private Vector3 alignmentForce;
    private Vector3 cohesionForce;
    private Vector3 separationForce;

    public float alignmentForceMag;
    public float cohesionForceMag;
    public float separationForceMag;

    public float alignmentGain = 1;
    public float cohesionGain = 0;
    public float separationGain = 1;

    public float a = 5;           // Repulsion force magnitude
    public int c = 1;              // Repulsion force peak width

    public float maxForce = 5;

    Rigidbody rb;

    private List<GameObject> neighbours;
    private List<GameObject> conflicts;

    private bool initialization;
    private float citySize;
    


    private void Start()
    {
        // Make sure the drone has a sphere collider
        SphereCollider FlockCollider = gameObject.AddComponent<SphereCollider>();
        // Set the range and make the collider a trigger 
        // triggers don't generate collision forces but still detect collisions
        FlockCollider.radius = SOIRadius;
        FlockCollider.isTrigger = true;

        neighbours = new List<GameObject>();

        conflicts = new List<GameObject>();

        rb = GetComponentInParent<Rigidbody>();

        InvokeRepeating("Flocking", 1f, 1f);  //1s delay, repeat every 1s

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

        if (!transform.IsChildOf(obj.transform))
        {
            neighbours.Add(obj.gameObject);
        }

    }

    private void OnTriggerExit(Collider obj)
    {
        if (neighbours.Contains(obj.gameObject))
        {
            neighbours.Remove(obj.gameObject);
        }

        if (conflicts.Contains(obj.gameObject))
        {
            conflicts.Remove(obj.gameObject);
        }
    }


    void FixedUpdate()
    {
        // House Keeping
        neighbours.RemoveAll(p => p == null);
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
                // If outside the city confines, we ignore this conflict
                if (Mathf.Abs(transform.position.x) > citySize / 2 || Mathf.Abs(transform.position.z) > citySize / 2) continue;

                conflicts.Add(drone);
                ConflictManager.NewConflict(transform.parent.gameObject, drone.transform.parent.gameObject);
            }
        }

    }

    void Flocking()
    {
        neighbours.RemoveAll(p => p == null);

        foreach (GameObject drone in neighbours)
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
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + totalForce.normalized * 2);
        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, transform.position + alignmentForce.normalized * alignmentForceMag);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + cohesionForce.normalized * cohesionForceMag);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + separationForce.normalized * separationForceMag);


    }

}
