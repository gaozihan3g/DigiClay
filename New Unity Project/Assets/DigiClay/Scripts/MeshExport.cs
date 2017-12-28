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
		OnScreenUIManager.Instance.AddCommand("Save Mesh " + identifier, ()=>{
			MeshIOManager.Instance.ExportMesh(_meshFilter.mesh);
		});
	}
}
