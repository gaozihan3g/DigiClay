using System.Collections.Generic;
using DigiClay;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive;
using UnityEngine;

public class TwoHandedDeformable : DeformableBase
{
	public bool VisualDebug = true;

	public float m_heightBase = 1f;
	public float m_radialBase = 1f;
    public float m_heightDeltaRatio = 1f;
    public float m_radialDeltaRatio = 1f;
    public float m_radiusDampFactor = 0.5f;

    Vector3[] m_orgHandLocalPos = new Vector3[2];
    Vector3[] m_orgHandWorldPos = new Vector3[2];
    Vector3[] m_prevHandWorldPos = new Vector3[2];

    Vector3[] m_curHandLocalPos = new Vector3[2];
    Vector3[] m_curHandWorldPos = new Vector3[2];

    Vector3 m_avgOrgHandLocalPos;
    Vector3 m_avgDir;
    float m_orgDist;
    float m_curDist;

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
        m_orgDist = Vector3.Distance(m_orgHandLocalPos[0], m_orgHandLocalPos[1]);

		m_orgVertices = m_meshFilter.mesh.vertices;
		m_weightList = new List<float>();

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

		m_avgDir = (m_curHandLocalPos[0] - m_orgHandLocalPos[0] + m_curHandLocalPos[1] - m_orgHandLocalPos[1]) / 2f;
		m_curDist = Vector3.Distance (m_curHandLocalPos[0], m_curHandLocalPos[1]);

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

		float verticalDelta = m_avgDir.y;
		m_heightDeltaRatio = verticalDelta / m_heightBase;

        m_clayMeshContext.clayMesh.Height *= 1f + m_heightDeltaRatio;

		/// method #1 - based on hand distance
		float distDelta = m_curDist - m_orgDist;
		m_radialDeltaRatio = distDelta / m_radialBase;

        Vector3[] vertices = m_meshFilter.mesh.vertices;
        ///
		for (int i = 0; i < vertices.Length; ++i)
		{
            if (m_clayMeshContext.clayMesh.GetVertexTypeFromIndex(i) == ClayMesh.VertexType.OuterSide)
            {
                //height

                //radial smooth

                //deform


                Vector3 vertNormalDir = new Vector3(vertices[i].x, 0f, vertices[i].z).normalized;
                float deltaR = 0f;
                Vector3 radiusOffset = Vector3.zero;

                //handle deform
                //early out if weight is 0
                if (m_weightList[i] == 0f)
                    continue;

                //TODO
                //handle height change
                vertices[i].y = m_orgVertices[i].y + m_orgVertices[i].y * m_heightDeltaRatio;

                // outer side
                // 1. make the radius closer to avg radius
                float oldR = m_clayMeshContext.clayMesh.RadiusMatrix[i];
                float targetR = m_clayMeshContext.clayMesh.GetRowAvgRadiusForVertex(i);
                deltaR = targetR - oldR;
                float newR = oldR + deltaR * m_weightList[i] * m_radiusDampFactor;

                // update Radius List, only for outer side
                m_clayMeshContext.clayMesh.RadiusMatrix[i] = newR;

                radiusOffset = vertNormalDir * deltaR * m_weightList[i];
                m_orgVertices[i] += radiusOffset * m_radiusDampFactor;
            }


			//vertices[i].x = m_orgVertices[i].x + m_orgVertices[i].x * m_radialDeltaRatio * m_strength * m_weightList [i];
			//vertices[i].z = m_orgVertices[i].z + m_orgVertices[i].z * m_radialDeltaRatio * m_strength * m_weightList [i];
		}
        ///
		m_meshFilter.mesh.vertices = vertices;
        m_clayMeshContext.clayMesh.RecalculateNormals();

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
