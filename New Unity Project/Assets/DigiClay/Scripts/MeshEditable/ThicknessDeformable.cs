using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Pose = HTC.UnityPlugin.PoseTracker.Pose;
using System.Collections;
using HTC.UnityPlugin.Vive;
using DigiClay;

public class ThicknessDeformable : DeformableBase
{
    public float m_heightDeltaPercentage = 0f;

    Vector3 m_originalLocalPos;
    Vector3 m_previousWorldPosition;
    HandRole m_role;
    

    public override void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton) { return; }

        var casterWorldPosition = eventData.eventCaster.transform.position;

        m_previousWorldPosition = casterWorldPosition;

        m_originalVertices = m_meshFilter.mesh.vertices;

        //register undo
        DeformManager.Instance.RegisterUndo(this, m_originalVertices);

        // this will remove rotation
        m_originalLocalPos = casterWorldPosition - transform.position;

        if (OnDeformStart != null)
        {
            OnDeformStart.Invoke(this);
        }

        m_role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue);
    }

    public override void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton) { return; }

        var currentWorldPosition = eventData.eventCaster.transform.position;

        //        var originalWorldPosition = transform.localToWorldMatrix.MultiplyPoint(_originalLocalPos);
        var originalWorldPosition = m_originalLocalPos + transform.position;

        Debug.DrawLine(originalWorldPosition, currentWorldPosition, Color.red);

        Vector3 offsetVector = currentWorldPosition - originalWorldPosition;

        float verticalDelta = Mathf.Clamp(offsetVector.y, -m_clayMeshContext.clayMesh.Height, 0f);

        m_heightDeltaPercentage = verticalDelta / m_clayMeshContext.clayMesh.Height + 1f;

        m_clayMeshContext.clayMesh.ThicknessRatio = m_heightDeltaPercentage;

        Vector3[] newVerts = m_meshFilter.mesh.vertices;

        for (int i = 0; i < newVerts.Length; ++i)
        {
            if (m_clayMeshContext.clayMesh.GetVertexTypeFromIndex(i) == ClayMesh.VertexType.InnerSide)
            {
                // inner side
                //m_originalVertices[i] = m_originalVertices[i - m_clayMeshContext.clayMesh.RadiusList.Count] - vertNormalDir * m_clayMeshContext.clayMesh.Thickness;
                newVerts[i] = newVerts[i - m_clayMeshContext.clayMesh.RadiusList.Count] * (1f - m_clayMeshContext.clayMesh.ThicknessRatio);
            }
            else if (m_clayMeshContext.clayMesh.GetVertexTypeFromIndex(i) == ClayMesh.VertexType.InnerBottomEdge)
            {
                // inner bottom
                newVerts[i] =
                    newVerts[i -
                                       (m_clayMeshContext.clayMesh.RadiusList.Count * 2 + 1 + m_clayMeshContext.clayMesh.Column)];
            }
        }

        m_meshFilter.mesh.vertices = newVerts;

        if (m_clayMeshContext != null)
            m_clayMeshContext.clayMesh.RecalculateNormals();
        else
            m_meshFilter.mesh.RecalculateNormals();

    }

    public override void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton) { return; }

        //TODO remesh!

        m_meshCollider.sharedMesh = m_meshFilter.mesh;

        if (OnDeformEnd != null)
        {
            OnDeformEnd.Invoke(this);
        }
    }
}
