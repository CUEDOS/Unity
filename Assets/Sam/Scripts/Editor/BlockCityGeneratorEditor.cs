using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BlockCityGenerator))]
public class BlockCityGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        BlockCityGenerator spawner = (BlockCityGenerator)target;

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

