using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

public class AdvancedMeshTester : MonoBehaviour {

	AdvancedMesh _advMesh;

	// Use this for initialization
	void Start () {
		
		Mesh mesh = new Mesh ();

		List<Vector3> verticeList = new List<Vector3> ();

		verticeList.Add(new Vector3(0f, 0f, 0f));
		verticeList.Add(new Vector3(0f, 1f, 0f));
		verticeList.Add(new Vector3(1f, 0f, 0f));

		List<int> triangleList = new List<int> ();

		triangleList.Add (0);
		triangleList.Add (1);
		triangleList.Add (2);

		mesh.vertices = verticeList.ToArray ();
		mesh.triangles = triangleList.ToArray ();

		mesh.RecalculateNormals ();

		_advMesh = new AdvancedMesh (mesh);

		_advMesh.PrintAllHalfEdges ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
