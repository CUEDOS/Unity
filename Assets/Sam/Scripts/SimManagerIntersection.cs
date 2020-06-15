using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Tayx.Graphy;

public class SimManagerIntersection : MonoBehaviour
{

    public Dictionary<string, float> expVars;
    public string testVariable = null;
    public float[] Variables = null;
    static public float CurrentVariable;

    public GameObject PrefabUAV;

    // Scene counter
    static public int sceneIndex = 0;

    // Runtime for each run after graph is found
    static public float runTime = 500f;

    // Incremented by runtime for each sim
    static private float endRun = 0;

    // For saving
    private float timeScale;
    public bool ScaleTime;

    private float timer = 0;

    public float currentFPS;

    GraphyManager gm;


    void Start()
    {

        Time.timeScale = 1f;
        timeScale = 1f;


        expVars = new Dictionary<string, float>
        {
            {"alignmentGain", 0.2f},
            {"cohesionGain", 1f},
            {"separationGain", 1f},
            {"migrationGain", 1f},
            {"orthSepGain", 1f},
            {"a", 20f},
            {"c", 3f},
            {"speed", 20f},
            {"maxForce", 20f},
            {"flockingInterval", 0.1f},
            {"SOIRadius", 25f},
            {"spawnRate", 1f },
            {"FPS", 30f }
        };

        if (ScaleTime) InvokeRepeating("GameTimeFPS", 1f, 1f);  //1s delay, repeat every 1s.

        NextVariable();

        gm = GameObject.FindObjectOfType<GraphyManager>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        // When at end of each simulation
        if (Time.time >= endRun)
        {
            // End sim if all vairables have been tested
            if (sceneIndex == Variables.Length)
            {
                Debug.Log("Time taken (mins): " + (Time.unscaledTime - timer) / 60f);
                Debug.Log("Total simulated time (mins): " + (Time.time - timer) / 60f);
                Application.Quit();
                EditorApplication.isPlaying = false;
                endRun = float.PositiveInfinity;
                return;
            }


            // Destroy all gameobjects for new run
            foreach (GameObject drone in GameObject.FindGameObjectsWithTag("UAV"))
            {
                Destroy(drone);
            }

            NextVariable();
        }

        currentFPS = gm.CurrentFPS;

    }



    public void StartRun()
    {
        endRun = Time.time + runTime;

    }


    private void NextVariable()
    {
        CurrentVariable = Variables[sceneIndex];
        expVars[testVariable] = CurrentVariable;

        sceneIndex++;

        UpdateUAV();
        UpdateSpawners();
        UpdateCollisionManager();

        StartRun();

    }

   private void GameTimeFPS()
    {


        if (Mathf.Abs(currentFPS - expVars["FPS"]) > 10f)
        {
            try
            {
                // If more FPS, run quicker
                if (currentFPS > expVars["FPS"]) timeScale*=1.1f;
                // If less, run slower
                else timeScale*=0.9f;

                Time.timeScale = timeScale;
            }
            catch { Debug.Log("Couldn't Change Timescale"); }
        }

    }


    void UpdateUAV()
    {
        Flock5 prefab = PrefabUAV.GetComponent<Flock5>();

        prefab.alignmentGain = expVars["alignmentGain"];
        prefab.cohesionGain = expVars["cohesionGain"];
        prefab.separationGain = expVars["separationGain"];
        prefab.migrationGain = expVars["migrationGain"];
        prefab.orthSepGain = expVars["orthSepGain"];
        prefab.a = expVars["a"];
        prefab.c = expVars["c"];
        prefab.speed = expVars["speed"];
        prefab.maxForce = expVars["maxForce"];
        prefab.flockingInterval = expVars["flockingInterval"];
        prefab.SOIRadius = expVars["SOIRadius"];

    }

    void UpdateCollisionManager()
    {
        CollisionManager.ResetCollisionManager();
    }

    void UpdateSpawners()
    {
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawner");
        foreach (var spawner in spawners)
        {
            spawner.GetComponent<IntersectionSpawner>().spawnRate = expVars["spawnRate"];        }
    }
}

