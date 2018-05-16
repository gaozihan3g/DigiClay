using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HighlightMesh))]
public class HighlightMeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HighlightMesh hm = (HighlightMesh)target;


        if (GUILayout.Button("Create Mesh"))
        {
            hm.UpdateMesh();
        }
    }
}
