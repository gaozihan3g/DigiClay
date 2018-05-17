using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;
using HTC.UnityPlugin.ColliderEvent;
using DigiClay;

public class Smoothable : DeformableBase
{
	public int m_iterations = 1;

	#region IColliderEventHandler implementation
	public override void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;
		
		//basic init
		base.OnColliderEventDragStart (eventData);

        //additional init

        m_weightList = new List<float> ();
	}

	public override void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;

        m_curHandWorldPos = eventData.eventCaster.transform.position;
		var curHandLocalPos = m_curHandWorldPos - transform.position;

        UpdateWeightList(curHandLocalPos);

		m_clayMeshContext.clayMesh.LaplacianSmooth(m_weightList);

		base.OnColliderEventDragUpdate (eventData);
	}

	public override void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
		if (eventData.button != m_deformButton)
			return;

        base.OnColliderEventDragEnd (eventData);
	}
	#endregion
}
