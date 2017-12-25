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
    Vector3 _originalLocalPos;
	public float maxDist = 0.1f;
	bool _isSymmetric;
	int _role;

	public override void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton) { return; }

        var casterWorldPosition = eventData.eventCaster.transform.position;

        m_originalVertices = m_meshFilter.mesh.vertices;

		//register undo
		DeformManager.Instance.RegisterUndo(this, m_originalVertices);

        // get all influenced vertices list, with falloff weights
        // list <float>
        Vector3[] vertices = m_meshFilter.mesh.vertices;

        m_weightList = new List<float>();

        _originalLocalPos = transform.worldToLocalMatrix.MultiplyPoint(casterWorldPosition);

		_isSymmetric = DeformManager.Instance.Symmetric;

        for (int i = 0; i < vertices.Length; ++i)
        {
            float dist = 0f;

			if(_isSymmetric)
			{
				dist = Mathf.Abs(vertices[i].y - _originalLocalPos.y);
			}
			else
			{
				dist = Vector3.Distance(vertices[i], _originalLocalPos);
			}

			float weight = Falloff( m_innerRadius, m_outerRadius, dist);
			m_weightList.Add(weight);

            //TODO symmetric deform
        }

        if (OnDeformStart != null)
        {
            OnDeformStart.Invoke(this);
        }

		_role = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue;

		HapticManager.Instance.StartHaptic ((HandRole)_role);
//		HapticManager.Instance.StartRightHaptic ();
    }

	public override void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
		if (eventData.button != m_deformButton) { return; }

        var currentWorldPosition = eventData.eventCaster.transform.position;
        var originalWorldPosition = transform.localToWorldMatrix.MultiplyPoint(_originalLocalPos);

		Transform currentTransform = eventData.eventCaster.transform;

        Debug.DrawLine(originalWorldPosition, currentWorldPosition, Color.red);

        Vector3 offsetVector = currentWorldPosition - originalWorldPosition;

		float offsetDistance = Vector3.Distance(originalWorldPosition, currentWorldPosition);
		// 0m - 0.1m

		//Haptic
		HapticManager.Instance.Strength = Mathf.InverseLerp(0, maxDist, offsetDistance);


        //Debug.Log(string.Format("origin {0} | current {1} | offset {2}", originalWorldPosition.ToString("F3"), currentWorldPosition.ToString("F3"), offsetVector.ToString("F3")));

        Vector3[] vertices = m_meshFilter.mesh.vertices;


        for (int i = 0; i < vertices.Length; ++i)
        {
            //early out if weight is 0
            if (m_weightList[i] == 0f)
                continue;


			Vector3 finalOffset;


			if(_isSymmetric)
			{


				Vector3 vertNormalDir = new Vector3 (vertices [i].x, 0f, vertices [i].z).normalized;

				float length = Vector3.ProjectOnPlane (offsetVector, Vector3.up).magnitude;

				var currentLocalPos = transform.worldToLocalMatrix.MultiplyPoint (currentWorldPosition);

				float sign = (currentLocalPos.sqrMagnitude > _originalLocalPos.sqrMagnitude) ? 1f : -1f;

				finalOffset = vertNormalDir * length * sign * m_strength * m_weightList[i];
			}
			else
			{
				finalOffset = offsetVector * m_strength * m_weightList[i];

				// TODO rotation
//				Matrix4x4 mx = Matrix4x4.identity;
//
//				finalOffset = offsetVector * _strength * weightList[i];
//
//				Quaternion q = Quaternion.FromToRotation (_originalTransform.forward, currentTransform.forward);
//
//				mx.SetTRS (finalOffset,
//					Quaternion.Lerp (Quaternion.identity, q, weightList [i]),
//					Vector3.one);
//
//				vertices [i] = mx.MultiplyPoint (_originalVertices [i]);
			}

			vertices[i] = m_originalVertices[i] + finalOffset;

            //Debug.Log(string.Format("origin pos: {0}, weight: {1}, offsetVector: {2}, finalOffset: {3}, finalPos: {4}", _originalVertices[i].ToString("F3"), weightList[i], offsetVector.ToString("F3"), finalOffset.ToString("F3"), vertices[i].ToString("F3")));
        }

        m_meshFilter.mesh.vertices = vertices;
        m_meshFilter.mesh.RecalculateNormals();
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
			
		HapticManager.Instance.EndHaptic ((HandRole)_role);
    }
}