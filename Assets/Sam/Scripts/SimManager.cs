using Mapbox.Unity.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SimManager : MonoBehaviour
{

    // Dictionary for variables customisazble between experiments
    static public Dictionary<string, float> expVars;

    // Independent variable and test cases
    public string testVariable = null;
    public float[] Variables = null;
    static public float CurrentVariable;

    // Scene counter
    static public int sceneIndex = 0;

    // Runtime for each run after graph is found
    static public float runTime = 200f;

    // Incremented by runtime for each sim
    static private float endRun = 0;

    // Boolean for if the sim need to be reset after each run
    static private bool useResetSim = true;

    static private bool simRunning;

    // If running speed tests, can use variable time; shorter runs for faster drones.
    public bool variableRunTime = true;


    // Class instances
    SpaceManager spaceManagerInstance;
    BlockCityGenerator blockCityGeneratorInstance;

    // For saving
    private float timeScale;
    public float timeScaleGain = 0.5f;
    private float speed;

    private float timer = 0;

    private void Start()
    {
        timer = Time.time;
        // Initialising variables
        spaceManagerInstance = FindObjectOfType<SpaceManager>();
        blockCityGeneratorInstance = FindObjectOfType<BlockCityGenerator>();

        // Assigning values to dictionary
        expVars = new Dictionary<string, float>
        {
            { "Speed", 10f },
            { "SpawnRate", 2f },
            { "CitySize", 240f },
            { "BCI", 0.6f },
            { "FAI", 10f },
            { "HVariability", 0.8f },
            { "ARVariability", 0.2f }
        };


        simRunning = false;

        // Decide whether the sim will need to be reset at the begining of each run;
        if (testVariable == "Speed" || testVariable == "SpawnRate") useResetSim = false;

        RunTime();

        ResetSim();

    }

    void FixedUpdate()
    {
        // If the run hasn't yet started and is waiting for primary processing, once complete, start the run.
        if (!simRunning && spaceManagerInstance.isPrimaryProcessingCompleted)
        {
            StartRun();
        }

        // When at end of each simulation
        if(Time.time >= endRun && simRunning)
        {
            // End sim if all vairables have been tested
            if(sceneIndex == Variables.Length)
            {
                Debug.Log("Time taken (Mins): " + (Time.time - timer) / 60f);
                Application.Quit();
                EditorApplication.isPlaying = false;
                return;
            }


            // Destroy all gameobjects for new run
            foreach (GameObject drone in GameObject.FindGameObjectsWithTag("Drone"))
            {
                Destroy(drone);
            }

            simRunning = false;
            ConflictManager.StopSim();

            if (useResetSim) ResetSim(); else NextVariable();
        }


    }

    private void ResetSim()
    {
        // Update variable
        NextVariable();

        if (blockCityGeneratorInstance != null) blockCityGeneratorInstance.Spawn();

        // Doesn't call spacemanager if using Mapbox
        AbstractMap mapbox = GameObject.FindObjectOfType<AbstractMap>();
        if (mapbox == null)
        {
            spaceManagerInstance.isPrimaryProcessingCompleted = false;
            spaceManagerInstance.PrimaryProcessing();
        }
    }

    public static void StartRun()
    {
        endRun = Time.time + runTime;
        simRunning = true;

        ConflictManager.StartSim();
    }


    private void NextVariable()
    {

        CurrentVariable = Variables[sceneIndex];
        expVars[testVariable] = CurrentVariable;
        if (variableRunTime) runTime = 300f / expVars["SpawnRate"];
        sceneIndex++;

        // Time updating for smooth run
        timeScale = timeScaleGain / expVars["SpawnRate"];
        Time.timeScale = timeScale;

        speed = expVars["Speed"];

        SpawnerController.SpawnerReset();

        Debug.Log("Next " + testVariable + ": " + CurrentVariable + "\t Estimated Runtime (mins): " + Mathf.Round(runTime /timeScale/60f));
    }

    private void RunTime()
    {
        float time;
        float vRunTime;
        float totalTime = 0;
        // Calculating the expected run time
        for (int i = 0; i < Variables.Length; i++)
        {
            if(testVariable == "SpawnRate")
            {
                timeScale = timeScaleGain / Variables[i];
            }
            else
            {
                timeScale = timeScaleGain / expVars["SpawnRate"];
            }
            

            if (variableRunTime && testVariable == "SpawnRate")
            {
                vRunTime = 300f / Variables[i];
                time = vRunTime / timeScale;
            }else if (variableRunTime)
            {
                vRunTime = 300f / expVars["SpawnRate"];
                time = vRunTime / timeScale;
            }
            else
            {
                time = runTime / timeScale;
                vRunTime = runTime;
            }

            Debug.Log("Variable (" + testVariable + ") : " + Variables[i] + " with sim time (mins): " + Mathf.Round(vRunTime/60f) + " will take estimated time (mins): " + Mathf.Round(time /60f));
            totalTime += time;
        }
        Debug.Log("Total estimated run time is (hrs): " + Mathf.Round(totalTime / 3600f));

    }

}
