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

	Vector3 m_orgHandLocalPos = new Vector3();
	[SerializeField]
	Vector3 m_orgHandWorldPos = new Vector3();

	float[] m_orgRadiusList;

	#region IColliderEventHandler implementation
	public override void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;

		m_orgHandWorldPos = eventData.eventCaster.transform.position;
		m_orgHandLocalPos = m_orgHandWorldPos - transform.position;

		m_orgVertices = m_meshFilter.mesh.vertices;
		m_weightList = new List<float>();

		m_orgRadiusList = new float[m_clayMeshContext.clayMesh.RadiusList.Count];
		m_clayMeshContext.clayMesh.RadiusList.CopyTo(m_orgRadiusList);

		//register undo
		DeformManager.Instance.RegisterUndo(new DeformManager.UndoArgs(this, m_clayMeshContext.clayMesh.Height,
			m_clayMeshContext.clayMesh.ThicknessRatio, m_orgRadiusList, Time.frameCount));

		if (OnDeformStart != null)
		{
			OnDeformStart.Invoke(this);
		}
	}

	public override void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;

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

		m_meshFilter.mesh = m_clayMeshContext.clayMesh.Mesh;

		ViveInput.TriggerHapticPulse(role, DeformManager.Instance.MinDuration);
	}

	public override void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
	}
	#endregion
}
