using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DigiClay
{
	public class Cursor : MonoBehaviour {

		[Range(DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS)]
		public float outerRadius;

		[Range(0f, 1f)]
		public float innerRatio;


		public Transform outerSphere;
		public Transform innerSphere;

		public Slider outerSlider;
		public Slider innerSlider;


		void Start()
		{
			if (innerSlider != null)
			{
				innerSlider.value = innerRatio;
				innerSlider.onValueChanged.AddListener (UpdateInnerRadius);
			}

			if (outerSlider != null)
			{
				outerSlider.value = Mathf.InverseLerp (DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS, innerRatio);
				outerSlider.onValueChanged.AddListener (UpdateOuterRadius);
			}
		}

		public void UpdateOuterRadius(float v)
		{
			outerRadius = Mathf.Lerp (DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS, v);
			outerSphere.localScale = Vector3.one * outerRadius;

			innerSphere.localScale = Vector3.one * outerRadius * innerRatio;
		}

		public void UpdateInnerRadius(float v)
		{
			innerRatio = v;
			innerSphere.localScale = Vector3.one * outerRadius * innerRatio;
		}
	}
}