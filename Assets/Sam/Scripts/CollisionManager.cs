using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
//https://gist.github.com/atandrau/847214/882418ab34737699a6b1394d3a28c66e2cc0856f


public class CollisionManager : MonoBehaviour
{
    public static Dictionary<List<GameObject>, float> reports;
    public static int totalConflicts;
    public Vector3 box;
    private Vector3 origin;
    public Collider[] localColls;
    public Vector3[] velocities;
    public float density;
    private static int lastCollisions;
    private int StatsPeriod = 1;
    double[,] covarianceMatrix;
    public double variance;
    public float collisionFrequency;
    public float time;
    private static float startTime;

    public LayerMask droneMask;

    private void Start()
    {
        reports = new Dictionary<List<GameObject>, float>();

        InvokeRepeating("Stats", 1f, StatsPeriod);

        origin = new Vector3(0f, box.y, 0f);

        startTime = Time.time;

    }


    public void Stats()
    {

        localColls = Physics.OverlapBox(origin, box, Quaternion.identity, droneMask);
        density = localColls.Length / (8 * box.x * box.y * box.z);

        collisionFrequency = (totalConflicts - lastCollisions) / StatsPeriod;
        lastCollisions = totalConflicts;



        if (localColls.Length > 1)
        {
            getCovarianceMatrix(localColls);
            variance = 0f;
            for (int i = 0; i < 3; i++)
            {
                variance += covarianceMatrix[i, i];
            }

        }

        time = Time.time-startTime;

    }
    public static void NewConflict(GameObject reportee, GameObject reported)
    {
        foreach (KeyValuePair<List<GameObject>, float> pair in reports)
        {
            if (pair.Key.Contains(reported) && pair.Key.Contains(reportee))
            {
                // Already been reported, no need to continue
                return;
            }
        }

        List<GameObject> newPair = new List<GameObject> {reported, reportee};
        reports.Add(newPair, Time.time);
        totalConflicts++;

        // Housekeeping - any entries that have been there for longer than 10 secs gets removed
        foreach (var item in reports.Where(kvp => Time.time - kvp.Value > 10f).ToList())
        {
            reports.Remove(item.Key);
        }

    }

    private void OnDrawGizmosSelected()
    {
        origin = new Vector3(0f, box.y, 0f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(origin, box * 2);
    }

    void getVelocities(Collider[] colliders)
    {
        velocities = new Vector3[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            velocities[i] = colliders[i].GetComponent<Rigidbody>().velocity;
        }

    }

    void getCovarianceMatrix(Collider[] colliders)
    {
        
        getVelocities(colliders);

        covarianceMatrix = new double[3,3];

        double[] means = new double[]{ 0f, 0f, 0f};

        for (int i = 0; i < velocities.Length; i++) {
            means[0] += velocities[i].x;
            means[1] += velocities[i].y;
            means[2] += velocities[i].z;
        }
        means[0] /= velocities.Length;
        means[1] /= velocities.Length;
        means[2] /= velocities.Length;

        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 3; j++)
            {
                covarianceMatrix[i, j] = 0.0f;

                for (int k = 0; k < velocities.Length; k++)
                {
                    covarianceMatrix[i, j] += (means[i] - velocities[k][i]) * (means[j] - velocities[k][j]);
                }
                    
                covarianceMatrix[i, j] /= velocities.Length - 1;
            }
        }

    }

    public static void ResetCollisionManager()
    {
        totalConflicts = 0;
        startTime = Time.time;
        lastCollisions = 0;

    }

}
