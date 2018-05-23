﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DigiClay
{
	public class Cursor : MonoBehaviour {

		[Range(DigiClayConstant.CURSOR_MIN_RADIUS, DigiClayConstant.CURSOR_MAX_RADIUS)]
		[SerializeField]
		private float outerRadius;

		[Range(DigiClayConstant.CURSOR_MIN_RADIUS, DigiClayConstant.CURSOR_MAX_RADIUS)]
		[SerializeField]
		private float innerRadius;

		public Transform outerSphere;
		public Transform innerSphere;

        void OnValidate()
        {
            if (outerSphere != null)
                outerSphere.localScale = Vector3.one * outerRadius;

            if (innerSphere != null)
                innerSphere.localScale = Vector3.one * innerRadius;
        }

		void OnEnable()
		{
			Debug.Log("Cursor OnEnable");

			Init ();

			DeformManager.Instance.ValueChanged.AddListener (DeformParameterChangedHandler);

//			if (innerSlider != null)
//			{
//				innerSlider.value = innerRadius;
//				innerSlider.onValueChanged.AddListener (UpdateInnerRadius);
//			}
//
//			if (outerSlider != null)
//			{
//				outerSlider.value = Mathf.InverseLerp (DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS, innerRadius);
//				outerSlider.onValueChanged.AddListener (UpdateOuterRadius);
//			}
		}

		void OnDisable()
		{
			DeformManager.Instance.ValueChanged.RemoveListener (DeformParameterChangedHandler);
		}

		void Start()
		{
			Init ();
		}

		void Init()
		{
			UpdateInnerRadius (DeformManager.Instance.InnerRadius);
			UpdateOuterRadius (DeformManager.Instance.OuterRadius);
		}

		public void UpdateOuterRadius(float v)
		{
			outerRadius = v;
			outerSphere.localScale = Vector3.one * outerRadius * 2f;
		}

		public void UpdateInnerRadius(float v)
		{
			innerRadius = v;
			innerSphere.localScale = Vector3.one * Mathf.Max(innerRadius * 2f, DigiClayConstant.CURSOR_MIN_RADIUS);
		}

		public void DeformParameterChangedHandler(DeformManager.DeformArgs args)
		{
//			Debug.Log ("ValueChangedHandler from Cursor");
			UpdateInnerRadius (args.innerRadius);
			UpdateOuterRadius (args.outerRadius);
		}
	}
}