using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.ColliderEvent;
using System;
using UnityEngine.Events;
using DigiClay;
using HTC.UnityPlugin.Vive;

public class OneAndTwoHandedDeformable : DeformableBase
{
    //state
    [SerializeField]
    DeformTools.ToolState m_state = DeformTools.ToolState.Idle;
    [SerializeField]
	HandRole m_workingHand = HandRole.Invalid;
    bool[] m_clicked = new bool[2];
    int[] m_frameCount = new int[2];

    // two handed
    Vector3[] m_orgHandLocalPosAry = new Vector3[2];
    float[] m_orgHand2DLocalDistAry = new float[2];
    Vector3[] m_orgHandWorldPosAry = new Vector3[2];
    Vector3[] m_prevHandWorldPosAry = new Vector3[2];

    Vector3[] m_curHandLocalPosAry = new Vector3[2];
    Vector3[] m_curHandWorldPosAry = new Vector3[2];
    Vector3[] m_curHand2DLocalPosAry = new Vector3[2];

    Vector3[] closest2DPointPosAry = new Vector3[2];
    Vector3[] offsetVectorAry = new Vector3[2];
    float[] offsetDistAry = new float[2];

    float m_orgHeight;


    #region IColliderEventDragStartHandler implementation

    public override void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
        if (eventData.button != m_deformButton)
            return;

        var roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue;
		UpdateStateStart (roleIndex);

