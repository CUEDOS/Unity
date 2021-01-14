using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System;

[CustomEditor(typeof(ExperimentManager))]
public class ExpManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get target from script
        var experimentManager = (ExperimentManager) target;
        
        //Title
        var boldText = new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold};
        GUILayout.Label("Experiment Script", boldText);
        experimentManager.targetExperiment = (MonoBehaviour)EditorGUILayout.ObjectField("Script", experimentManager.targetExperiment, typeof(MonoBehaviour), true);

        if (!experimentManager.targetExperiment) return;
        
        
        // Set up variables
        
        var dropDownNames = GetFieldNames(experimentManager.targetExperiment);
        var buttonWidth = 0.44f * (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth);
        var defaultWidth = EditorGUIUtility.labelWidth;
        
       
        
        GUILayout.Space(10f);
        
        // Independent Variable section
        GUILayout.Label("Independent Variable",boldText);

        for (var j = 0; j < experimentManager.independentVariables.Length; j++)
        {
            if (experimentManager.independentVariables[j] == null)
            {
                experimentManager.independentVariables[j] = new ExperimentManager.IndependentVariable
                    {name = dropDownNames[0]};
            }
            
            var label = new GUIContent("Variable");
            var id = Array.IndexOf(dropDownNames, experimentManager.independentVariables[j].name);
            id = id < 0 ? 0 : id;
            id = EditorGUILayout.Popup(label, id, dropDownNames);
            experimentManager.independentVariables[j].name = dropDownNames[id];
            EditorGUIUtility.labelWidth = 30f;
            GUILayout.BeginHorizontal();
            GUILayout.Space(200f);
            experimentManager.independentVariables[j].min = EditorGUILayout.FloatField("Min", experimentManager.independentVariables[j].min);
            GUILayout.Space(10f);
            experimentManager.independentVariables[j].step = EditorGUILayout.FloatField("Step", experimentManager.independentVariables[j].step);
            GUILayout.Space(10f);
            experimentManager.independentVariables[j].max = EditorGUILayout.FloatField("Max", experimentManager.independentVariables[j].max);
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = defaultWidth;
            
            EditorGUILayout.Space();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Add Field", GUILayout.Width(buttonWidth)))
        {
            var oldData = experimentManager.independentVariables;
            experimentManager.independentVariables = new ExperimentManager.IndependentVariable[experimentManager.independentVariables.Length + 1];

            for (var k = 0; k < oldData.Length; k++)
            {
                experimentManager.independentVariables[k] = oldData[k];
            }
            
            serializedObject.Update();
            
            experimentManager.independentVariables[experimentManager.independentVariables.Length - 1].min = 0f;
            GUILayout.Space(10f);
            experimentManager.independentVariables[experimentManager.independentVariables.Length - 1].step = 1f;
            GUILayout.Space(10f);
            experimentManager.independentVariables[experimentManager.independentVariables.Length - 1].max = 1f;

        }
        
        if (GUILayout.Button("Remove Field", GUILayout.Width(buttonWidth)))
        {
            if(experimentManager.independentVariables.Length==0) return;
            var oldData = experimentManager.independentVariables;
            experimentManager.independentVariables = new 
                ExperimentManager.IndependentVariable[experimentManager.independentVariables.Length - 1];

            for (var k = 0; k < oldData.Length - 1; k++)
            {
                experimentManager.independentVariables[k] = oldData[k];
            }
            serializedObject.Update();
        }
        
        GUILayout.EndHorizontal();
        
        // Expected Execution time
        try
        {
            experimentManager.FindRanges();
            var num = experimentManager.FindNumShapeExperiments();
            var numb = num.Item1;
            var time = numb * experimentManager.experimentTime / Time.timeScale;
            if (time > 60)
            {
                GUILayout.Label(time > 60 * 60
                    ? $"Expected Execution Time: {time / 3600} Hour(s)"
                    : $"Expected Execution Time: {time / 60} Minutes");
            }
            else
            {
                GUILayout.Label($"Expected Execution Time: {time} Seconds");

            }
        }
        catch
        {
            // ignored
        }

        // Dependent Variable Section
        GUILayout.Space(10f);
        GUILayout.Label("Dependent Variables",boldText);
        
        for (var j = 0; j < experimentManager.dependentVariables.Length; j++)
        {
            if (experimentManager.dependentVariables[j] == null)
            {
                experimentManager.dependentVariables[j] = new ExperimentManager.DependentVariable
                    {name = dropDownNames[0]};
            }
            
            var label = new GUIContent("Variable ");
            var id = Array.IndexOf(dropDownNames, experimentManager.dependentVariables[j].name);
            id = id < 0 ? 0 : id;
            id = EditorGUILayout.Popup(label, id, dropDownNames);
            experimentManager.dependentVariables[j].name = dropDownNames[id];
            
            EditorGUIUtility.labelWidth = 100f;
            GUILayout.BeginHorizontal();
            GUILayout.Space(200f);
            experimentManager.dependentVariables[j].dataType = 
                (ExperimentManager.Type)EditorGUILayout.EnumPopup("Data Type", 
                    experimentManager.dependentVariables[j].dataType);
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = defaultWidth;
            EditorGUILayout.Space();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Add Field", GUILayout.Width(buttonWidth)))
        {
            var oldData = experimentManager.dependentVariables;
            experimentManager.dependentVariables = new ExperimentManager.DependentVariable[experimentManager.dependentVariables.Length + 1];

            for (var k = 0; k < oldData.Length; k++)
            {
                experimentManager.dependentVariables[k] = oldData[k];
            }
            serializedObject.Update();
        }
        
        if (GUILayout.Button("Remove Field", GUILayout.Width(buttonWidth)))
        {
            if(experimentManager.dependentVariables.Length==0) return;
            var oldData = experimentManager.dependentVariables;
            experimentManager.dependentVariables = new ExperimentManager.DependentVariable[experimentManager.dependentVariables.Length - 1];

            for (var k = 0; k < oldData.Length - 1; k++)
            {
                experimentManager.dependentVariables[k] = oldData[k];
            }
            serializedObject.Update();
        }
        
        GUILayout.EndHorizontal();
        
        // Experiment Variable Section
        GUILayout.Space(10f);
        GUILayout.Label("Experimental Variables",boldText);
        experimentManager.experimentTime = EditorGUILayout.FloatField("Experiment Time", experimentManager.experimentTime);
        
        experimentManager.directory =
            EditorGUILayout.TextField("Directory", experimentManager.directory);
        experimentManager.fileName =
            EditorGUILayout.TextField("File Name", experimentManager.fileName);
        
        serializedObject.Update();
    }
    
    string[] GetFieldNames(MonoBehaviour script)
    {
        const BindingFlags bindingFlags = BindingFlags.Public |
                                          BindingFlags.NonPublic |
                                          BindingFlags.Instance |
                                          BindingFlags.Static;

        var classType = script.GetType();

        var fnames = classType.GetFields(bindingFlags);
        var mnames = classType.GetMethods(bindingFlags);

        var realMethods = new List<MethodInfo>();

        // There are 93 methods in a MonoBehaviour, we don't care about those
        for (var i = 0; i < mnames.Length - 93; i++)
        {
            if (mnames[i].ReturnType == typeof(void))
            {
                continue;
            }
            realMethods.Add(mnames[i]);
        }

        var names = new string[fnames.Length + realMethods.Count];

        for (var j = 0; j < fnames.Length; j++)
        {
            names[j] = fnames[j].Name;
        }

        for (var i = 0; i < realMethods.Count; i++)
        {
            names[i + fnames.Length] = realMethods[i].Name;
        }

        return names;
    }
}
