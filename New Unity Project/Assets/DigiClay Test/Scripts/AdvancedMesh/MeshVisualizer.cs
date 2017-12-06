using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshVisualizer : MonoBehaviour {

	public List<int> _vertexIndices;
	public List<Color> _vertexColors;
	public float size = 1f;

	public MeshFilter _meshFilter;
	Mesh _mesh;

	// Use this for initialization
	void Start () {

		if (_meshFilter == null)
			_meshFilter = GetComponent<MeshFilter> ();

		_mesh = _meshFilter.mesh;
	}

	void OnDrawGizmos()
	{
		if (_mesh == null)
			return;
		
		if (_vertexIndices == null)
			return;

		for (int i = 0; i < _vertexIndices.Count; ++i) {
			var vi = _vertexIndices [i];
			Gizmos.color = _vertexColors [i];
			Gizmos.DrawSphere (_mesh.vertices [vi] + transform.position, size);
		}

	}
}
