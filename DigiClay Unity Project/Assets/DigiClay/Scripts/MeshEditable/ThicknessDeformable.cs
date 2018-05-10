﻿using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Vive;
using UnityEngine;
using DigiClay;

public class ThicknessDeformable : DeformableBase
{
    float m_orgThickness;

	[SerializeField]
	float verticalDelta;
	[SerializeField]
	float thicknessDelta;

    #region IColliderEventHandler implementation
    public override void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
		if (eventData.button != m_deformButton)
			return;

		//basic init
		base.OnColliderEventDragStart (eventData);

		//additional init

		if (m_role != HandRole.RightHand)
			return;

        m_orgThickness = m_clayMeshContext.clayMesh.Thickness;
    }

    public override void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
		if (eventData.button != m_deformButton)
			return;

		if (m_role != HandRole.RightHand)
			return;

		m_curHandWorldPos = eventData.eventCaster.transform.position;

        Debug.DrawLine(m_orgHandWorldPos, m_curHandWorldPos, Color.red);

        Vector3 offsetVector = m_curHandWorldPos - m_orgHandWorldPos;

		verticalDelta = offsetVector.y;

		thicknessDelta = verticalDelta / m_clayMeshContext.clayMesh.Height;

        // get thickness 0 - 1
		m_clayMeshContext.clayMesh.Thickness = m_orgThickness + thicknessDelta;

		base.OnColliderEventDragUpdate (eventData);
    }

    public override void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
		if (eventData.button != m_deformButton)
			return;

        if (m_role != HandRole.RightHand)
            return;

        base.OnColliderEventDragEnd (eventData);
    }
    #endregion
}
