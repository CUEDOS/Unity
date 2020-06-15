using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public AnimationCurve plot = new AnimationCurve();
    public GameObject[] drones;
    private void Start()
    {
        drones = GameObject.FindGameObjectsWithTag("Drone");
    }
    void Update()
    {
        float force = 0;
        foreach (GameObject drone in drones)
        {
            //force += drone.GetComponent<Pursuer>().dbg;
            //force += drone.GetComponent<Pursuer>().Force.magnitude;
            force += drone.GetComponent<Rigidbody>().velocity.magnitude;
        }
        float value = force / drones.Length;
        plot.AddKey(Time.realtimeSinceStartup, value);
    }
}
