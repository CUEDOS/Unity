using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class AreaCalculator : MonoBehaviour
{
    public GameObject plane;
    public float points = 100;
    public float xmin = -210;
    public float xmax = 175;
    public float zmin = -245;
    public float zmax = 190;

    public float ymin = 0;
    public float ymax = 200;
    public float area = 0;
    Vector3 origin;
    public float planeHit;
    public float buildingHit;
    public float levels;
    public float BCI;
    public float floorArea;
    public float FAI;

    public void Calc()
    {
        double[,] heights = new double[(int)points, (int)points];
        //float[] heights = new float[(int)points * (int)points];
        area = (xmax - xmin) * (zmax - zmin);
        floorArea = 0;

        levels = Mathf.Ceil((ymax - ymin) / 2.3f);
        int inc = 0;
        double totalHeight = 0;
        for (int k = 0; k < levels; k++)
        {
            planeHit = 0;
            buildingHit = 0;
            for (int i = 0; i < points; i++)
            {
                for (int j = 0; j < points; j++)
                {
                    float x = xmin + i / points * (xmax - xmin);
                    float z = zmin + j / points * (zmax - zmin);
                    origin = new Vector3(x, 200, z);
                    RaycastHit hit;
                    if (Physics.Raycast(origin, transform.TransformDirection(Vector3.down), out hit, ymax - k * 2.3f))
                    {
                        if (hit.rigidbody == plane.GetComponent<Rigidbody>())
                        {
                            planeHit++;
                            if (k == 0)
                            {
                                heights[i, j] = double.NaN;
                            }
                        }
                        else
                        {
                           
                            buildingHit++;
                            if (k == 0)
                            {
                                heights[i, j] = ymax - hit.distance;
                                totalHeight += ymax - hit.distance;
                                inc++;
                            }
                        }

                    }
                    else
                    {
                        planeHit++;
                        if (k == 0)
                        {
                            heights[i, j] = double.NaN;
                        }
                    }

                }

            }
            if (k == 0) BCI = buildingHit / (planeHit + buildingHit);
            floorArea += buildingHit / (planeHit + buildingHit) * area;
        }
        FAI = floorArea / area;
        Debug.Log("FINAL: BCI = " + BCI + "\t FAI = " + FAI + "\tArea = " + area + "\tCitySize = " + Mathf.Round(Mathf.Sqrt(area)));

        double meanHeight = totalHeight / inc;

        double[] heights1d = new double[(int)points * (int)points];
        int ind = 0;
        for (int i = 0; i < points; i++)
        {
            for (int j = 0; j < points; j++)
            {
                heights1d[ind] = heights[i, j];
                ind++;
            }
        }
        Array.Sort<double>(heights1d);
        double[] novelHeights = new double[heights1d.Length];
        int nov = 0;
        for (int i = 0; i < heights1d.Length-1; i++)
        {
            if(!double.IsNaN(heights1d[i]) && Math.Round(heights1d[i]) != Math.Round(heights1d[i + 1]))
            {
                novelHeights[nov] = heights1d[i];
                nov++;
            } 
        }
        double sigma = 0;
        double sums = 0;
        for (int i = 0; i < nov; i++)
        {
            sums += (meanHeight - novelHeights[i]) * (meanHeight - novelHeights[i]);

        }
        sigma = Math.Sqrt(Convert.ToDouble(1) / Convert.ToDouble(nov) * sums);

        
        Debug.Log("Mean Height = " + meanHeight + " +- " + sigma + " (" + sigma/meanHeight + ")");



    }
    void Update()
    {
        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            if (hit.rigidbody == plane.GetComponent<Rigidbody>())
            {
                Debug.Log("Hit Plane");
            }
            else
            {
                Debug.Log("Hit Building");
            }
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 1000, Color.white);
            Debug.Log("Did not Hit");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(new Vector3(xmin + xmax, ymin + ymax/2 , zmin + zmax ), new Vector3(xmax - xmin, ymax - ymin, zmax - zmin));
    }
}
