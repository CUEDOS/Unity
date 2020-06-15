using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    float spawnRate;
    float spawnPeriod;
    float nextSpawn = 0;
    float sceneRadius = 50;
    List<string> keys;
    List<float> spawnRatios;

    #region Singleton
    public static SpawnerManager Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public void NextExp(SimManagerGas.Experiment exp)
    {
        spawnRate = exp.spawnRate;
        spawnPeriod = 1 / spawnRate;
        keys = exp.keys;
        spawnRatios = exp.spawnRatios;
        Pooler.Instance.ResetPool(exp.speed, exp.maxAcceleration);

    }

    void FixedUpdate()
    {
        if(nextSpawn < Time.time && Time.time > 2f)
        {
            float r = Random.value;
            float sum = 0f;
            int i;
            for (i = 0; i < keys.Count; i++)
            {
                sum += spawnRatios[i];
                if (r < sum)
                    break;
            }
            nextSpawn = Time.time + spawnPeriod;
            Pooler.Instance.SpawnFromPool(keys[i], Random.onUnitSphere * sceneRadius, Random.onUnitSphere * sceneRadius);
        }

    }
}
