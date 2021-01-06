using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

[CustomEditor(typeof(SaveData))]
public class SaveDataEditor : Editor
{

    //void OnEnable()
    //{
    //    data = serializedObject.FindProperty("targetData");
    //}

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SaveData data = (SaveData)target;

        if (GUILayout.Button("Clear File"))
        {
            data.ClearFile();

        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Classes and fields to collect data from");

        float buttonWidth = 0.4f * (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth);


        for (int i = 0; i < data.targetData.Length; i++)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (data.targetData[i].instance != null)
            {
                EditorGUILayout.LabelField(data.targetData[i].instance.name);
            }
            else
            {
                EditorGUILayout.LabelField("");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove Class", GUILayout.Width(buttonWidth)))
            {
                DeleteClassAtIndex(data, i);
                serializedObject.Update();
                return;
            }
            EditorGUILayout.EndHorizontal();

            DrawDataFields(data.targetData[i]);

            if (data.targetData[i].instance != null)
            {
                string[] dropDownNames = GetFieldNames(data.targetData[i].instance);

                for (int j = 0; j < data.targetData[i].fieldNames.Length; j++)
                {
                    GUIContent label = new GUIContent("Field to Save: ");
                    int id = Array.IndexOf(dropDownNames, data.targetData[i].fieldNames[j]);
                    id = id < 0 ? 0 : id;
                    id = EditorGUILayout.Popup(label, id, dropDownNames);
                    data.targetData[i].fieldNames[j] = dropDownNames[id];
                }

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(EditorGUIUtility.labelWidth));

                if (GUILayout.Button("Remove Field", GUILayout.Width(buttonWidth)))
                {
                    string[] oldData = data.targetData[i].fieldNames;
                    data.targetData[i].fieldNames = new string[data.targetData[i].fieldNames.Length - 1];

                    for (int k = 0; k < oldData.Length - 1; k++)
                    {
                        data.targetData[i].fieldNames[k] = oldData[k];
                    }
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Add Field", GUILayout.Width(buttonWidth)))
                {
                    string[] oldData = data.targetData[i].fieldNames;
                    data.targetData[i].fieldNames = new string[data.targetData[i].fieldNames.Length + 1];

                    for (int k = 0; k < oldData.Length; k++)
                    {
                        data.targetData[i].fieldNames[k] = oldData[k];
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Add Data Class/Script"))
        {
            SaveData.TargetSaveData[] oldData = data.targetData;
            data.targetData = new SaveData.TargetSaveData[data.targetData.Length + 1];

            for (int i = 0; i < oldData.Length; i++)
            {
                data.targetData[i] = oldData[i];
            }
        }

        serializedObject.Update();
    }

    void DrawDataFields(SaveData.TargetSaveData dataDraw)
    {
        dataDraw.instance = (MonoBehaviour)EditorGUILayout.ObjectField("Script/Instance: ", dataDraw.instance, typeof(MonoBehaviour), true);
    }

    void DeleteClassAtIndex(SaveData data, int index)
    {
        SaveData.TargetSaveData[] oldData = data.targetData;
        data.targetData = new SaveData.TargetSaveData[data.targetData.Length - 1];

        int i = 0;
        int j = 0;

        while(i < oldData.Length)
        {
            if(i != index)
            {
                data.targetData[j] = oldData[i];
                j++;
            }
            i++;
        }
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