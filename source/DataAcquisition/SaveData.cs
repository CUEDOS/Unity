using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.IO;

public class SaveData : MonoBehaviour
{
    [System.Serializable]
    public class TargetSaveData
    {
        public GameObject targetGameObject;
        public MonoBehaviour scriptInstance;
        public string[] fieldNames = new string[] { "ENTER NAME" };
        public FieldInfo[] fieldInfos = new FieldInfo[0];
        public MethodInfo[] methodInfos = new MethodInfo[0];
    }

    [HideInInspector]
    public TargetSaveData[] targetData = new TargetSaveData[1];
    public string filePath = "Test.txt";
    string path;

    // In the future it may be useful to have separate collection and writing to file, so that the file writing is spread out?
    // I'm not sure how expensive it is to open and close a file, we can't leave it open.
    // public float DataCollectionFrequency = 10f;
    public float SaveFrequency = 10f;
    float count;

    public void CreateFile()
    {
        // Unity by default saves files to the project folder, so I add this to put them in assets folder
        path = "Assets/" + filePath;

        // Make sure the file doesn't already exist
        if (File.Exists(path))
        {
            // For now I'm just adding a new line to show a new run of data collection
            File.AppendAllText(path, "\n");
            Debug.Log("Found existing file, data will be appended to this.");
            return;
        }

        // Create the new file
        FileStream f = File.Create(path);
        f.Close();

        // We're going to add a header which is all the field names
        string header = "";

        for (int i = 0; i < targetData.Length; i++)
        {
            // Make sure the instance we are looking at actually exists
            if (targetData[i].scriptInstance == null)
                continue;

            // Run through the fields and check for vectors - they need extra columns
            for (int j = 0; j < targetData[i].fieldInfos.Length; j++)
            {
                if (targetData[i].fieldInfos[j].FieldType == typeof(Vector3))
                {
                    header += Vector3Header(targetData[i].fieldInfos[j].Name);
                }
                else if (targetData[i].fieldInfos[j].FieldType == typeof(Vector2))
                {
                    header += Vector2Header(targetData[i].fieldInfos[j].Name);
                }
                else
                {
                    header += targetData[i].fieldInfos[j].Name + '\t';
                }
            }

            // Do the same for the methods
            for (int j = 0; j < targetData[i].methodInfos.Length; j++)
            {
                if (targetData[i].methodInfos[j].ReturnType == typeof(Vector3))
                {
                    header += Vector3Header(targetData[i].methodInfos[j].Name);
                }
                else if (targetData[i].methodInfos[j].ReturnType == typeof(Vector2))
                {
                    header += Vector2Header(targetData[i].methodInfos[j].Name);
                }
                else
                {
                    header += targetData[i].methodInfos[j].Name + '\t';
                }
            }
        }

        // Write the header to the file
        header += '\n';
        File.WriteAllText(path, header);
    }

    string Vector3Header(string name)
    {
        return name + "_x" + '\t' + name + "_y" + '\t' + name + "_z" + '\t';
    }

    string Vector2Header(string name)
    {
        return name + "_x" + '\t' + name + "_y" + '\t';
    }

    // This function should take a MonoBehaviour and list of field names AT RUNTIME
    // the instance of the object can be hooked up by a public reference to MonoBehaviour
    // Reflection then allows you to dig down to the inheriting class type and use that
    public void GetFields()
    {
        BindingFlags bindingFlags = BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.Instance |
                            BindingFlags.Static;

        // Iterate through our save targets (each class instance) and get references to all the target fields and methods
        for (int i = 0; i < targetData.Length; i++)
        {
            // Make sure the instance actually exists
            if (targetData[i].scriptInstance == null)
                Debug.LogError("Null Script Reference!! Please remove any empty class references");

            Type classType = targetData[i].scriptInstance.GetType();

            List<FieldInfo> fieldList = new List<FieldInfo>();
            List<MethodInfo> methodList = new List<MethodInfo>();

            for (int j = 0; j < targetData[i].fieldNames.Length; j++)
            {
                FieldInfo fo = classType.GetField(targetData[i].fieldNames[j], bindingFlags);
                if (fo != null)
                {
                    fieldList.Add(fo);
                }
                else
                {
                    MethodInfo mo = classType.GetMethod(targetData[i].fieldNames[j], bindingFlags);
                    if (mo == null)
                    {
                        Debug.LogError("No Field or Method with name " + targetData[i].fieldNames[j] + " could be found.");
                    }
                    methodList.Add(mo);
                }
            }
            targetData[i].fieldInfos = fieldList.ToArray();
            targetData[i].methodInfos = methodList.ToArray();
        }
    }

    // Start is called before the first frame update
    public void Start()
    {
        GetFields();
        CreateFile();
    }

    void FixedUpdate()
    {
        count += Time.fixedDeltaTime;

        // If it's time to save some data
        if (count >= (1f / SaveFrequency))
        {
            Save();
        }
    }

    public void Save()
    {
        string saveData = "";
        for (int i = 0; i < targetData.Length; i++)
        {
            // Make sure the instance still exists
            if (targetData[i].scriptInstance == null)
            {
                // Just let the user know that we found a null reference
                Debug.LogWarning("Null instance found, saving will continue with NULL values for object " + i.ToString());
                // If it doesn't we're already commited to saving data, maybe we can just put a placeholder in the file
                for (int j = 0; j < targetData[i].fieldInfos.Length; j++)
                {
                    saveData += "NULL" + '\t';
                }
                for (int j = 0; j < targetData[i].methodInfos.Length; j++)
                {
                    saveData += "NULL" + '\t';
                }
            }
            else
            {
                saveData += GetFieldValues(targetData[i]);
                saveData += GetMethodValues(targetData[i]);
            }
        }
        saveData += '\n';
        File.AppendAllText(path, saveData);
        count -= (1f / SaveFrequency);
    }

    string GetFieldValues(TargetSaveData target)
    {
        string data = "";
        for (int j = 0; j < target.fieldInfos.Length; j++)
        {
            if (target.fieldInfos[j].FieldType == typeof(Vector3))
            {
                data += GetVector3String((Vector3)target.fieldInfos[j].GetValue(target.scriptInstance)) + '\t';
            }
            else
            {
                data += target.fieldInfos[j].GetValue(target.scriptInstance).ToString() + '\t';
            }
        }
        return data;
    }

    string GetMethodValues(TargetSaveData target)
    {
        string data = "";
        for (int j = 0; j < target.methodInfos.Length; j++)
        {
            if (target.methodInfos[j].ReturnType == typeof(Vector3))
            {
                data += GetVector3String((Vector3)target.methodInfos[j].Invoke(target.scriptInstance, new object[] { })) + '\t';
            }
            else
            {
                data += target.methodInfos[j].Invoke(target.scriptInstance, new object[] { }).ToString() + '\t';
            }
        }
        return data;
    }

    string GetVector3String(Vector3 vector)
    {
        return vector.x.ToString() + '\t' + vector.y.ToString() + '\t' + vector.z.ToString();
    }


    public void AddNewLine()
    {
        File.AppendAllText(path, "\n");
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}
