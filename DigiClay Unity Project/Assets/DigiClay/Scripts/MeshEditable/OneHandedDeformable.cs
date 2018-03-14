using System.Collections.Generic;
using DigiClay;
using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive;
using UnityEngine;

public class OneHandedDeformable : DeformableBase
{
    public bool VisualDebug = true;

    Vector3 m_orgHandLocalPos;
    Vector3 m_orgHandWorldPos;
    Vector3 m_prevHandWorldPos;

    bool m_isSymmetric;
    HandRole m_role;

    public float m_radiusDampFactor = 0.5f;

    #region IColliderEventHandler implementation
    public override void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton) { return; }

        m_orgHandWorldPos = eventData.eventCaster.transform.position;
        // this will remove rotation
        m_orgHandLocalPos = m_orgHandWorldPos - transform.position;

        m_prevHandWorldPos = m_orgHandWorldPos;

        // get all influenced vertices list, with falloff weights
        // list <float>
        m_orgVertices = m_meshFilter.mesh.vertices;
        m_weightList = new List<float>();
        m_isSymmetric = DeformManager.Instance.Symmetric;

        // calculate weights
        for (int i = 0; i < m_orgVertices.Length; ++i)
        {
            float dist = 0f;

            if (m_isSymmetric)
            {
                dist = Mathf.Abs(m_orgVertices[i].y - m_orgHandLocalPos.y);
            }
            else
            {
                dist = Vector3.Distance(m_orgVertices[i], m_orgHandLocalPos);
            }

            float weight = Falloff(m_innerRadius, m_outerRadius, dist);
            m_weightList.Add(weight);
        }

        m_role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue);

        //register undo
        DeformManager.Instance.RegisterUndo(this, m_orgVertices);

        if (OnDeformStart != null)
        {
            OnDeformStart.Invoke(this);
        }
    }

    public override void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton) { return; }

        var curHandWorldPos = eventData.eventCaster.transform.position;

        if (VisualDebug)
            Debug.DrawLine(m_orgHandWorldPos, curHandWorldPos, Color.red);

        Vector3 offsetVector = curHandWorldPos - m_orgHandWorldPos;

        var curHandLocalPos = curHandWorldPos - transform.position;

        //float radius = Vector3.ProjectOnPlane(curHandLocalPos, Vector3.up).magnitude;

        Vector3 finalOffset = Vector3.zero;

        if (m_isSymmetric)
        {
            // calculate avgRadius for each row in grid
            m_clayMeshContext.clayMesh.RecalculateAvgRadius();
        }

        Vector3[] vertices = m_meshFilter.mesh.vertices;
        ///
        for (int i = 0; i < vertices.Length; ++i)
        {
            //early out if weight is 0
            if (m_weightList[i] == 0f)
                continue;

            if (m_isSymmetric)
            {
                Vector3 vertNormalDir = new Vector3(vertices[i].x, 0f, vertices[i].z).normalized;

                float length = Vector3.ProjectOnPlane(offsetVector, Vector3.up).magnitude;

                float sign = (curHandLocalPos.sqrMagnitude > m_orgHandLocalPos.sqrMagnitude) ? 1f : -1f;

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

                if (m_clayMeshContext.clayMesh.GetVertexTypeFromIndex(i) == ClayMesh.VertexType.OuterSide)
                {
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
                else if (m_clayMeshContext.clayMesh.GetVertexTypeFromIndex(i) == ClayMesh.VertexType.InnerSide)
                {
                    // inner side
                    m_orgVertices[i] = m_orgVertices[i - m_clayMeshContext.clayMesh.RadiusMatrix.Count] * m_clayMeshContext.clayMesh.ThicknessRatio;
                }
                else if (m_clayMeshContext.clayMesh.GetVertexTypeFromIndex(i) == ClayMesh.VertexType.OuterBottomEdge)
                {
                    // outer bottom
                    m_orgVertices[i] = m_orgVertices[i - m_clayMeshContext.clayMesh.RadiusMatrix.Count * 2];
                }
                else if (m_clayMeshContext.clayMesh.GetVertexTypeFromIndex(i) == ClayMesh.VertexType.InnerBottomEdge)
                {
                    // inner bottom
                    m_orgVertices[i] = m_orgVertices[i - (m_clayMeshContext.clayMesh.RadiusMatrix.Count * 2 + 1 + m_clayMeshContext.clayMesh.Column)];
                }

                //2.
                finalOffset = vertNormalDir * length * sign * m_strength * m_weightList[i];
                 
            }
            else // not symmetric
            {
                finalOffset = offsetVector * m_strength * m_weightList[i];
            }

            vertices[i] = m_orgVertices[i] + finalOffset;

            //Debug.Log(string.Format("origin pos: {0}, weight: {1}, offsetVector: {2}, finalOffset: {3}, finalPos: {4}", _originalVertices[i].ToString("F3"), weightList[i], offsetVector.ToString("F3"), finalOffset.ToString("F3"), vertices[i].ToString("F3")));
        }
        ///
        m_meshFilter.mesh.vertices = vertices;
        m_clayMeshContext.clayMesh.RecalculateNormals();

        TriggerHaptic(m_role, m_prevHandWorldPos, curHandWorldPos);
        m_prevHandWorldPos = curHandWorldPos;
    }

    public override void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_deformButton)
            return;

        m_meshCollider.sharedMesh = m_meshFilter.mesh;

        if (OnDeformEnd != null)
        {
            OnDeformEnd.Invoke(this);
        }
    }
    #endregion
}