using UnityEngine;
using UnityEditor;
namespace SCG
{
    [CustomEditor(typeof(SimpleCityGenerator))]
    public class SimpleCityGeneratorEditor : Editor
    {
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            SimpleCityGenerator spawner = (SimpleCityGenerator)target;

            if (GUILayout.Button("Spawn Objects"))
            {
                spawner.Spawn();

            }

            if (GUILayout.Button("Remove Objects"))
            {
                spawner.DestroyAll();
            }

        }
    }

}