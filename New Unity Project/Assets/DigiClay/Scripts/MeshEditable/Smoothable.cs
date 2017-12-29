using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.ColliderEvent;
using DigiClay;
using mattatz.MeshSmoothingSystem;

public class Smoothable : DeformableBase
{
	public int m_iterations = 1;

	#region IColliderEventHandler implementation
	public override void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		HapticManager.Instance.StartHaptic (role);
	}

	public override void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;

		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		var casterWorldPosition = eventData.eventCaster.transform.position;
		var localPos = transform.worldToLocalMatrix.MultiplyPoint(casterWorldPosition);
		m_meshFilter.mesh = MeshSmoothing.LaplacianFilter(m_meshFilter.mesh, m_iterations, m_clayMeshContext.clayMesh.IsFeaturePoints.ToArray (), localPos, m_outerRadius);
	}

	public override void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		HapticManager.Instance.EndHaptic (role);
	}
	#endregion
}
