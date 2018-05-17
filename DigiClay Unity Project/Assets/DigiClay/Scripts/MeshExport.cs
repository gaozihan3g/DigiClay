using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshExport : MonoBehaviour {

	public int identifier = 1;
	// Use this for initialization
	MeshFilter _meshFilter;
    ClayMeshContext _cmc;
	void Awake()
	{
		_meshFilter = GetComponent<MeshFilter> ();
        _cmc = GetComponent<ClayMeshContext>();
	}

	void Start () {
		MeshIOManager.Instance.Mesh = _meshFilter.sharedMesh;
        MeshIOManager.Instance.ClayMesh = _cmc.clayMesh;
	}
}
