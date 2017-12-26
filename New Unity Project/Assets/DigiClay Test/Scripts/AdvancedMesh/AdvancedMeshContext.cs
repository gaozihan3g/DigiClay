using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

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
	void Start () {
		_advMesh = new AdvancedMesh(GetComponentInChildren<MeshFilter> ().mesh);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
