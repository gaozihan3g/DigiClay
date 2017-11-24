using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Pose = HTC.UnityPlugin.PoseTracker.Pose;

public class Deformable : MonoBehaviour
    , IInitializePotentialDragHandler
    , IBeginDragHandler
    , IDragHandler
    , IEndDragHandler
{
    public float radius = 0.5f;
    public float strength = 5f;

    [SerializeField]
    Mesh _mesh;

    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;
    MeshCollider _meshCollider;

    void Start()
    {
        _meshFilter = GetComponentInChildren<MeshFilter>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _meshCollider = GetComponentInChildren<MeshCollider>();
        _mesh = _meshFilter.mesh;
    }
    

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Deformation begins!");
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log(eventData.pointerCurrentRaycast.worldPosition);

        Vector3[] vertices = _mesh.vertices;

        for (int i = 0; i < vertices.Length; ++i)
        {
            float dist = Vector3.Distance(vertices[i], eventData.pointerCurrentRaycast.worldPosition);
            if (dist < radius)
            {
                Vector3 dir = new Vector3(vertices[i].x, 0, vertices[i].z).normalized;
                vertices[i] += dir * strength * Time.deltaTime * Falloff(dist, radius);
            }
        }

        _mesh.vertices = vertices;

        _meshCollider.sharedMesh = _mesh;
    }

    float Falloff(float dist, float radius)
    {
        return Mathf.Clamp01(1f - dist / radius);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Deformation ends!");
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {

    }

    private Pose GetEventPose(PointerEventData eventData)
    {
        var cam = eventData.pointerPressRaycast.module.eventCamera;
        var ray = cam.ScreenPointToRay(eventData.position);
        return new Pose(ray.origin, Quaternion.LookRotation(ray.direction, cam.transform.up));
    }
}
