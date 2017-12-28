using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/**
 * Create a module that every tick sends a 'Move' event to
 * the target object
 */
public class MyInputModule : BaseInputModule
{
	public GameObject m_TargetObject;

	public override void Process()
	{
		if (m_TargetObject == null)
			return;

		if (Input.GetMouseButton (0)) {
			PointerEventData ped = new PointerEventData (eventSystem);
			ped.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			ExecuteEvents.Execute (m_TargetObject, ped, ExecuteEvents.pointerDownHandler);
		}
	}
}