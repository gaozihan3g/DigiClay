using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDataLogger : MonoBehaviour {

	Mesh _mesh;
	public float gizmosSize = 0.1f;

	// Use this for initialization
	void Start () {

		_mesh = GetComponent<MeshFilter> ().mesh;

		Debug.Log (_mesh.vertexCount); // 88 = 20 + 20 + 2 + 

		for (int i = 0; i < _mesh.vertexCount; ++i) {
			Debug.Log (_mesh.vertices [i]);
		}


		Debug.Log (_mesh.triangles.Length / 3);

		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDrawGizmos() {
		
		if (_mesh == null)
			_mesh = GetComponent<MeshFilter> ().mesh;

		for (int i = 0; i < _mesh.vertexCount; ++i) {
			Gizmos.DrawSphere(_mesh.vertices[i], gizmosSize);
		}
	}
}
