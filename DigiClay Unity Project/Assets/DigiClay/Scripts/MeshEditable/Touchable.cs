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
    public string highlightObjName = "Outline";

	#region implementation
	public void OnColliderEventHoverEnter (ColliderHoverEventData eventData)
	{
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);

        eventData.eventCaster.gameObject.transform.Find(highlightObjName).gameObject.SetActive(true);

        HapticManager.Instance.TriggerHaptic(role);

        //DeformManager.Instance.IsTouching(role, true);
	}

	public void OnColliderEventHoverExit (ColliderHoverEventData eventData)
	{
		HandRole role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue);

        eventData.eventCaster.gameObject.transform.Find(highlightObjName).gameObject.SetActive(false);

        //DeformManager.Instance.IsTouching(role, false);
	}

    #endregion

    void Start()
    { }
}
