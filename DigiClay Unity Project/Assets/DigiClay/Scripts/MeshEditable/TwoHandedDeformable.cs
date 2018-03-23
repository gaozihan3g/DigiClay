//using System.Collections.Generic;
//using HTC.UnityPlugin.ColliderEvent;
//using HTC.UnityPlugin.Vive;
//using UnityEngine;
//using DigiClay;

//public class TwoHandedDeformable : DeformableBase
//{
//	[SerializeField]
//	bool VisualDebug = true;
//	[SerializeField]
//	bool HeightChangeEnabled = true;
//	[SerializeField]
//	bool DeformEnabled = true;
//	[SerializeField]
//	bool RadialSmoothEnabled = true;

//	// two handed
//    Vector3[] m_orgHandLocalPosAry = new Vector3[2];
//	float[] m_orgHand2DLocalDistAry = new float[2];
//	Vector3[] m_orgHandWorldPosAry = new Vector3[2];
//	Vector3[] m_prevHandWorldPosAry = new Vector3[2];

//	Vector3[] m_curHandLocalPosAry = new Vector3[2];
//	Vector3[] m_curHandWorldPosAry = new Vector3[2];
//	Vector3[] m_curHand2DLocalPosAry = new Vector3[2];

//	Vector3[] closest2DPointPosAry = new Vector3[2];
//	Vector3[] offsetVectorAry = new Vector3[2];
//	float[] offsetDistAry = new float[2];

//    float m_orgHeight;

//	#region IColliderEventHandler implementation
//	public override void OnColliderEventDragStart (ColliderButtonEventData eventData)
//	{
//		if (m_tool != DeformManager.Instance.ToolState)
//			return;
		
//		if (eventData.button != m_deformButton)
//			return;

//		// save hand pos
//		int roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue;
//		HandRole role = (HandRole)roleIndex;

//		m_orgHandWorldPosAry[roleIndex] = eventData.eventCaster.transform.position;
//		m_orgHandLocalPosAry[roleIndex] = m_orgHandWorldPosAry[roleIndex] - transform.position;
//		m_prevHandWorldPosAry[roleIndex] = m_orgHandWorldPosAry[roleIndex];

//		m_orgHand2DLocalDistAry[roleIndex] = Vector3.ProjectOnPlane(m_orgHandLocalPosAry[roleIndex], Vector3.up).magnitude;

//		DeformManager.Instance.IsDeforming (role, true);

//		// check condition
//		if (!DeformManager.Instance.AreBothHandsReady)
//			return;
		
//		//below ONLY EXECUTE ONCE

//		// origin data snapshot
//		m_orgVertices = m_meshFilter.sharedMesh.vertices;
//		m_orgRadiusList = new float[m_clayMeshContext.clayMesh.RadiusList.Count];
//		m_clayMeshContext.clayMesh.RadiusList.CopyTo(m_orgRadiusList);
//		m_orgHeight = m_clayMeshContext.clayMesh.Height;

//		//register undo
//		DeformManager.Instance.RegisterUndo(new DeformManager.UndoArgs(this, m_clayMeshContext.clayMesh.Height,
//			m_clayMeshContext.clayMesh.ThicknessRatio, m_orgRadiusList, Time.frameCount));
//		DeformManager.Instance.ClearRedo();

//		//additional init
//		var avgOrgHandLocalPos = (m_orgHandLocalPosAry[0] + m_orgHandLocalPosAry[1]) / 2f;

//        UpdateWeightList(avgOrgHandLocalPos);

//		if (OnDeformStart != null)
//		{
//			OnDeformStart.Invoke(this);
//		}
//	}

//	public override void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
//	{
//		if (m_tool != DeformManager.Instance.ToolState)
//			return;
		
//		if (eventData.button != m_deformButton)
//			return;

//		if (!DeformManager.Instance.AreBothHandsReady)
//			return;

//		int roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue;
//		HandRole role = (HandRole)roleIndex;

