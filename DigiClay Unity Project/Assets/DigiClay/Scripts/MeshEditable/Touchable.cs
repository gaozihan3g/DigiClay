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
	public bool enter = true;
	public bool exit = true;

	#region implementation
	public void OnColliderEventHoverEnter (ColliderHoverEventData eventData)
	{
		if (!enter)
			return;
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		HapticManager.Instance.StartHaptic(role);
		Debug.Log ("OnColliderEventHoverEnter " + role.ToString());
	}

	public void OnColliderEventHoverExit (ColliderHoverEventData eventData)
	{
		if (!exit)
			return;
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);
		HapticManager.Instance.EndHaptic(role);
		Debug.Log ("OnColliderEventHoverExit " + role.ToString());
	}

	#endregion
}
