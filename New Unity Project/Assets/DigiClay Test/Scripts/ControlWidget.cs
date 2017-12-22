using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class ControlWidget : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
	[System.Serializable]
	public class ControlEvent : UnityEvent<float>
	{
	}

    public LineRenderer _controlLine;
    public GameObject _controlCube;
    //public GameObject _target;

    public UnityEvent controlStartEvent;
    public ControlEvent controlChangedEvent;

    public float _output = 1f;

    Plane _plane;
    Vector3 _originalPosition;
    //Vector3 _originalScale;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Drag Begin!");

        _originalPosition = _controlCube.transform.position;
        //_originalScale = _target.transform.localScale;

        controlStartEvent.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
		Ray ray = Camera.allCameras[1].ScreenPointToRay(eventData.position);

		float rayDistance;

        if (_plane.Raycast(ray, out rayDistance))
        {
            var newPos = ray.GetPoint(rayDistance);
            _controlCube.transform.position = new Vector3(_controlCube.transform.position.x, newPos.y, _controlCube.transform.position.z);

            _output = _controlCube.transform.localPosition.y / transform.worldToLocalMatrix.MultiplyPoint3x4(_originalPosition).y;

            controlChangedEvent.Invoke(_output);

            //_target.transform.localScale = new Vector3( _originalScale.x, _originalScale.y * _output, _originalScale.z);
        }
			
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Drag End!");

        _controlCube.transform.position = _originalPosition;

    }

    // Use this for initialization
    void Start () {
        _plane = new Plane(_controlCube.transform.forward.normalized, _controlCube.transform.position);

	}
	
	// Update is called once per frame
	void Update () {
        _controlLine.SetPosition(0, transform.position);
        _controlLine.SetPosition(1, _controlCube.transform.position);
	}
}