//		m_curHandWorldPosAry[roleIndex] = eventData.eventCaster.transform.position;
//		m_curHandLocalPosAry[roleIndex] = m_curHandWorldPosAry[roleIndex] - transform.position;
//		// get 2D hand local pos
//		m_curHand2DLocalPosAry[roleIndex] = Vector3.ProjectOnPlane (m_curHandLocalPosAry [roleIndex], Vector3.up);
//		// get closest point
//		closest2DPointPosAry[roleIndex] = m_curHand2DLocalPosAry[roleIndex].normalized * m_orgHand2DLocalDistAry[roleIndex];
//		// offset vector between
//		offsetVectorAry[roleIndex] = m_curHand2DLocalPosAry[roleIndex] - closest2DPointPosAry[roleIndex];

//		// ##
//		// dist of offset
//		offsetDistAry[roleIndex] = offsetVectorAry[roleIndex].magnitude;

//		UpdateHapticStrength(role, m_prevHandWorldPosAry[roleIndex], m_curHandWorldPosAry[roleIndex]);
//		m_prevHandWorldPosAry[roleIndex] = m_curHandWorldPosAry[roleIndex];


//		//below ONLY EXECUTE ONCE
//		if (roleIndex != 0)
//			return;

//		if (DeformEnabled) {
//			// get the index of min dist
//			int id = (offsetDistAry [0] < offsetDistAry [1]) ? 0 : 1;
//			// the distance of the closer hand
//			float curHand2DLocalDist = m_curHand2DLocalPosAry[id].magnitude;

//			// ## sign, compare the dist to get the sign
//			float sign = (curHand2DLocalDist > m_orgHand2DLocalDistAry[id]) ? 1f : -1f;

//			// ## update MATRIX
//			for (int i = 0; i < m_clayMeshContext.clayMesh.RadiusList.Count; ++i)
//			{
//				//early out
//				if (Mathf.Approximately(m_weightList[i], 0f))
//					continue;
//				//deform
//				m_clayMeshContext.clayMesh.Deform(i, m_orgRadiusList[i], sign, offsetDistAry[id] * DeformManager.Instance.DeformRatio, m_weightList[i]);
//			}
//		}

//		if (RadialSmoothEnabled) {
//			// ## radial smooth
//	        m_clayMeshContext.clayMesh.RadialSmooth(m_weightList);
//		}

//		if (HeightChangeEnabled) {
//			var avgHandDeltaPos = (m_curHandLocalPosAry[0] - m_orgHandLocalPosAry[0] + m_curHandLocalPosAry[1] - m_orgHandLocalPosAry[1]) / 2f;
//			// ## heightDelta
//			float heightDelta = avgHandDeltaPos.y * DeformManager.Instance.DeformRatio;
//			// ## update HEIGHT
//			m_clayMeshContext.clayMesh.Height = m_orgHeight + heightDelta;
//		}

//		m_clayMeshContext.clayMesh.UpdateMesh();

//		//visual debug
//		if (VisualDebug)
//		{
//			Debug.DrawLine(m_curHandWorldPosAry[0], m_orgHandWorldPosAry[0], Color.green);
//			Debug.DrawLine(m_curHandWorldPosAry[1], m_orgHandWorldPosAry[1], Color.green);

//			var avgOrgHandWorldPos = (m_orgHandWorldPosAry[0] + m_orgHandWorldPosAry[1]) / 2f;
//			var avgCurHandWorldPos = (m_curHandWorldPosAry[0] + m_curHandWorldPosAry[1]) / 2f;

//			Debug.DrawLine(avgOrgHandWorldPos, avgCurHandWorldPos, Color.red);
//			Debug.DrawLine(m_curHandWorldPosAry[0], m_curHandWorldPosAry[1], Color.blue);
//		}
//	}

//	public override void OnColliderEventDragEnd (ColliderButtonEventData eventData)
//	{
//		if (m_tool != DeformManager.Instance.ToolState)
//			return;
		
//		if (eventData.button != m_deformButton)
//			return;

//		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
//		DeformManager.Instance.IsDeforming (role, false);

//		if (!DeformManager.Instance.AreBothHandsFree)
//			return;

//		//below ONLY EXECUTE ONCE
//		m_meshCollider.sharedMesh = m_meshFilter.sharedMesh;

//		if (OnDeformEnd != null)
//		{
//			OnDeformEnd.Invoke(this);
//		}
//	}

//	#endregion
//}
