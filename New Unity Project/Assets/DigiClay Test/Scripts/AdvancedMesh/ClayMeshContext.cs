using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

[ExecuteInEditMode]
public class ClayMeshContext : MonoBehaviour {

	[SerializeField]
	ClayMesh m_clayMesh;

	MeshFilter m_meshFilter;
	MeshCollider m_meshCollider;

	public ClayMesh clayMesh {
		get {
			return m_clayMesh;
		}
		set {
			m_clayMesh = value;
		}
	}

	void Awake()
	{
		m_meshFilter = GetComponent<MeshFilter>();
		m_meshCollider = GetComponent<MeshCollider>();
	}
		
	void Start () {
		m_meshFilter.mesh = clayMesh.mesh;
		m_meshCollider.sharedMesh = clayMesh.mesh;
	}

	void Update () {
		
	}
}
