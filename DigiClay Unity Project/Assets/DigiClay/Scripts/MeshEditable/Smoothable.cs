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
	Vector3 m_prevHandWorldPos;

	#region IColliderEventHandler implementation
	public override void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		base.OnColliderEventDragStart (eventData);

		m_prevHandWorldPos = eventData.eventCaster.transform.position;

		m_weightList = new List<float> ();
	}

	public override void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		base.OnColliderEventDragUpdate (eventData);

		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);

        var curHandWorldPos = eventData.eventCaster.transform.position;
        var curHandLocalPos = curHandWorldPos - transform.position;

		m_weightList.Clear();

		for (int i = 0; i < m_orgVertices.Length; ++i)
		{
			float dist = 0f;
			dist = Mathf.Abs(m_orgVertices[i].y - curHandLocalPos.y);

			float weight = Falloff( m_innerRadius, m_outerRadius, dist);
			m_weightList.Add(weight);
		}

		m_clayMeshContext.clayMesh.LaplacianSmooth(m_weightList);
		// update mesh
		m_clayMeshContext.clayMesh.UpdateMesh();

		UpdateHapticStrength(role, m_prevHandWorldPos, curHandWorldPos);
		m_prevHandWorldPos = curHandWorldPos;
	}

	public override void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		base.OnColliderEventDragEnd (eventData);
	}
	#endregion
}
