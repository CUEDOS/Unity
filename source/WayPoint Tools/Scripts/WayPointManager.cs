using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPointManager : MonoBehaviour
{
    // We need to maintain a list of WayPoint objects
    // NOTE: This IS NOT the same as Waypoint - that's Bill's class
    // We should probably create a namespace for this but for the sake of
    // getting things done quickly I'm just writing this comment for now
    public bool showWayPointList = false;
    public List<WayPoint> WayPoints = new List<WayPoint>();

    [Tooltip("Displacement of new waypoint from the previous waypoint")]
    public Vector3 newWayPointOffset = new Vector3(0, 0, 5);
    [Tooltip("Displacement of new lookpoint from the new waypoint position")]
    public Vector3 newLookPointOffset = new Vector3(0, 5, 5);

    // Gizmo stats
    public float debugRadius = 1f;
    public Color pointColor = Color.green;

    // Look point gizmo stats
    [Tooltip("Toggle the look point gizmos, doesn't affect controllers using waypoints")]
    public bool useLookPoints;
    public Color lookPointColor = Color.blue;
    public Color lineColor = Color.white;
    public Color lineOfSightColor = Color.blue;
    public bool persistentGizmos = true;

    public void AddWayPoint()
    {
        Vector3 newPoint;
        Vector3 newLookPoint;

        if (WayPoints.Count == 0)
        {
            // Default to this transform's position for first waypoint
            newPoint = transform.position;
            newLookPoint = newPoint + newLookPointOffset;
        }
        else
        {
            newPoint = WayPoints[WayPoints.Count - 1].position + newWayPointOffset;
            newLookPoint = newPoint + newLookPointOffset;
        }
        WayPoints.Add(new WayPoint(newPoint, newLookPoint));
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
                // Draw position gizmo
                Gizmos.color = pointColor;
                Gizmos.DrawCube(WayPoints[i].position, Vector3.one * debugRadius);
                Gizmos.color = lineColor;
                Gizmos.DrawLine(WayPoints[i].position, WayPoints[i + 1].position);
                if (useLookPoints)
                {
                    // Draw look point gizmo
                    Gizmos.color = lookPointColor;
                    Gizmos.DrawSphere(WayPoints[i].lookPoint, 0.5f* debugRadius);
                    Gizmos.color = lineOfSightColor;
                    Gizmos.DrawLine(WayPoints[i].position, WayPoints[i].lookPoint);
                }
            }
            if (count >= 1)
            {
                // Draw final waypoint
                Gizmos.color = pointColor;
                Gizmos.DrawCube(WayPoints[count - 1].position, Vector3.one*debugRadius);
                if (useLookPoints)
                {
                    // Draw final look point gizmo
                    Gizmos.color = lookPointColor;
                    Gizmos.DrawSphere(WayPoints[count - 1].lookPoint, 0.5f* debugRadius);
                    Gizmos.color = lineOfSightColor;
                    Gizmos.DrawLine(WayPoints[count-1].position, WayPoints[count-1].lookPoint);
                }
            }
        }
    }
}