		if (m_state == DeformTools.ToolState.OneHand) {
			if (!IsWorkingHand (roleIndex))
				return;
			OneHandStart (eventData);
		} else if (m_state == DeformTools.ToolState.TwoHand) {
			TwoHandStart (eventData);
		}
	}

	public override void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
	{
        if (eventData.button != m_deformButton)
            return;

        var roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue;

		if (m_state == DeformTools.ToolState.OneHand) {
			if (!IsWorkingHand (roleIndex))
				return;
			OneHandUpdate (eventData);
		} else if (m_state == DeformTools.ToolState.TwoHand) {
			TwoHandUpdate (eventData);
		}

        m_clayMeshContext.clayMesh.UpdateMesh();
    }

	public override void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
        if (eventData.button != m_deformButton)
            return;

        var roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue;

        if (m_state == DeformTools.ToolState.OneHand)
        {
            if (!IsWorkingHand(roleIndex))
                return;
            OneHandEnd(eventData);
        }
        else if (m_state == DeformTools.ToolState.TwoHand)
        {
            TwoHandEnd(eventData);
        }

        UpdateStateEnd(roleIndex);
    }

	#endregion


	#region StateUpdate
	void UpdateStateStart(int roleIndex)
	{
		m_clicked [roleIndex] = true;
		m_frameCount [roleIndex] = Time.frameCount;
		// if 2 clicked => two
		if (m_clicked [0] && m_clicked [1] && IsSimultaneous()) {
			m_state = DeformTools.ToolState.TwoHand;
			m_workingHand = HandRole.Invalid;
			return;
		}
		
		// if 1 clicked => one
		if (m_clicked [0] || m_clicked [1]) {
			m_state = DeformTools.ToolState.OneHand;

			if (m_workingHand == HandRole.Invalid)
				m_workingHand = (HandRole)roleIndex;
		}
	}

	void UpdateStateEnd(int roleIndex)
	{
		m_clicked [roleIndex] = false;

		if (m_state == DeformTools.ToolState.TwoHand)
			m_state = DeformTools.ToolState.Idle;

		if (IsWorkingHand(roleIndex)) {
			m_state = DeformTools.ToolState.Idle;
			m_workingHand = HandRole.Invalid;
		}
	}

	bool IsSimultaneous()
	{
		return Mathf.Abs (m_frameCount [0] - m_frameCount [1]) < 10;
	}

	bool IsWorkingHand(int roleIndex)
	{
		return (HandRole)roleIndex == m_workingHand;
	}

	#endregion

	void OneHandStart(ColliderButtonEventData eventData)
	{
		//Debug.Log ("OneHandStart" + Time.frameCount);

        int roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue;

        RegisterOrgHandPos(roleIndex, eventData);
        RegisterOrgClayMesh();

        //additional init
        m_orgHeight = m_clayMeshContext.clayMesh.Height;
        UpdateWeightList(m_orgHandLocalPosAry[roleIndex]);
        DeformManager.Instance.IsDeforming((HandRole)roleIndex, true);
    }

    void TwoHandStart(ColliderButtonEventData eventData)
	{
		//Debug.Log ("TwoHandStart" + Time.frameCount);

        int roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue;

        RegisterOrgHandPos(roleIndex, eventData);

        //additional init
        var avgOrgHandLocalPos = (m_orgHandLocalPosAry[0] + m_orgHandLocalPosAry[1]) / 2f;
        UpdateWeightList(avgOrgHandLocalPos);
        DeformManager.Instance.IsDeforming((HandRole)roleIndex, true);
    }

    void OneHandUpdate(ColliderButtonEventData eventData)
	{
		//Debug.Log ("OneHandUpdate" + Time.frameCount);

        int roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue;

        UpdateCurHandPos(roleIndex, eventData);

        // ### Deform
        var curHand2DLocalDist = m_curHand2DLocalPosAry[roleIndex].magnitude;
        // ## sign
        var sign = (curHand2DLocalDist > m_orgHand2DLocalDistAry[roleIndex]) ? 1f : -1f;

        DeformManager.Instance.Pressure = ViveInput.GetTriggerValue((HandRole)roleIndex);

        m_clayMeshContext.clayMesh.Deform(sign, offsetDistAry[roleIndex], m_orgRadiusList, m_weightList);
    }

    void TwoHandUpdate(ColliderButtonEventData eventData)
	{
		//Debug.Log ("TwoHandUpdate" + Time.frameCount);

        int roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue;

        UpdateCurHandPos(roleIndex, eventData);

        //below ONLY EXECUTE ONCE
        if (roleIndex == 0)
            return;

        // ### RadialSmooth
        m_clayMeshContext.clayMesh.RadialSmooth(m_weightList);


        // ### deform
        if (false)
        {
            // get the index of min dist
            int minDistIndex = (offsetDistAry[0] < offsetDistAry[1]) ? 0 : 1;
            // the distance of the closer hand
            float curHand2DLocalDist = m_curHand2DLocalPosAry[minDistIndex].magnitude;
            // ## sign, compare the dist to get the sign
            float sign = (curHand2DLocalDist > m_orgHand2DLocalDistAry[minDistIndex]) ? 1f : -1f;

            m_clayMeshContext.clayMesh.Deform(sign, offsetDistAry[minDistIndex], m_orgRadiusList, m_weightList);
        }

        // ### HeightChange

        var avgHandDeltaPos = (m_curHandLocalPosAry[0] - m_orgHandLocalPosAry[0] + m_curHandLocalPosAry[1] - m_orgHandLocalPosAry[1]) / 2f;
        // ## heightDelta
        float heightDelta = avgHandDeltaPos.y * DeformManager.Instance.DeformStrength;
        // ## update HEIGHT
        m_clayMeshContext.clayMesh.Height = m_orgHeight + heightDelta;

    }

	void OneHandEnd(ColliderButtonEventData eventData)
	{
		Debug.Log ("OneHandEnd" + Time.frameCount);
        int roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue;
        m_meshCollider.sharedMesh = m_meshFilter.sharedMesh;
        DeformManager.Instance.IsDeforming((HandRole)roleIndex, false);
    }

	void TwoHandEnd(ColliderButtonEventData eventData)
	{
		Debug.Log ("TwoHandEnd" + Time.frameCount);
        //int roleIndex = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue;
        m_meshCollider.sharedMesh = m_meshFilter.sharedMesh;
        DeformManager.Instance.IsDeforming((HandRole)0, false);
        DeformManager.Instance.IsDeforming((HandRole)1, false);
    }

    void RegisterOrgHandPos(int roleIndex, ColliderButtonEventData eventData)
    {
        m_orgHandWorldPosAry[roleIndex] = eventData.eventCaster.transform.position;
        m_orgHandLocalPosAry[roleIndex] = m_orgHandWorldPosAry[roleIndex] - transform.position;
        m_prevHandWorldPosAry[roleIndex] = m_orgHandWorldPosAry[roleIndex];
        m_orgHand2DLocalDistAry[roleIndex] = Vector3.ProjectOnPlane(m_orgHandLocalPosAry[roleIndex], Vector3.up).magnitude;
    }

    void UpdateCurHandPos(int roleIndex, ColliderButtonEventData eventData)
    {
        m_curHandWorldPosAry[roleIndex] = eventData.eventCaster.transform.position;
        m_curHandLocalPosAry[roleIndex] = m_curHandWorldPosAry[roleIndex] - transform.position;
        // get 2D hand local pos
        m_curHand2DLocalPosAry[roleIndex] = Vector3.ProjectOnPlane(m_curHandLocalPosAry[roleIndex], Vector3.up);
        // get closest point
        closest2DPointPosAry[roleIndex] = m_curHand2DLocalPosAry[roleIndex].normalized * m_orgHand2DLocalDistAry[roleIndex];
        // offset vector between
        offsetVectorAry[roleIndex] = m_curHand2DLocalPosAry[roleIndex] - closest2DPointPosAry[roleIndex];

        // ##
        // dist of offset
        offsetDistAry[roleIndex] = offsetVectorAry[roleIndex].magnitude;

        m_prevHandWorldPosAry[roleIndex] = m_curHandWorldPosAry[roleIndex];
    }
}
