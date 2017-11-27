using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDataLogger : MonoBehaviour {

	Mesh _mesh;
	public float gizmosSize = 0.1f;
	public float gizmosDist = 0.1f;

	// Use this for initialization
	void Start () {

		_mesh = GetComponent<MeshFilter> ().mesh;

		Debug.Log (_mesh.vertexCount); // 88 = 20 + 20 + 2 + 

		for (int i = 0; i < _mesh.vertexCount; ++i) {
			Debug.Log (_mesh.vertices [i] + " , " + _mesh.uv [i] );
		}


		Debug.Log (_mesh.triangles.Length / 3);

		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnDrawGizmosSelected() {
		
		if (_mesh == null)
			_mesh = GetComponent<MeshFilter> ().sharedMesh;

		for (int i = 0; i < _mesh.vertexCount; ++i) {
			Gizmos.color = new Color (_mesh.uv [i].x, _mesh.uv [i].y, 0);
			Gizmos.DrawSphere(_mesh.vertices[i] + transform.position + i * Vector3.up * gizmosDist, gizmosSize);
		}
	}
}
