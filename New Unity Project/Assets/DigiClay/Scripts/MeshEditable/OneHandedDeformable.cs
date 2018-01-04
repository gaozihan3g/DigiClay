using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Pose = HTC.UnityPlugin.PoseTracker.Pose;
using System.Collections;
using HTC.UnityPlugin.Vive;

public class OneHandedDeformable : DeformableBase
{
	Vector3 m_originalLocalPos;
	Vector3 m_previousWorldPosition;
	bool _isSymmetric;
	HandRole m_role;

    public float m_radiusOffsetFactor = 0.5f;

	public override void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton) { return; }

        var casterWorldPosition = eventData.eventCaster.transform.position;

		m_previousWorldPosition = casterWorldPosition;

        m_originalVertices = m_meshFilter.mesh.vertices;

		//register undo
		DeformManager.Instance.RegisterUndo(this, m_originalVertices);

        // get all influenced vertices list, with falloff weights
        // list <float>
        Vector3[] vertices = m_meshFilter.mesh.vertices;

        m_weightList = new List<float>();

//        _originalLocalPos = transform.worldToLocalMatrix.MultiplyPoint(casterWorldPosition);

		// this will remove rotation
		m_originalLocalPos = casterWorldPosition - transform.position;

		_isSymmetric = DeformManager.Instance.Symmetric;

		// calculate weights
        for (int i = 0; i < vertices.Length; ++i)
        {
            float dist = 0f;

			if(_isSymmetric)
			{
				dist = Mathf.Abs(vertices[i].y - m_originalLocalPos.y);
			}
			else
			{
				dist = Vector3.Distance(vertices[i], m_originalLocalPos);
			}

			float weight = Falloff( m_innerRadius, m_outerRadius, dist);
			m_weightList.Add(weight);
        }

        if (OnDeformStart != null)
        {
            OnDeformStart.Invoke(this);
        }

		m_role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
    }

	public override void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
		if (eventData.button != m_deformButton) { return; }

        var currentWorldPosition = eventData.eventCaster.transform.position;

//        var originalWorldPosition = transform.localToWorldMatrix.MultiplyPoint(_originalLocalPos);
		var originalWorldPosition = m_originalLocalPos + transform.position;

        Debug.DrawLine(originalWorldPosition, currentWorldPosition, Color.red);

        Vector3 offsetVector = currentWorldPosition - originalWorldPosition;

//		float offsetDistance = Vector3.Distance(originalWorldPosition, currentWorldPosition);
		// 0m - 0.1m

        //Debug.Log(string.Format("origin {0} | current {1} | offset {2}", originalWorldPosition.ToString("F3"), currentWorldPosition.ToString("F3"), offsetVector.ToString("F3")));

        Vector3[] vertices = m_meshFilter.mesh.vertices;

		var currentLocPos = currentWorldPosition - transform.position;
		float radius = Vector3.ProjectOnPlane (currentLocPos, Vector3.up).magnitude;

        if (_isSymmetric)
        {
            // calculate avgRadius for each row in grid
            m_clayMeshContext.clayMesh.RecalculateAvgRadius();
        }


        for (int i = 0; i < vertices.Length; ++i)
        {
            //early out if weight is 0
            if (m_weightList[i] == 0f)
                continue;

			if(_isSymmetric)
			{
				Vector3 vertNormalDir = new Vector3 (vertices [i].x, 0f, vertices [i].z).normalized;

				float length = Vector3.ProjectOnPlane (offsetVector, Vector3.up).magnitude;

				var currentLocalPos = transform.worldToLocalMatrix.MultiplyPoint (currentWorldPosition);

				float sign = (currentLocalPos.sqrMagnitude > m_originalLocalPos.sqrMagnitude) ? 1f : -1f;


                // smooth radius diffs
                // main affect: 0 - seg * (vSeg + 1)
                // based on weights
                // final += deltaR[i] * weight[i]
                // deltaR = targetR - currentR
                // targetR = sum / seg -> calculate elsewhere
                // currentR = grid[i]
                // cases:
                // 1. outer side
                // 2. inner side
                // 3. outer bottom
                // 4. inner bottom

                float deltaR = 0f;
                Vector3 radiusOffset = Vector3.zero;

                if (i < m_clayMeshContext.clayMesh.RadiusList.Count)
                {
                    // outer side
                    float oldR = m_clayMeshContext.clayMesh.RadiusList[i];
                    float targetR = m_clayMeshContext.clayMesh.GetRowAvgRadiusForVertex(i);
                    deltaR = targetR - oldR;
                    float newR = oldR + deltaR * m_weightList[i] * m_radiusOffsetFactor;

                    // grid[i] = currentR based on vertices[i]
                    m_clayMeshContext.clayMesh.RadiusList[i] = newR;
                    radiusOffset = vertNormalDir * deltaR * m_weightList[i];
                }
                else if (i < m_clayMeshContext.clayMesh.RadiusList.Count * 2)
                {
                    // inner side
                }



                Vector3 finalOffset = vertNormalDir * length * sign * m_strength * m_weightList[i];

                m_originalVertices[i] += radiusOffset * m_radiusOffsetFactor;

                vertices[i] = m_originalVertices[i] + finalOffset;
			}
			else
			{
				Vector3 finalOffset = offsetVector * m_strength * m_weightList[i];
				vertices[i] = m_originalVertices[i] + finalOffset;
			}

            //Debug.Log(string.Format("origin pos: {0}, weight: {1}, offsetVector: {2}, finalOffset: {3}, finalPos: {4}", _originalVertices[i].ToString("F3"), weightList[i], offsetVector.ToString("F3"), finalOffset.ToString("F3"), vertices[i].ToString("F3")));
        }

        m_meshFilter.mesh.vertices = vertices;
        m_meshFilter.mesh.RecalculateNormals();

		TriggerHaptic (m_role, m_previousWorldPosition, currentWorldPosition);

		m_previousWorldPosition = currentWorldPosition;
    }

	public override void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
		if (eventData.button != m_deformButton) { return; }

		//TODO remesh!

        m_meshCollider.sharedMesh = m_meshFilter.mesh;

        if (OnDeformEnd != null)
        {
            OnDeformEnd.Invoke(this);
        }
    }
}