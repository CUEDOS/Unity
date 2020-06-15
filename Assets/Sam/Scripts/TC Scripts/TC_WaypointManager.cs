using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TC_WaypointManager : MonoBehaviour
{
    public List<Vector3> WayPoints = new List<Vector3>();
    public float debugRadius = 1f;
    public Color pointColor = Color.green;
    public Color lineColor = Color.white;
    public bool persistentGizmos = true;

    public void AddWayPoint()
    {
        Vector3 newPoint;
        if (WayPoints.Count == 0)
        {
            newPoint = Vector3.zero;
        }
        else
        {
            newPoint = WayPoints[WayPoints.Count - 1] + Vector3.forward;
        }
        WayPoints.Add(newPoint);
    }

    public void RemoveLastWayPoint()
    {
        int count = WayPoints.Count;
        if (count >= 1)
        {
            WayPoints.RemoveAt(count - 1);
        }
    }

    void OnDrawGizmos()
    {
        if (persistentGizmos)
        {
            int count = WayPoints.Count;
            for (int i = 0; i < count - 1; i++)
            {
                Gizmos.color = pointColor;
                Gizmos.DrawCube(WayPoints[i], Vector3.one * debugRadius);
                Gizmos.color = lineColor;
                Gizmos.DrawLine(WayPoints[i], WayPoints[i + 1]);
            }
            if (count >= 1)
            {
                Gizmos.color = pointColor;
                Gizmos.DrawCube(WayPoints[count - 1], Vector3.one * debugRadius);
            }
        }
    }
}