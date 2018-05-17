using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DigiClay;

[CustomEditor(typeof(DataAnalysisManager))]
public class DataAnalysisManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DataAnalysisManager dam = (DataAnalysisManager)target;


        if (GUILayout.Button("Calculate Correlation"))
        {
            dam.EditorModeCalculation();
        }


        EditorGUILayout.HelpBox("DataAnalysisManager", MessageType.Info);
    }
}
