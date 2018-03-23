//using System.Collections.Generic;
//using HTC.UnityPlugin.ColliderEvent;
//using HTC.UnityPlugin.Vive;
//using UnityEngine;
//using DigiClay;

//public class OneHandedDeformable : DeformableBase
//{
//	float m_orgHand2DLocalDist;

//	#region IColliderEventHandler implementation
//	public override void OnColliderEventDragStart(ColliderButtonEventData eventData)
//	{
//		if (m_tool != DeformManager.Instance.ToolState)
//			return;
		
//		if (eventData.button != m_deformButton)
//			return;

//		if (DeformManager.Instance.AreBothHandsReady)
//			return;

//		//basic init
//		base.OnColliderEventDragStart (eventData);

//		//additional init
//		m_orgHand2DLocalDist = Vector3.ProjectOnPlane(m_orgHandLocalPos, Vector3.up).magnitude;

//        UpdateWeightList(m_orgHandLocalPos);
//	}

//	public override void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
//	{
//		if (m_tool != DeformManager.Instance.ToolState)
//			return;
		
//		if (eventData.button != m_deformButton)
//			return;

//		if (DeformManager.Instance.AreBothHandsReady)
//			return;

//		m_curHandWorldPos = eventData.eventCaster.transform.position;
//		var curHandLocalPos = m_curHandWorldPos - transform.position;

//		Debug.DrawLine(m_orgHandWorldPos, m_curHandWorldPos, Color.red);

//		// get 2D hand local pos
//		var curHand2DLocalPos = Vector3.ProjectOnPlane (curHandLocalPos, Vector3.up);
//		// get closest point
//		var closest2DPointPos = curHand2DLocalPos.normalized * m_orgHand2DLocalDist;
//		// offset vector between
//		var offsetVector = curHand2DLocalPos - closest2DPointPos;

//		// ##
//		// dist of offset
//		var offsetDist = offsetVector.magnitude;

//		var curHand2DLocalDist = curHand2DLocalPos.magnitude;

//		// ## sign
//		var sign = (curHand2DLocalDist > m_orgHand2DLocalDist) ? 1f : -1f;

//		// ## update MATRIX
//		for (int i = 0; i < m_clayMeshContext.clayMesh.RadiusList.Count; ++i)
//		{
//			//early out
//			if (Mathf.Approximately(m_weightList[i], 0f))
//				continue;
//			//deform
//			m_clayMeshContext.clayMesh.Deform(i, m_orgRadiusList[i], sign, offsetDist * DeformManager.Instance.DeformRatio, m_weightList[i]);
//		}

//		base.OnColliderEventDragUpdate (eventData);
//	}

//	public override void OnColliderEventDragEnd(ColliderButtonEventData eventData)
//	{
//		if (m_tool != DeformManager.Instance.ToolState)
//			return;
		
//		if (eventData.button != m_deformButton)
//			return;

//		base.OnColliderEventDragEnd (eventData);
//	}
//	#endregion

//}
