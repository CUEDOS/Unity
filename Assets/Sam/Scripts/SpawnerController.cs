using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour
{
    private static GameObject[] spawners;
    public float SpawnRate;
    private static bool initialSet = false;
    private static float LastSpawnRate;
    private static SpaceManager spaceManagerInstance;

    void Start()
    {
        // Start instances
        spaceManagerInstance = Component.FindObjectOfType<SpaceManager>();


        SpawnerReset();
    }

    void FixedUpdate()
    {

        if (spaceManagerInstance.isPrimaryProcessingCompleted && !initialSet)
        {
            SpawnRate = SimManager.expVars["SpawnRate"];
            initialSet = true;
        }

        if (SpawnRate != LastSpawnRate)
        {
            SetSpawners(SpawnRate);
            LastSpawnRate = SpawnRate;
        }
    
    }

    public static void SpawnerReset()
    {
        initialSet = false;
        LastSpawnRate = 0f;

        // Find all spawners
        spawners = GameObject.FindGameObjectsWithTag("Spawner");

        SetSpawners(LastSpawnRate);

    }

    private static void SetSpawners(float rate)
    {
        foreach (GameObject spawner in spawners)
        {
            spawner.GetComponent<SpawnScript>().spawnRate = rate;
        }
    }
}
