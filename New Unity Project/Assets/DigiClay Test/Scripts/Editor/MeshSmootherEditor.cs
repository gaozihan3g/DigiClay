using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshSmoother))]
public class MeshSmootherEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MeshSmoother ms = (MeshSmoother)target;
        if (GUILayout.Button("Smooth Mesh"))
        {
            ms.SmoothMesh();
        }
    }

}
