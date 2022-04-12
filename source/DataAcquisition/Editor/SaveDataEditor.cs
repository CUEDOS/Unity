using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

[CustomEditor(typeof(SaveData))]
public class SaveDataEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SaveData data = (SaveData)target;

        // Button to delete the file at the path specified by the data saver script
        if (GUILayout.Button("Delete File"))
        {
            string path = "Assets/" + data.filePath;
            // DeleteAsset returns true if the file was deleted
            if (AssetDatabase.DeleteAsset(path))
            {
                Debug.Log("File: " + data.filePath + " deleted successfully.");
            }
            else
            {
                Debug.LogWarning("File could not be deleted, check if it exists and if it's in use somewhere else");
            }
        }

        

        float buttonWidth = 0.4f * (EditorGUIUtility.currentViewWidth - EditorGUIUtility.labelWidth);

        // -------------------------------------------------------------------------------------
        //          Draw the same collection of UI elements for each script we're saving from
        // -------------------------------------------------------------------------------------

        SaveData.TargetSaveData targetSaveData;
        for (int i = 0; i < data.targetData.Length; i++)
        {
            targetSaveData = data.targetData[i];

            // -------------------------------------------------------------------------------------
            //      Game object selection
            // -------------------------------------------------------------------------------------
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Game Object with target monobehaviour scripts attached");

            // Header and Remove Button for the root game object
            EditorGUILayout.BeginHorizontal();

            if (targetSaveData.targetGameObject != null)
            {
                EditorGUILayout.LabelField(data.targetData[i].targetGameObject.name);
            }
            else
            {
                EditorGUILayout.LabelField("");
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(buttonWidth)))
            {
                DeleteClassAtIndex(data, i);
                serializedObject.Update();
                return;
            }
            EditorGUILayout.EndHorizontal();

            // Draw the input for the target game object
            targetSaveData.targetGameObject = (GameObject)EditorGUILayout.ObjectField("Game Object: ", targetSaveData.targetGameObject, typeof(GameObject), true);

            // -------------------------------------------------------------------------------------
            //      Monobehaviour script selection
            // -------------------------------------------------------------------------------------

            // If no game object is selected we'll skip drawing this part and wait for one to be selected
            if (targetSaveData.targetGameObject != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Select script from the dropdown menu or drag and drop");

                // Draw the drop down for the script instances which are attached to the target game object
                EditorGUILayout.BeginHorizontal();

                // Grab any monobehaviour derived scripts on the object
                MonoBehaviour[] monoBehaviours = targetSaveData.targetGameObject.GetComponents<MonoBehaviour>();

                // Get the names of the scripts
                string[] dropDownNames = new string[monoBehaviours.Length];
                for (int j = 0; j < monoBehaviours.Length; j++)
                {
                    dropDownNames[j] = monoBehaviours[j].GetType().Name;
                }

                GUIContent label = new GUIContent("Select Script: ");

                int id = -1;

                if (targetSaveData.scriptInstance != null)
                {
                    // This seems like an expensive search but it'll do for now!
                    id = Array.IndexOf(dropDownNames, targetSaveData.scriptInstance.GetType().Name);
                }
                id = id < 0 ? 0 : id;
                id = EditorGUILayout.Popup(label, id, dropDownNames);
                targetSaveData.scriptInstance = monoBehaviours[id];

                EditorGUILayout.EndHorizontal();

                targetSaveData.scriptInstance = (MonoBehaviour)EditorGUILayout.ObjectField("Drag+Drop Script: ", targetSaveData.scriptInstance, typeof(MonoBehaviour), true);

                if (data.targetData[i].scriptInstance != null)
                {
                    dropDownNames = GetFieldNames(data.targetData[i].scriptInstance);

                    if (dropDownNames.Length == 0)
                    {
                        // There are no fields or methods which we can collect data from to save
                        EditorGUILayout.LabelField("Script has no suitable fields or methods");
                        data.targetData[i].fieldNames = new string[] { "ENTER NAME" };
                    }
                    else
                    {
                        for (int j = 0; j < data.targetData[i].fieldNames.Length; j++)
                        {
                            label = new GUIContent("Field to Save: ");
                            id = Array.IndexOf(dropDownNames, data.targetData[i].fieldNames[j]);
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

    //private string[] GetMonoBehaviourNames(GameObject targetGameObject)
    //{
    //    MonoBehaviour[] monoBehaviours = targetGameObject.GetComponents<MonoBehaviour>();
    //    string[] names = new string[monoBehaviours.Length];
    //    for (int i = 0; i < monoBehaviours.Length; i++)
    //    {
    //        names[i] = monoBehaviours[i].GetType().Name;
    //    }
    //    return names;
    //}

    
    void DeleteClassAtIndex(SaveData data, int index)
    {
        SaveData.TargetSaveData[] oldData = data.targetData;
        data.targetData = new SaveData.TargetSaveData[data.targetData.Length - 1];

        int i = 0;
        int j = 0;

        while (i < oldData.Length)
        {
            if (i != index)
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
