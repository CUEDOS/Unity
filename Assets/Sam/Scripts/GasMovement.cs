using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GasMovement : MonoBehaviour
{

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.velocity = Random.insideUnitSphere * GasConflictManager.speed;
    }

    private void FixedUpdate()
    {
        rb.velocity = rb.velocity.normalized * GasConflictManager.speed;
    }

}
