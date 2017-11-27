using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MyGameObject : MonoBehaviour, IMoveHandler, IPointerDownHandler, IPointerClickHandler, IPointerUpHandler {
	#region IPointerUpHandler implementation

	public void OnPointerUp (PointerEventData eventData)
	{
		Debug.Log ("up" + eventData);
	}

	#endregion

	#region IPointerClickHandler implementation

	public void OnPointerClick (PointerEventData eventData)
	{
		Debug.Log ("click" + eventData);
	}

	#endregion

	
	#region IMoveHandler implementation
	public void OnMove (AxisEventData eventData)
	{
		Debug.Log (eventData);
	}
	#endregion

	#region IPointerDownHandler implementation

	public void OnPointerDown (PointerEventData eventData)
	{
		Debug.Log ("down" + eventData);
	}

	#endregion

	#region IDragHandler implementation

	public void OnDrag (PointerEventData eventData)
	{
		Debug.Log (eventData);
	}

	#endregion

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
