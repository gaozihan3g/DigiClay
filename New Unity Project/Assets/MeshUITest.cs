using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshUITest : MonoBehaviour {

	Mesh _mesh;

	// Use this for initialization
	void Start () {
		_mesh = GetComponent<MeshFilter> ().mesh;


		for (int i = 0; i < _mesh.vertexCount; i++) {
			if (Mathf.Abs(_mesh.vertices [i].y) < 0.0001f)
				Debug.Log (_mesh.vertices [i].ToString("F3") + " index " + i + " uv " + _mesh.uv[i].ToString("F3"));
		}

		for (int i = 0; i < _mesh.triangles.Length; i+=3) {
			if (_mesh.triangles [i] == 368 ||
				_mesh.triangles [i+1] == 368 ||
				_mesh.triangles [i+2] == 368 ||
				_mesh.triangles [i] == 413 ||
				_mesh.triangles [i+1] == 413 ||
				_mesh.triangles [i+2] == 413 ||
				_mesh.triangles [i] == 220 ||
				_mesh.triangles [i+1] == 220 ||
				_mesh.triangles [i+2] == 220)
				Debug.Log (string.Format ("{0} {1} {2}", _mesh.triangles [i], _mesh.triangles [i+1], _mesh.triangles [i+2]));
		}












	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
