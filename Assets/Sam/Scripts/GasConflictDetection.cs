using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasConflictDetection : MonoBehaviour
{
    public float SOIRadius = 10f;

    bool initialization = true;


    void Start()
    {
        // Make sure the drone has a sphere collider
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
        { 
            collider = gameObject.AddComponent<SphereCollider>();
        }
        collider.radius = SOIRadius;
        collider.isTrigger = true;



    }

    void OnTriggerEnter(Collider obj)
    {
        if (obj.name != "Conflict") return;
        // Collision is detected when drone is initiallised
        if (initialization)
        {
            initialization = false;
            return;
        }
        // If not in the area of interest
        if (Mathf.Abs(transform.position.x) > 100 / 2 || Mathf.Abs(transform.position.z) > 100 / 2) return;

        if (obj.transform != transform.parent.transform)
        {
            GasConflictManager.NewConflict(transform.parent.gameObject, obj.transform.parent.gameObject);
        }

    }
}
