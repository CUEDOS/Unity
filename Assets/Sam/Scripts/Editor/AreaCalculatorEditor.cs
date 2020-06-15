using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AreaCalculator))]
public class AreaCalculatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        AreaCalculator area = (AreaCalculator)target;

        if (GUILayout.Button("Calc"))
        {
            area.Calc();

        }

    }
}

