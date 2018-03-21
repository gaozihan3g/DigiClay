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

	[SerializeField]
	float verticalDelta;
	[SerializeField]
	float thicknessDelta;

    #region IColliderEventHandler implementation
    public override void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton)
            return;

        m_orgHandWorldPos = eventData.eventCaster.transform.position;
        // this will remove rotation
        m_orgHandLocalPos = m_orgHandWorldPos - transform.position;

        m_prevHandWorldPos = m_orgHandWorldPos;

		m_orgVertices = m_meshFilter.sharedMesh.vertices;

        m_orgThicknessRatio = m_clayMeshContext.clayMesh.ThicknessRatio;

		//register undo
		DeformManager.Instance.RegisterUndo(new DeformManager.UndoArgs(this, m_clayMeshContext.clayMesh.Height,
			m_clayMeshContext.clayMesh.ThicknessRatio, null, Time.frameCount));
		DeformManager.Instance.ClearRedo();

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

		var curHandWorldPos = eventData.eventCaster.transform.position;

        Debug.DrawLine(m_orgHandWorldPos, curHandWorldPos, Color.red);

        Vector3 offsetVector = curHandWorldPos - m_orgHandWorldPos;

		verticalDelta = offsetVector.y;

		thicknessDelta = verticalDelta / m_clayMeshContext.clayMesh.Height;

        // get thickness 0 - 1
		m_clayMeshContext.clayMesh.ThicknessRatio = Mathf.Clamp01(m_orgThicknessRatio + thicknessDelta);

        // update mesh
        m_clayMeshContext.clayMesh.UpdateMesh();

        UpdateHapticStrength(m_role, m_prevHandWorldPos, curHandWorldPos);
		m_prevHandWorldPos = curHandWorldPos;
    }

    public override void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton)
            return;

		m_meshCollider.sharedMesh = m_meshFilter.sharedMesh;

		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		HapticManager.Instance.SetRoleStrength(role, 0f);

        if (OnDeformEnd != null)
        {
            OnDeformEnd.Invoke(this);
        }
    }
    #endregion
}
