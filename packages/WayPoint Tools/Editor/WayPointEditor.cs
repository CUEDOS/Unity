using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(WayPointManager))]
public class WayPointEditor : Editor
{
    Tool lastTool = Tool.None;
    void OnEnable()
    {
        lastTool = Tools.current;
        Tools.current = Tool.None;
    }

    void OnDisable()
    {
        Tools.current = lastTool;
    }

    void DrawCustomInspector()
    {
        WayPointManager wpManager = (WayPointManager)target;

        wpManager.newWayPointOffset = EditorGUILayout.Vector3Field("New WayPoint Offset", wpManager.newWayPointOffset);
        wpManager.newLookPointOffset = EditorGUILayout.Vector3Field("New LookPoint Offset", wpManager.newLookPointOffset);

        // WayPoint gizmos
        wpManager.debugRadius = EditorGUILayout.FloatField("Gizmo Size", wpManager.debugRadius);
        wpManager.pointColor = EditorGUILayout.ColorField("WayPoint Colour", wpManager.pointColor);
        wpManager.lineColor = EditorGUILayout.ColorField("WayPoint Line Colour", wpManager.lineColor);

        // LookPoint gizmos
        wpManager.useLookPoints = GUILayout.Toggle(wpManager.useLookPoints, "Show LookPoints");
        if (wpManager.useLookPoints)
        {
            wpManager.lookPointColor = EditorGUILayout.ColorField("LookPoint Colour", wpManager.lookPointColor);
            wpManager.lineOfSightColor = EditorGUILayout.ColorField("LookPoint Line Colour", wpManager.lineOfSightColor);
        }

        wpManager.persistentGizmos = GUILayout.Toggle(wpManager.persistentGizmos, "Persistent Gizmos");

        wpManager.showWayPointList = GUILayout.Toggle(wpManager.showWayPointList, "Show WayPoint List");
        // Only draw the list if it's checked
        if (wpManager.showWayPointList)
        {
            int wpCount = wpManager.WayPoints.Count;
            for (int i = 0; i < wpCount; i++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("WP " + (i+1).ToString());
                wpManager.WayPoints[i].position = EditorGUILayout.Vector3Field("Position", wpManager.WayPoints[i].position);
                wpManager.WayPoints[i].lookPoint = EditorGUILayout.Vector3Field("Look Point", wpManager.WayPoints[i].lookPoint);
            }

        }
    }

    protected virtual void OnSceneGUI()
    {
        Tools.current = Tool.None;
        WayPointManager wpManager = (WayPointManager)target;

        int count = wpManager.WayPoints.Count;
        for (int i = 0; i < count - 1; i++)
        {
            if (!wpManager.persistentGizmos)
            {
                // Draw handles for waypoint positions
                Handles.color = wpManager.pointColor;
                Handles.CubeHandleCap(0, wpManager.WayPoints[i].position, Quaternion.identity, wpManager.debugRadius, EventType.Repaint);
                Handles.color = wpManager.lineColor;
                Handles.DrawLine(wpManager.WayPoints[i].position, wpManager.WayPoints[i + 1].position);

                if (wpManager.useLookPoints)
                {
                    // Draw handles for waypoint look positions
                    Handles.color = wpManager.lookPointColor;
                    Handles.SphereHandleCap(0, wpManager.WayPoints[i].lookPoint, Quaternion.identity, wpManager.debugRadius, EventType.Repaint);
                    Handles.color = wpManager.lineOfSightColor;
                    Handles.DrawLine(wpManager.WayPoints[i].position, wpManager.WayPoints[i].lookPoint);
                }
            }
            // Update positions using standard xyz position handles
            wpManager.WayPoints[i].position = Handles.PositionHandle(wpManager.WayPoints[i].position, Quaternion.identity);

            if (wpManager.useLookPoints)
            {
                // Update lookpoint positions using xyz handles
                wpManager.WayPoints[i].lookPoint = Handles.PositionHandle(wpManager.WayPoints[i].lookPoint, Quaternion.identity);
            }
        }
        if (count >= 1)
        {
            if (!wpManager.persistentGizmos)
            {
                // Draw waypoint positions
                Handles.color = wpManager.pointColor;
                Handles.CubeHandleCap(0, wpManager.WayPoints[count - 1].position, Quaternion.identity, wpManager.debugRadius, EventType.Repaint);

                if (wpManager.useLookPoints)
                {
                    // Draw lookpoints
                    Handles.color = wpManager.lookPointColor;
                    Handles.SphereHandleCap(0, wpManager.WayPoints[count - 1].lookPoint, Quaternion.identity, wpManager.debugRadius, EventType.Repaint);
                    Handles.DrawLine(wpManager.WayPoints[count-1].position, wpManager.WayPoints[count-1].lookPoint);
                }
            }
            wpManager.WayPoints[count - 1].position = Handles.PositionHandle(wpManager.WayPoints[count - 1].position, Quaternion.identity);
            if (wpManager.useLookPoints)
            {
                wpManager.WayPoints[count-1].lookPoint = Handles.PositionHandle(wpManager.WayPoints[count-1].lookPoint, Quaternion.identity);
            }
        }

    }

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();

        DrawCustomInspector();

        WayPointManager wpManager = (WayPointManager)target;
        if (GUILayout.Button("Add WayPoint"))
        {
            wpManager.AddWayPoint();
            HandleUtility.Repaint();
            SceneView.RepaintAll();
            Repaint();
        }
        GUILayout.Space(10);
        if (GUILayout.Button("Remove Last WayPoint"))
        {
            wpManager.RemoveLastWayPoint();
            Repaint();
            HandleUtility.Repaint();
            SceneView.RepaintAll();

        }
    }
}