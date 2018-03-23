using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.ColliderEvent;
using System;
using UnityEngine.Events;
using DigiClay;
using HTC.UnityPlugin.Vive;

public class Touchable : MonoBehaviour
, IColliderEventHoverEnterHandler
, IColliderEventHoverExitHandler
{
	#region implementation
	public void OnColliderEventHoverEnter (ColliderHoverEventData eventData)
	{
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		DeformManager.Instance.IsTouching(role, true);
	}

	public void OnColliderEventHoverExit (ColliderHoverEventData eventData)
	{
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		DeformManager.Instance.IsTouching(role, false);
	}

	#endregion
}
