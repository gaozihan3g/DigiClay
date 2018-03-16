﻿using System.Collections.Generic;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive;
using UnityEngine;

public class TwoHandedDeformable : DeformableBase
{
	public bool VisualDebug = true;

    Vector3[] m_orgHandLocalPos = new Vector3[2];
    Vector3[] m_orgHandWorldPos = new Vector3[2];
    Vector3[] m_prevHandWorldPos = new Vector3[2];

    Vector3[] m_curHandLocalPos = new Vector3[2];
    Vector3[] m_curHandWorldPos = new Vector3[2];

	float[] m_handPosDeltaLength = new float[2];

    Vector3 m_avgOrgHandLocalPos;
    Vector3 m_avgHandDeltaPos;
    float m_orgHandDist;
    float m_curHandDist;

    float m_orgHeight;
    List<float> m_orgMatrix;

	#region IColliderEventHandler implementation
	public override void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);

		//record original positions
        m_orgHandWorldPos[(int)role] = eventData.eventCaster.transform.position;
        m_orgHandLocalPos[(int)role] = m_orgHandWorldPos[(int)role] - transform.position;

        DeformManager.Instance.SetHandStatus(role, true);
		//early out if both hands not ready
		if (!DeformManager.Instance.IsBothHandReady)
			return;

		m_avgOrgHandLocalPos = (m_orgHandLocalPos[0] + m_orgHandLocalPos[1]) / 2f;
        m_orgHandDist = Vector3.Distance(m_orgHandLocalPos[0], m_orgHandLocalPos[1]);

		m_orgVertices = m_meshFilter.mesh.vertices;
		m_weightList = new List<float>();

        m_orgHeight = m_clayMeshContext.clayMesh.Height;
        m_orgMatrix = m_clayMeshContext.clayMesh.RadiusMatrix;

		for (int i = 0; i < m_orgVertices.Length; ++i)
		{
			float dist = 0f;
			dist = Mathf.Abs(m_orgVertices[i].y - m_avgOrgHandLocalPos.y);

			float weight = Falloff( m_innerRadius, m_outerRadius, dist);
			m_weightList.Add(weight);
		}

        //register undo
        DeformManager.Instance.RegisterUndo(this, m_orgVertices);

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

        // TODO
        // how many times does this get called per frame ?
        Debug.Log(string.Format("role {0} frameCount {1}", role, Time.frameCount));

        m_curHandWorldPos[(int)role] = eventData.eventCaster.transform.position;
        m_curHandLocalPos[(int)role] = m_curHandWorldPos[(int)role] - transform.position;

        //visual debug
        if (VisualDebug)
        {
            for (int i = 0; i < 2; ++i)
            {
                Debug.DrawLine(m_curHandWorldPos[i], m_orgHandWorldPos[i], Color.green);
            }

            var avgOrgHandWorldPos = (m_orgHandWorldPos[0] + m_orgHandWorldPos[1]) / 2f;
            var avgCurHandWorldPos = (m_curHandWorldPos[0] + m_curHandWorldPos[1]) / 2f;

            Debug.DrawLine(avgOrgHandWorldPos, avgCurHandWorldPos, Color.red);

            Debug.DrawLine(m_curHandWorldPos[0], m_curHandWorldPos[1], Color.blue);
        }

        m_avgHandDeltaPos = (m_curHandLocalPos[0] - m_orgHandLocalPos[0] + m_curHandLocalPos[1] - m_orgHandLocalPos[1]) / 2f;
        m_curHandDist = Vector3.Distance (m_curHandLocalPos[0], m_curHandLocalPos[1]);

		for (int i = 0; i < 2; ++i)
			m_handPosDeltaLength [i] = Vector3.ProjectOnPlane ((m_curHandWorldPos [i] - m_orgHandWorldPos [i]), Vector3.up).magnitude;

        // ## heightDelta
        float heightDelta = m_avgHandDeltaPos.y;
        // ## update HEIGHT
        m_clayMeshContext.clayMesh.Height = m_orgHeight + heightDelta;

        // ## sign
        float distDelta = m_curHandDist - m_orgHandDist;
		float sign = (distDelta > 0) ? 1f : -1f;
        // ## longerLengthIndex
        int longerLengthIndex = (m_handPosDeltaLength[0] > m_handPosDeltaLength[1]) ? 0 : 1;
        // ## update MATRIX
        for (int i = 0; i < m_clayMeshContext.clayMesh.RadiusMatrix.Count; ++i)
        {
            //early out
            if (Mathf.Approximately(m_weightList[i], 0f))
                continue;
            //deform
            m_clayMeshContext.clayMesh.Deform(i, m_orgMatrix[i], sign, m_handPosDeltaLength[longerLengthIndex], m_weightList[i]);
        }

        // ## radial smooth
        m_clayMeshContext.clayMesh.RadialSmooth(m_weightList);

        // update mesh
        m_clayMeshContext.clayMesh.UpdateMesh();

        TriggerHaptic (role, m_prevHandWorldPos[(int)role], m_curHandWorldPos[(int)role]);
        m_prevHandWorldPos[(int)role] = m_curHandWorldPos[(int)role];
	}

	public override void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;

		m_meshCollider.sharedMesh = m_meshFilter.mesh;


		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);

        //reset position
        m_orgHandLocalPos[(int)role] = Vector3.zero;
        m_curHandLocalPos[(int)role] = Vector3.zero;

		DeformManager.Instance.SetHandStatus (role, false);

		if (OnDeformEnd != null)
		{
			OnDeformEnd.Invoke(this);
		}
	}

	#endregion
}