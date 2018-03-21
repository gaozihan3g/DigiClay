using System.Collections.Generic;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive;
using UnityEngine;

public class TwoHandedDeformable : DeformableBase
{
	[SerializeField]
	bool VisualDebug = true;
	[SerializeField]
	bool HeightChangeEnabled = true;
	[SerializeField]
	bool DeformEnabled = true;
	[SerializeField]
	bool RadialSmoothEnabled = true;

    Vector3[] m_orgHandLocalPos = new Vector3[2];

	float[] m_orgHand2DLocalDist = new float[2];

	[SerializeField]
    Vector3[] m_orgHandWorldPos = new Vector3[2];

    Vector3[] m_prevHandWorldPos = new Vector3[2];
    Vector3[] m_curHandLocalPos = new Vector3[2];

	//TODO fix this
	[SerializeField]
    Vector3[] m_curHandWorldPos = new Vector3[2];

	[SerializeField]
    Vector3 m_avgOrgHandLocalPos;
    Vector3 m_avgHandDeltaPos;

    float m_orgHeight;

	[SerializeField]
	Vector3[] closest2DPointPos = new Vector3[2];
	[SerializeField]
	Vector3[] offsetVector = new Vector3[2];
	float[] offsetDist = new float[2];
	[SerializeField]
	float sign;
	[SerializeField]
	float curHandLocalDist;
	[SerializeField]
	int id;
	[SerializeField]
	Vector3[] curHand2DLocalPos = new Vector3[2];

	#region IColliderEventHandler implementation
	public override void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		base.OnColliderEventDragStart (eventData);

		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		int roleIndex = (int)role;
		//record original positions
		m_orgHandWorldPos[roleIndex] = eventData.eventCaster.transform.position;
		m_orgHandLocalPos[roleIndex] = m_orgHandWorldPos[roleIndex] - transform.position;

		m_orgHand2DLocalDist [roleIndex] = Vector3.ProjectOnPlane (m_orgHandLocalPos [roleIndex], Vector3.up).magnitude;

        DeformManager.Instance.SetHandStatus(role, true);
		//early out if both hands not ready
		if (!DeformManager.Instance.AreBothHandsReady)
			return;

		m_avgOrgHandLocalPos = (m_orgHandLocalPos[0] + m_orgHandLocalPos[1]) / 2f;

		m_weightList = new List<float>();

        m_orgHeight = m_clayMeshContext.clayMesh.Height;

		for (int i = 0; i < m_orgVertices.Length; ++i)
		{
			float dist = 0f;
			dist = Mathf.Abs(m_orgVertices[i].y - m_avgOrgHandLocalPos.y);

			float weight = Falloff( m_innerRadius, m_outerRadius, dist);
			m_weightList.Add(weight);
		}
	}

	public override void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		base.OnColliderEventDragUpdate (eventData);

		if (!DeformManager.Instance.AreBothHandsReady)
			return;
		//record original positions
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		int roleIndex = (int)role;

		m_curHandWorldPos[roleIndex] = eventData.eventCaster.transform.position;
		m_curHandLocalPos[roleIndex] = m_curHandWorldPos[roleIndex] - transform.position;

        //visual debug
        if (VisualDebug)
        {
			Debug.DrawLine(m_curHandWorldPos[roleIndex], m_orgHandWorldPos[roleIndex], Color.green);

            var avgOrgHandWorldPos = (m_orgHandWorldPos[0] + m_orgHandWorldPos[1]) / 2f;
            var avgCurHandWorldPos = (m_curHandWorldPos[0] + m_curHandWorldPos[1]) / 2f;

            Debug.DrawLine(avgOrgHandWorldPos, avgCurHandWorldPos, Color.red);
            Debug.DrawLine(m_curHandWorldPos[0], m_curHandWorldPos[1], Color.blue);
        }

        m_avgHandDeltaPos = (m_curHandLocalPos[0] - m_orgHandLocalPos[0] + m_curHandLocalPos[1] - m_orgHandLocalPos[1]) / 2f;

		if (HeightChangeEnabled) {
	        // ## heightDelta
			float heightDelta = m_avgHandDeltaPos.y * DeformManager.Instance.DeformRatio;
	        // ## update HEIGHT
	        m_clayMeshContext.clayMesh.Height = m_orgHeight + heightDelta;
		}

		if (DeformEnabled) {

			// get 2D hand local pos
			curHand2DLocalPos[roleIndex] = Vector3.ProjectOnPlane (m_curHandLocalPos [roleIndex], Vector3.up);
			// get closest point
			closest2DPointPos[roleIndex] = curHand2DLocalPos[roleIndex].normalized * m_orgHand2DLocalDist[roleIndex];
			// offset vector between
			offsetVector[roleIndex] = curHand2DLocalPos[roleIndex] - closest2DPointPos[roleIndex];

			// ##
			// dist of offset
			offsetDist[roleIndex] = offsetVector[roleIndex].magnitude;

			// get the index of min dist
			id = (offsetDist [0] < offsetDist [1]) ? 0 : 1;

			curHandLocalDist = curHand2DLocalPos[id].magnitude;

			// ## sign
			sign = (curHandLocalDist > m_orgHand2DLocalDist[id]) ? 1f : -1f;

			// ## update MATRIX
			for (int i = 0; i < m_clayMeshContext.clayMesh.RadiusList.Count; ++i)
			{
				//early out
				if (Mathf.Approximately(m_weightList[i], 0f))
					continue;
				//deform
				m_clayMeshContext.clayMesh.Deform(i, m_orgRadiusList[i], sign, offsetDist[id] * DeformManager.Instance.DeformRatio, m_weightList[i]);
			}
		}

		if (RadialSmoothEnabled) {
			// ## radial smooth
	        m_clayMeshContext.clayMesh.RadialSmooth(m_weightList);
		}

        // update mesh
        m_clayMeshContext.clayMesh.UpdateMesh();

		UpdateHapticStrength (role, m_prevHandWorldPos[roleIndex], m_curHandWorldPos[roleIndex]);
		m_prevHandWorldPos[roleIndex] = m_curHandWorldPos[roleIndex];
	}

	public override void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		base.OnColliderEventDragEnd (eventData);

		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);

		DeformManager.Instance.SetHandStatus(role, false);
	}

	#endregion
}
