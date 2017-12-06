using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DigiClay;

[RequireComponent(typeof(AdvancedMeshContext))]
public class Smoothable : MonoBehaviour
	, IInitializePotentialDragHandler
	, IBeginDragHandler
	, IDragHandler
	, IEndDragHandler
{

	[Range(0.01f, 1f)]
	public float _radius = 0.5f;
	public bool _symmetric = true;
	MeshFilter _meshFilter;
	MeshCollider _meshCollider;
	AdvancedMeshContext _advMeshContext;

	void Awake()
	{
		_meshFilter = GetComponentInChildren<MeshFilter>();
		_meshCollider = GetComponentInChildren<MeshCollider>();
	}

	// Use this for initialization
	void Start () {
		_advMeshContext = GetComponent<AdvancedMeshContext> ();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		if (SystemManager.Instance.Mode != SystemManager.EditMode.Smooth)
			return;

		Debug.Log("Smoothing begins!");
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (SystemManager.Instance.Mode != SystemManager.EditMode.Smooth)
			return;

		Vector3[] vertices = _meshFilter.mesh.vertices;

		for (int i = 0; i < vertices.Length; ++i)
		{
			float dist = 0f;

			dist = _symmetric ?
				Mathf.Abs (vertices [i].y - eventData.pointerCurrentRaycast.worldPosition.y) : 
				Vector3.Distance (vertices [i], eventData.pointerCurrentRaycast.worldPosition);

			if (dist < _radius)
			{
				_advMeshContext.AdvMesh.SmoothVertex (i);
			}
		}
//		_meshFilter.mesh.vertices = vertices;
		_meshFilter.mesh.RecalculateNormals ();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (SystemManager.Instance.Mode != SystemManager.EditMode.Smooth)
			return;

		Debug.Log("Smoothing ends!");
		_meshCollider.sharedMesh = _meshFilter.mesh;
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{

	}
}
