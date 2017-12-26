using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshGenerator))]
public class MeshGeneratorEditor : Editor {

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();

		MeshGenerator mg = (MeshGenerator)target;
		if (GUILayout.Button ("Create Mesh")) {
			mg.CreateMesh ();
		}


		EditorGUILayout.HelpBox ("haha", MessageType.Info);
	}
}
