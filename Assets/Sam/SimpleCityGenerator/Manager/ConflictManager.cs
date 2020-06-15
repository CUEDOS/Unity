using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConflictManager : MonoBehaviour
{

    public static Vector3 LastReported;
    public static Vector3 LastReportedND;


    private static int totalConflicts = 0;
    private static float conflictFrequency = 0;
    public static float timeBin = 1f;
    private static float nextTime = 1f;
    private static int lastTotalConflicts = 0;
    private static Collider[] BOI;
    private static double dronesDensity;
    private static float currentTime;

    private static float startTime = 0;
    public static bool simRunning = false;
    public static LayerMask mask;

    public static Dictionary<List<GameObject>, float> reports;


    public static Vector3 DensityBoxDim;

    private void Start()
    {
        mask = LayerMask.GetMask("Collider");

        reports = new Dictionary<List<GameObject>, float>();

    }

    void FixedUpdate()
    {
        if (simRunning) currentTime = Time.time - startTime; else currentTime = 0;

        if(Time.time >= nextTime && simRunning)
        {
            conflictFrequency = (totalConflicts - lastTotalConflicts) / timeBin;
            nextTime = Time.time + timeBin;
            lastTotalConflicts = totalConflicts;

            BOI = Physics.OverlapBox(Vector3.up * 50, DensityBoxDim, Quaternion.identity, mask);

            dronesDensity = BOI.Length / Mathf.Pow(SimManager.expVars["CitySize"], 3f);

        }

    }
    
    public static void NewConflict(GameObject reportee, GameObject reported)
    {
        foreach (KeyValuePair<List<GameObject>,float> pair in reports)
        {
            if (pair.Key.Contains(reported) && pair.Key.Contains(reportee))
            {
                // Already been reported, no need to continue
                return;
            }
        }

        List<GameObject> newPair = new List<GameObject> { reported, reportee };
        reports.Add(newPair, Time.time);
        totalConflicts++;

        LastReportedND = (reportee.transform.position + reported.transform.position) / (2 * SimManager.expVars["CitySize"]);

        // Housekeeping - any entries that have been there for longer than 10 secs gets removed
        foreach (var item in reports.Where(kvp => Time.time - kvp.Value > 10f).ToList())
        {
            reports.Remove(item.Key);
        }

    }
    public static void StopSim()
    {
        simRunning = false;
        totalConflicts = 0;
        conflictFrequency = 0;
        lastTotalConflicts = 0;
    }
    public static void StartSim()
    {
        startTime = Time.time;
        DensityBoxDim = Vector3.one * SimManager.expVars["CitySize"];
        simRunning = true;

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(Vector3.up * SimManager.expVars["CitySize"]/2, DensityBoxDim);
    }
}
