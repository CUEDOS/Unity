using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDrone : MonoBehaviour
{
    public Vector3 velocity;

    // All for the demo, you don't need this script at all
    void FixedUpdate()
    {
        transform.position += Time.fixedDeltaTime * velocity;
    }
}
