using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DigiClay;

[CustomEditor(typeof(ClayObject))]
public class ClayObjectEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ClayObject co = (ClayObject)target;


        if (GUILayout.Button("Link"))
        {
            co.Link();
        }


        EditorGUILayout.HelpBox("Clay", MessageType.Info);
    }
}
