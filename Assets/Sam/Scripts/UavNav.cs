using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class UavNav : MonoBehaviour
{
    Vector3 destination;
    Vector3 direction;
    Vector3 velocity;
    Rigidbody rb;
    float forceMag;
    float maxAcceleration;
    float speed;
    string key;

    Flock3 flock;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        flock = GetComponent<Flock3>();
    }

    public void Initalise(Vector3 dest, string k, float spd, float acc)
    {
        destination = dest;
        key = k;
        speed = spd;
        maxAcceleration = acc;
        flock.maxForce = acc;

        rb.velocity = Vector3.Normalize(destination - transform.position) * speed;
    }


    void FixedUpdate()
    {

        if (Vector3.Distance(transform.position, destination) < 2f)
            Pooler.Instance.ReturnToPool(this.gameObject, key);
        // move toward destination

        direction = (destination - transform.position).normalized;

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, destination);
    }
}
