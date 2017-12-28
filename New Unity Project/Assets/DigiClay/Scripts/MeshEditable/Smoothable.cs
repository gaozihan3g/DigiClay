using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DigiClay;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;
using mattatz.MeshSmoothingSystem;

public class Smoothable : MonoBehaviour
{
	[Range(0.01f, 1f)]
	public float _radius = 0.5f;

	public int m_iterations = 1;

	public Transform cursor;

	MeshFilter _meshFilter;
	MeshCollider _meshCollider;

	ClayMeshContext m_cmc;

	bool[] _isFeaturePoints;

	void Awake()
	{
		_meshFilter = GetComponentInChildren<MeshFilter>();
		_meshCollider = GetComponentInChildren<MeshCollider>();
		m_cmc = GetComponent<ClayMeshContext>();
	}

	// Use this for initialization
	void Start () {
		_isFeaturePoints = m_cmc.clayMesh.IsFeaturePoints.ToArray ();
	}

	void Update()
	{
		if (ViveInput.GetPress (HandRole.RightHand, ControllerButton.Grip)) {
			Debug.Log ("right hand is gripping!");

			var localPos = transform.worldToLocalMatrix.MultiplyPoint (cursor.position);

			_meshFilter.mesh = MeshSmoothing.LaplacianFilter(_meshFilter.mesh, m_iterations, _isFeaturePoints, localPos, _radius);
		}
	}
}
