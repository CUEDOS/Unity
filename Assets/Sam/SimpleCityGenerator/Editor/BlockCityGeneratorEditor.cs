using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SimpleCityGenerator))]
public class BlockCityGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SimpleCityGenerator spawner = (SimpleCityGenerator)target;

        if (GUILayout.Button("Spawn Objects"))
        {
            spawner.EditorSpawn();

        }

        if (GUILayout.Button("Remove Spawns"))
        {
            spawner.DestroyAll();
        }
    }
}

