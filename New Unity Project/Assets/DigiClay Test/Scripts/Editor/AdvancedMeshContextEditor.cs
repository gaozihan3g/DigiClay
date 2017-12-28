using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AdvancedMeshContext))]
public class AdvancedMeshContextEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AdvancedMeshContext amc = (AdvancedMeshContext)target;
        if (GUILayout.Button("Create Advanced Mesh"))
        {
            amc.CreateAdvMesh();
        }
    }

}
