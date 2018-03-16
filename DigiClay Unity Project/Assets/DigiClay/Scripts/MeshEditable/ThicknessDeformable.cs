using DigiClay;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive;
using UnityEngine;

public class ThicknessDeformable : DeformableBase
{
    Vector3 m_orgHandLocalPos;
    Vector3 m_orgHandWorldPos;
    Vector3 m_prevHandWorldPos;
    HandRole m_role;

    float m_orgThicknessRatio;

    #region IColliderEventHandler implementation
    public override void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton)
            return;

        m_orgHandWorldPos = eventData.eventCaster.transform.position;
        // this will remove rotation
        m_orgHandLocalPos = m_orgHandWorldPos - transform.position;

        m_prevHandWorldPos = m_orgHandWorldPos;

        m_orgVertices = m_meshFilter.mesh.vertices;

        m_orgThicknessRatio = m_clayMeshContext.clayMesh.ThicknessRatio;

        //register undo
        DeformManager.Instance.RegisterUndo(this, m_orgVertices);

        if (OnDeformStart != null)
        {
            OnDeformStart.Invoke(this);
        }

        m_role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue);
    }

    public override void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton)
            return;

        var m_curHandWorldPos = eventData.eventCaster.transform.position;

        Debug.DrawLine(m_orgHandWorldPos, m_curHandWorldPos, Color.red);

        Vector3 offsetVector = m_curHandWorldPos - m_orgHandWorldPos;


        float verticalDelta = Mathf.Clamp(offsetVector.y, -m_clayMeshContext.clayMesh.Height, 0f);

        float thicknessDelta = verticalDelta / m_clayMeshContext.clayMesh.Height + 1f;

        // get thickness
        m_clayMeshContext.clayMesh.ThicknessRatio = m_orgThicknessRatio + thicknessDelta;

        // update mesh
        m_clayMeshContext.clayMesh.UpdateMesh();

        TriggerHaptic(m_role, m_prevHandWorldPos, m_curHandWorldPos);
    }

    public override void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton)
            return;

        m_meshCollider.sharedMesh = m_meshFilter.mesh;

        if (OnDeformEnd != null)
        {
            OnDeformEnd.Invoke(this);
        }
    }
    #endregion
}
