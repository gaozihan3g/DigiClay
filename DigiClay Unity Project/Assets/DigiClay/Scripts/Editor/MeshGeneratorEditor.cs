using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshGenerator))]
public class MeshGeneratorEditor : Editor {

    double t = 0f;

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector ();

		MeshGenerator mg = (MeshGenerator)target;


		if (GUILayout.Button ("Create Mesh")) {

            t = CodeExecutionTime.Execute(()=>{mg.CreateMesh();});
			
		}


        EditorGUILayout.HelpBox (string.Format("{0} \t axis segments\n" +
                                               "{1} \t height segments\n" +
                                               "{2} \t vertices\n" +
                                               "{3} \t triangles\n" +
                                               "{4} \t ms",
                                               mg.Segment, mg.VerticalSegment, mg.Vertices, mg.Triangles, t), MessageType.Info);
	}
}
