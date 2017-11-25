using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Deformable : MonoBehaviour
    , IInitializePotentialDragHandler
    , IBeginDragHandler
	, IDragHandler
    , IEndDragHandler
{
    public float _radius = 0.5f;
    public float _strength = 5f;
	public bool _symmetric = true;
	public bool _push = true;

    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;
    MeshCollider _meshCollider;

	IEnumerator Start()
    {
		yield return new WaitForEndOfFrame ();

        _meshFilter = GetComponentInChildren<MeshFilter>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _meshCollider = GetComponentInChildren<MeshCollider>();
    }
    

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Deformation begins!");
    }

	public void OnDrag(PointerEventData eventData)
    {
		Debug.Log(eventData.pointerCurrentRaycast.worldPosition + " frame count: " + Time.frameCount);

		Vector3[] vertices = _meshFilter.mesh.vertices;

        for (int i = 0; i < vertices.Length; ++i)
        {
			float dist = 0f;

			dist = _symmetric ?
				Mathf.Abs (vertices [i].y - eventData.pointerCurrentRaycast.worldPosition.y) : 
				Vector3.Distance (vertices [i], eventData.pointerCurrentRaycast.worldPosition);

			if (dist < _radius)
			{
				Vector3 dir = new Vector3(vertices[i].x, 0, vertices[i].z).normalized;
				vertices[i] += dir * _strength * (_push ? 1f : -1f) * Time.deltaTime * Falloff(dist, _radius);
			}
        }

		_meshFilter.mesh.vertices = vertices;
		_meshFilter.mesh.RecalculateNormals ();
    }

    float Falloff(float dist, float radius)
    {
        return Mathf.Clamp01(1f - dist / radius);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Deformation ends!");
		_meshCollider.sharedMesh = _meshFilter.mesh;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {

    }
}
