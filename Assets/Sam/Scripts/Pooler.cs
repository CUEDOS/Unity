using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Pooler : MonoBehaviour
{

    #region Singleton
    public static Pooler Instance;

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    [System.Serializable]
    public class Pool
    {
        public string key;
        public GameObject prefab;
    }

    public List<Pool> poolList; // Editable List
    Dictionary<string, GameObject> pools;
    Dictionary<string, Queue<GameObject>> poolDict;
    public List<GameObject> activeAgents;

    private float currentSpeed;
    private float currentAcc;

    void Start()
    {
        poolDict = new Dictionary<string, Queue<GameObject>>();
        pools = new Dictionary<string, GameObject>();
        foreach (Pool pool in poolList)
        {
            // Putting serializable list into dictionary
            pools.Add(pool.key, pool.prefab);

            // Creating queue for the actual pool dictionary
            Queue<GameObject> objectPool = new Queue<GameObject>();
            poolDict.Add(pool.key, objectPool);
        }

    }

    public void SpawnFromPool(string key,  Vector3 position, Vector3 destination)
    {
        if (!poolDict.ContainsKey(key))
        {
            Debug.LogWarning("Pool Dicitionary key: " + key + " - does not exist.");
            return;
        }
        GameObject spawn;
        try
        {
            spawn = poolDict[key].Dequeue();
        }
        catch (InvalidOperationException)
        {
            spawn = GenerateAgent(key);
        }
        
        spawn.SetActive(true);
        spawn.transform.position = position;

        UavNav nav = spawn.GetComponent<UavNav>();

        nav.Initalise(destination, key, currentSpeed, currentAcc);

        activeAgents.Add(spawn);
         
    }

    public void ReturnToPool(GameObject obj, string key)
    {
        obj.SetActive(false);
        poolDict[key].Enqueue(obj);
        activeAgents.Remove(obj);
    }

    public void ResetPool(float speed, float maxAcceleration)
    {
        currentSpeed = speed;
        currentAcc = maxAcceleration;

        foreach (var kvp in poolDict)
        {
            foreach(GameObject obj in kvp.Value)
            {

                if (obj.activeSelf)
                {
                    poolDict[kvp.Key].Enqueue(obj);
                    obj.SetActive(false);
                }
            }
        }
    }


    public GameObject GenerateAgent(string key)
    {
        GameObject obj = Instantiate(pools[key], this.transform);
        obj.SetActive(false);
        return obj;
    }





}
