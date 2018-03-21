using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExport : MonoBehaviour {

	public int identifier = 1;
	// Use this for initialization
	MeshFilter _meshFilter;
	void Awake()
	{
		_meshFilter = GetComponent<MeshFilter> ();
	}

	void Start () {
		MeshIOManager.Instance.Mesh = _meshFilter.mesh;
	}
}
