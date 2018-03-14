using System.Collections.Generic;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive;
using UnityEngine;

public class TwoHandedDeformable : DeformableBase
{
	public bool VisualDebug = true;

	public float m_heightBase = 1f;
	public float m_radialBase = 1f;
	public float m_heightDeltaPercentage = 1f;
	public float m_radialDeltaPercentage = 1f;
	public Vector3[] m_originalLocalPos = new Vector3[2];
	public Vector3[] m_currentLocalPos = new Vector3[2];
	public Vector3 m_originAvgLocalPos;
	public Vector3 m_averageDir;
	public float m_originDist;
	public float m_currentDist;

	Vector3[] m_previousWorldPosition = new Vector3[2];

	#region IColliderEventHandler implementation
	public override void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		DeformManager.Instance.SetHandStatus (role, true);

		//record original positions
		var casterWorldPosition = eventData.eventCaster.transform.position;

		m_previousWorldPosition[(int)role] = casterWorldPosition;

		m_originalLocalPos[(int)role] = transform.worldToLocalMatrix.MultiplyPoint(casterWorldPosition);

		//early out if both hands not ready
		if (!DeformManager.Instance.IsBothHandReady)
			return;

		m_originAvgLocalPos = (m_originalLocalPos [0] + m_originalLocalPos [1]) / 2f;

		m_orgVertices = m_meshFilter.mesh.vertices;

		//register undo
		DeformManager.Instance.RegisterUndo(this, m_orgVertices);

		m_originDist = Vector3.Distance (m_originalLocalPos [0], m_originalLocalPos [1]);

		m_weightList = new List<float>();

		for (int i = 0; i < m_orgVertices.Length; ++i)
		{
			float dist = 0f;

			dist = Mathf.Abs(m_orgVertices[i].y - m_originAvgLocalPos.y);


			float weight = Falloff( m_innerRadius, m_outerRadius, dist);
			m_weightList.Add(weight);
		}

		if (OnDeformStart != null)
		{
			OnDeformStart.Invoke(this);
		}
	}

	public override void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		if (!DeformManager.Instance.IsBothHandReady)
			return;

		//record original positions
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		var casterWorldPosition = eventData.eventCaster.transform.position;
		m_currentLocalPos[(int)role] = transform.worldToLocalMatrix.MultiplyPoint(casterWorldPosition);


		m_averageDir = (m_currentLocalPos [0] - m_originalLocalPos [0] + m_currentLocalPos [1] - m_originalLocalPos [1]) / 2f;

		m_currentDist = Vector3.Distance (m_currentLocalPos [0], m_currentLocalPos [1]);

		//visual debug
		if (VisualDebug)
		{
			for (int i = 0; i < 2; ++i)
			{
				var currentWorldPos = transform.localToWorldMatrix.MultiplyPoint (m_currentLocalPos[i]);
				var originalWorldPos = transform.localToWorldMatrix.MultiplyPoint(m_originalLocalPos[i]);
				Debug.DrawLine(currentWorldPos, originalWorldPos, Color.green);
			}

			var avgOriginalWorldPos = transform.localToWorldMatrix.MultiplyPoint ((m_originalLocalPos[0] + m_originalLocalPos[1]) / 2f);
			var avgCurrentWorldPos = transform.localToWorldMatrix.MultiplyPoint ((m_currentLocalPos[0] + m_currentLocalPos[1]) / 2f);

			Debug.DrawLine(avgOriginalWorldPos, avgCurrentWorldPos, Color.red);

			Debug.DrawLine(transform.localToWorldMatrix.MultiplyPoint (m_currentLocalPos[0]),
				transform.localToWorldMatrix.MultiplyPoint (m_currentLocalPos[1]), Color.blue);
		}


		float verticalDelta = m_averageDir.y;
		m_heightDeltaPercentage = verticalDelta / m_heightBase;

		/// method #1 - based on hand distance
		float distDelta = m_currentDist - m_originDist;
		m_radialDeltaPercentage = distDelta / m_radialBase;
		///

		Vector3[] newVerts = m_meshFilter.mesh.vertices;

		for (int i = 0; i < newVerts.Length; ++i)
		{
			//handle height change
			newVerts[i].y = m_orgVertices[i].y + m_orgVertices[i].y * m_heightDeltaPercentage;

			//handle deform
			//early out if weight is 0
			if (m_weightList [i] == 0f)
				continue;

//				Vector3 vertRadialDir = new Vector3 (newVerts [i].x, 0f, newVerts [i].z);
				newVerts[i].x = m_orgVertices[i].x + m_orgVertices[i].x * m_radialDeltaPercentage * m_strength * m_weightList [i];
				newVerts[i].z = m_orgVertices[i].z + m_orgVertices[i].z * m_radialDeltaPercentage * m_strength * m_weightList [i];
		}

		m_meshFilter.mesh.vertices = newVerts;

		if (m_clayMeshContext != null)
			m_clayMeshContext.clayMesh.RecalculateNormals ();
		else
			m_meshFilter.mesh.RecalculateNormals();

		TriggerHaptic (role, m_previousWorldPosition [(int)role], casterWorldPosition);

		m_previousWorldPosition[(int)role] = casterWorldPosition;
	}

	public override void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;

		
		m_meshCollider.sharedMesh = m_meshFilter.mesh;

		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		DeformManager.Instance.SetHandStatus (role, false);

		//reset position
		m_originalLocalPos[(int)role] = Vector3.zero;
		m_currentLocalPos[(int)role] = Vector3.zero;

		if (OnDeformEnd != null)
		{
			OnDeformEnd.Invoke(this);
		}
	}

	#endregion
}
