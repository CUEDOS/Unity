using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntersectionSpawner : MonoBehaviour
{
    GridSpawn gridSpawn;
    public float spawnRate;
    float nextTime = 0f;

    public GameObject destination;
    void Start()
    {
        gridSpawn = GetComponent<GridSpawn>();
        gridSpawn.GenerateSpawnPoints();
    }


    void Update()
    {
        if (nextTime < Time.time)
        {
            nextTime = Time.time + 1 / spawnRate;
            GameObject spawnee = gridSpawn.SpawnAtRandomPoint();
            Flock5 flock = spawnee.GetComponent<Flock5>();
            flock.destination = destination;
            spawnee.GetComponent<Rigidbody>().velocity = flock.speed * (destination.transform.position - spawnee.transform.position).normalized;
        }
        
    }
}
