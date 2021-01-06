using System.Collections;
using System.Collections.Generic;
using Samspace;
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
        ExperimentManager experimentManager = (ExperimentManager) target;
        
        // Set up variables
        var boldtext = new GUIStyle (GUI.skin.label);
        boldtext.fontStyle = FontStyle.Bold;
        string[] dropDownNames = GetFieldNames(experimentManager.targetExperiment);
        float buttonWidth = 0.44f * (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth);
        float defaultWidth = EditorGUIUtility.labelWidth;
        //Title
        GUILayout.Label("Experiment Script", boldtext);
        experimentManager.targetExperiment = (MonoBehaviour)EditorGUILayout.ObjectField("Script", experimentManager.targetExperiment, typeof(MonoBehaviour), true);
        GUILayout.Space(10f);
        
        // Independent Variable section
        GUILayout.Label("Independent Variable",boldtext);

        for (int j = 0; j < experimentManager.IndependentVariables.Length; j++)
        {
            if (experimentManager.IndependentVariables[j] == null)
            {
                experimentManager.IndependentVariables[j] = new ExperimentManager.IndependentVariable
                    {Name = dropDownNames[0]};
            }
            
            GUIContent label = new GUIContent("Variable");
            int id = Array.IndexOf(dropDownNames, experimentManager.IndependentVariables[j].Name);
            id = id < 0 ? 0 : id;
            id = EditorGUILayout.Popup(label, id, dropDownNames);
            experimentManager.IndependentVariables[j].Name = dropDownNames[id];
            EditorGUIUtility.labelWidth = 30f;
            GUILayout.BeginHorizontal();
            GUILayout.Space(200f);
            experimentManager.IndependentVariables[j].MIN = EditorGUILayout.FloatField("Min", experimentManager.IndependentVariables[j].MIN);
            GUILayout.Space(10f);
            experimentManager.IndependentVariables[j].Step = EditorGUILayout.FloatField("Step", experimentManager.IndependentVariables[j].Step);
            GUILayout.Space(10f);
            experimentManager.IndependentVariables[j].MAX = EditorGUILayout.FloatField("Max", experimentManager.IndependentVariables[j].MAX);
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = defaultWidth;
            
             EditorGUILayout.Space();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Field", GUILayout.Width(buttonWidth)))
        {
            ExperimentManager.IndependentVariable[] oldData = experimentManager.IndependentVariables;
            experimentManager.IndependentVariables = new ExperimentManager.IndependentVariable[experimentManager.IndependentVariables.Length + 1];

            for (int k = 0; k < oldData.Length; k++)
            {
                experimentManager.IndependentVariables[k] = oldData[k];
            }
        }
        if (GUILayout.Button("Remove Field", GUILayout.Width(buttonWidth)))
        {
            if(experimentManager.IndependentVariables.Length==0) return;
            ExperimentManager.IndependentVariable[] oldData = experimentManager.IndependentVariables;
            experimentManager.IndependentVariables = new ExperimentManager.IndependentVariable[experimentManager.IndependentVariables.Length - 1];

            for (int k = 0; k < oldData.Length - 1; k++)
            {
                experimentManager.IndependentVariables[k] = oldData[k];
            }
        }
        GUILayout.EndHorizontal();
        
        // Dependent Variable Section
        GUILayout.Space(10f);
        GUILayout.Label("Dependent Variables",boldtext);
        
        for (int j = 0; j < experimentManager.DependentVariables.Length; j++)
        {
            if (experimentManager.DependentVariables[j] == null)
            {
                experimentManager.DependentVariables[j] = new ExperimentManager.DependentVariable
                    {Name = dropDownNames[0]};
            }
            
            GUIContent label = new GUIContent("Variable ");
            int id = Array.IndexOf(dropDownNames, experimentManager.DependentVariables[j].Name);
            id = id < 0 ? 0 : id;
            id = EditorGUILayout.Popup(label, id, dropDownNames);
            experimentManager.DependentVariables[j].Name = dropDownNames[id];
            
            EditorGUIUtility.labelWidth = 100f;
            GUILayout.BeginHorizontal();
            GUILayout.Space(200f);
            experimentManager.DependentVariables[j].DataType = (ExperimentManager.Type)EditorGUILayout.EnumPopup("Data Type", experimentManager.DependentVariables[j].DataType);
            GUILayout.EndHorizontal();
            EditorGUIUtility.labelWidth = defaultWidth;
            EditorGUILayout.Space();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Add Field", GUILayout.Width(buttonWidth)))
        {
            ExperimentManager.DependentVariable[] oldData = experimentManager.DependentVariables;
            experimentManager.DependentVariables = new ExperimentManager.DependentVariable[experimentManager.DependentVariables.Length + 1];

            for (int k = 0; k < oldData.Length; k++)
            {
                experimentManager.DependentVariables[k] = oldData[k];
            }
        }
        if (GUILayout.Button("Remove Field", GUILayout.Width(buttonWidth)))
        {
            if(experimentManager.DependentVariables.Length==0) return;
            ExperimentManager.DependentVariable[] oldData = experimentManager.DependentVariables;
            experimentManager.DependentVariables = new ExperimentManager.DependentVariable[experimentManager.DependentVariables.Length - 1];

            for (int k = 0; k < oldData.Length - 1; k++)
            {
                experimentManager.DependentVariables[k] = oldData[k];
            }
        }
        GUILayout.EndHorizontal();
        
        // Dependent Variable Section
        GUILayout.Space(10f);
        GUILayout.Label("Experimental Variables",boldtext);
        experimentManager.ExperimentTime = EditorGUILayout.FloatField("Experiment Time", experimentManager.ExperimentTime);
        
        experimentManager.Directory =
            EditorGUILayout.TextField("Directory", experimentManager.Directory);
        experimentManager.FileName =
            EditorGUILayout.TextField("File Name", experimentManager.FileName);
        
        serializedObject.Update();
    }
    
    string[] GetFieldNames(MonoBehaviour script)
    {

        BindingFlags bindingFlags = BindingFlags.Public |
                                    BindingFlags.NonPublic |
                                    BindingFlags.Instance |
                                    BindingFlags.Static;

        Type classType = script.GetType();

        FieldInfo[] Fnames = classType.GetFields(bindingFlags);
        MethodInfo[] Mnames = classType.GetMethods(bindingFlags);

        List<MethodInfo> realMethods = new List<MethodInfo>();

        // There are 93 methods??? in a MonoBehaviour, we don't care about those
        // I am praying that it will always find my methods first and then add on
        // the monobehaviour methods after otherwise I can't hide them from users
        for (int i = 0; i < Mnames.Length - 93; i++)
        {
            if (Mnames[i].ReturnType == typeof(void))
            {
                continue;
            }
            realMethods.Add(Mnames[i]);
        }


        string[] names = new string[Fnames.Length + realMethods.Count];

        for (int j = 0; j < Fnames.Length; j++)
        {
            names[j] = Fnames[j].Name;
        }

        for (int i = 0; i < realMethods.Count; i++)
        {
            names[i + Fnames.Length] = realMethods[i].Name;
        }

        return names;
    }
    
}
