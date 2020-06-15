using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataCollector : MonoBehaviour
{

    public float density;
    private List<Vector3> velocities;
    public float velocityVariance;
    public float speedMean;

    Pooler pooler;

    void Start()
    {
        velocities = new List<Vector3>();
        pooler = Pooler.Instance;
    }

    void FixedUpdate()
    {
        velocities.Clear();
        foreach (GameObject obj in pooler.activeAgents)
        {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            velocities.Add(rb.velocity);
        }

        speedMean = 0f;
        foreach (Vector3 vec in velocities)
        {
            speedMean += vec.magnitude;
        }

        speedMean /= velocities.Count;

        velocityVariance = 0f;
        foreach (Vector3 vec in velocities)
        {
            velocityVariance += Mathf.Pow((vec.magnitude - speedMean), 2);
        }
        

        density = velocities.Count / (4 / 3 * Mathf.PI * Mathf.Pow(50f, 3));
    }
}
