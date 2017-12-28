using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

[ExecuteInEditMode]
public class AdvancedMeshContext : MonoBehaviour {

	[SerializeField]
	private AdvancedMesh _advMesh;

	public AdvancedMesh AdvMesh {
		get {
			return _advMesh;
		}
		set {
			_advMesh = value;
		}
	}

	// Use this for initialization
	public void CreateAdvMesh () {
        _advMesh = new AdvancedMesh(GetComponentInChildren<MeshFilter> ().sharedMesh);
	}
	
}
