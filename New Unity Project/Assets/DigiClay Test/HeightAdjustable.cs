using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightAdjustable : MonoBehaviour {

	[Range(DigiClayConstant.MIN_HEIGHT, DigiClayConstant.MAX_HEIGHT)]
	public float _height = 1f;

	float _oldHeight;

	MeshFilter _meshFilter;
	MeshCollider _meshCollider;

	MeshGenerator _meshGenerator;
	float _originalHeight = 1f;


	Vector3[] _originalVertices;

	IEnumerator Start()
	{
		yield return new WaitForEndOfFrame ();

		_meshFilter = GetComponentInChildren<MeshFilter>();
		_meshCollider = GetComponentInChildren<MeshCollider>();

		// this could be null, if the mesh is not generated from code
		_meshGenerator = GetComponent<MeshGenerator> ();

		if (_meshGenerator != null) {
			_height = _originalHeight = _meshGenerator.Height;
		}

		_originalVertices = _meshFilter.mesh.vertices;

		_oldHeight = _height;
	}


	void Update () {
		if (SystemManager.Instance.Mode != SystemManager.EditMode.HeightControl)
			return;

		if (_height == _oldHeight)
			return;

		if (_meshFilter == null)
			return;

		Vector3[] newVerts = _meshFilter.mesh.vertices;

		for (int i = 0; i < newVerts.Length; ++i) {
			newVerts [i].y = _originalVertices [i].y * _height / _originalHeight;
		}

		_meshFilter.mesh.vertices = newVerts;

		_meshCollider.sharedMesh = _meshFilter.mesh;

		_oldHeight = _height;
	}
}
