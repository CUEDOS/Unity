using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimManagerGas : MonoBehaviour
{
    [System.Serializable]
    public class Experiment
    {
        public float speed;
        public float maxAcceleration;
        public float runTime;
        public float spawnRate;
        public List<float> spawnRatios;
        public List<string> keys;
    }

    #region Singleton
    public static SimManagerGas Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public List<Experiment> experiments;
    float nextTime = 0;
    int exp = 0;


    private void FixedUpdate()
    {
        if (nextTime < Time.time)
        {
            nextTime = Time.time + experiments[exp].runTime;
            SpawnerManager.Instance.NextExp(experiments[exp]);
            exp++;
        }
        
    }

}
