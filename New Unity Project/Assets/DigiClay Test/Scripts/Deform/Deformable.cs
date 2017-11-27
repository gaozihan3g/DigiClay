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
	[Range(0.01f, 1f)]
    public float _radius = 0.5f;
	[Range(0.01f, 1f)]
    public float _strength = 0.01f;
	public bool _symmetric = true;
	public bool _push = true;

    MeshFilter _meshFilter;
    MeshCollider _meshCollider;

	IEnumerator Start()
    {
		yield return new WaitForEndOfFrame ();

        _meshFilter = GetComponentInChildren<MeshFilter>();
        _meshCollider = GetComponentInChildren<MeshCollider>();
    }

	void OnDrawGizmos()
	{
		
	}

    public void OnBeginDrag(PointerEventData eventData)
    {
		if (SystemManager.Instance.Mode != SystemManager.EditMode.Sculpture)
			return;
		
        Debug.Log("Deformation begins!");
    }

	public void OnDrag(PointerEventData eventData)
    {
		if (SystemManager.Instance.Mode != SystemManager.EditMode.Sculpture)
			return;

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
				Vector3 radialVec = new Vector3 (vertices [i].x, 0, vertices [i].z);

				if (radialVec.sqrMagnitude > (DigiClayConstant.MAX_RADIUS * DigiClayConstant.MAX_RADIUS) ||
					radialVec.sqrMagnitude < (DigiClayConstant.MIN_RADIUS * DigiClayConstant.MIN_RADIUS))
					break;

				Vector3 radialUnitDir = radialVec.normalized;

				int outerSign = Vector3.Angle (eventData.pointerCurrentRaycast.worldNormal, radialUnitDir) < 90 ? 1 : -1;

				int pushSign = _push ? -1 : 1;

				if (_symmetric)
					vertices[i] += radialUnitDir * _strength * Time.deltaTime * Falloff(dist, _radius) * pushSign;
				else
					vertices[i] += radialUnitDir * _strength * Time.deltaTime * Falloff(dist, _radius) * pushSign * outerSign;
			}
        }

		_meshFilter.mesh.vertices = vertices;
		_meshFilter.mesh.RecalculateNormals ();
    }

    float Falloff(float dist, float radius)
    {
//        return Mathf.Clamp01(1f - dist / radius);

		return Mathf.Clamp01 (Mathf.Pow (360.0f, -Mathf.Pow (dist / radius, 2.5f) - 0.01f));

    }

    public void OnEndDrag(PointerEventData eventData)
    {
		if (SystemManager.Instance.Mode != SystemManager.EditMode.Sculpture)
			return;
		
        Debug.Log("Deformation ends!");
		_meshCollider.sharedMesh = _meshFilter.mesh;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {

    }
}
