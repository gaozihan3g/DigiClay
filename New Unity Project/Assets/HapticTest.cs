using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

public class HapticTest : MonoBehaviour {

	public ushort duration = 500;

	public float interval = 1;

	public int loops = 10;

	// Use this for initialization
	void Start () {
		OnScreenUIManager.Instance.AddCommand ("Right Haptic Test", () => {
			ViveInput.TriggerHapticPulse(HandRole.RightHand, duration);
		});

		OnScreenUIManager.Instance.AddCommand ("Left Haptic Test", () => {
			StartCoroutine("HapticSequence");
		});

		OnScreenUIManager.Instance.AddCommand ("Stop", () => {
			StopCoroutine("HapticSequence");
		});

	}

	IEnumerator HapticSequence()
	{
		while (true)
		{
			
			ViveInput.TriggerHapticPulse(HandRole.LeftHand, duration);

			yield return new WaitForSeconds (interval);
		}
	}

	// Update is called once per frame
	void Update () {
		
	}
}
