using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UserStudyManager))]
public class UserStudyManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        

        UserStudyManager usm = (UserStudyManager)target;


        if (GUILayout.Button("Initialize"))
        {
            usm.Init();
        }

        if (GUILayout.Button("Start Task"))
        {
            usm.StartTask();
        }

        if (GUILayout.Button("End Task"))
        {
            usm.EndTask();
        }

        if (GUILayout.Button("Save Data"))
        {
            usm.SaveData();
        }

        EditorGUILayout.HelpBox("UserStudyManager", MessageType.Info);

        DrawDefaultInspector();
    }
}
