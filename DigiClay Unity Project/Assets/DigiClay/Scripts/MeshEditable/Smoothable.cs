using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.ColliderEvent;
using DigiClay;

public class Smoothable : DeformableBase
{
	public int m_iterations = 1;

	#region IColliderEventHandler implementation
	public override void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		m_orgVertices = m_meshFilter.mesh.vertices;
		//register undo
		DeformManager.Instance.RegisterUndo(this, m_orgVertices);
	}

	public override void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;

		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		var casterWorldPosition = eventData.eventCaster.transform.position;
		var localPos = transform.worldToLocalMatrix.MultiplyPoint(casterWorldPosition);

		if (DeformManager.Instance.IsHCSmoothing)
        	m_meshFilter.mesh = MeshSmoothing.HCFilter(m_meshFilter.mesh, m_iterations, 0.5f, 0.75f, m_clayMeshContext.clayMesh.IsFeaturePoints.ToArray(), localPos, m_outerRadius);
		else
			m_meshFilter.mesh = MeshSmoothing.LaplacianFilter(m_meshFilter.mesh, m_iterations, m_clayMeshContext.clayMesh.IsFeaturePoints.ToArray (), localPos, m_outerRadius);

		ViveInput.TriggerHapticPulse(role, DeformManager.Instance.MinDuration);
	}

	public override void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
	}
	#endregion
}
