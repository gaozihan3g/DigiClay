using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

public class HapticManager : MonoBehaviour {

	[Range(1, 3600)]
	public ushort _duration = 500;
	public ushort maxDuration = 3600;

	public float _strength;

	public float Strength {
		get {
			return _strength;
		}
		set {
			_strength = value;
			_duration = (ushort)(maxDuration * value);
		}
	}

	[Range(1,1000)]
	public ushort _interval = 1;

	public int _loops = 10;

	public static HapticManager Instance;

	IEnumerator _rightCoroutine;
	IEnumerator _leftCoroutine;

	Dictionary<HandRole, IEnumerator> coroutineDic = new Dictionary<HandRole, IEnumerator>();

	void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Destroy(this);
		}
	}

	// Use this for initialization
	void Start ()
	{
		Init ();
	}

	void Init()
	{
//		OnScreenUIManager.Instance.AddCommand ("Right", () => {
//			StartRightHaptic();
//		});
//
//		OnScreenUIManager.Instance.AddCommand ("Left", () => {
//			StartLeftHaptic();
//		});
//
//		OnScreenUIManager.Instance.AddCommand ("Stop Left", () => {
//			EndLeftHaptic();
//		});
//
//		OnScreenUIManager.Instance.AddCommand ("Stop Right", () => {
//			EndRightHaptic();
//		});
	}

	public void StartHaptic(HandRole role)
	{
		IEnumerator c = HapticSequence (role);
		coroutineDic.Add (role, c);
		StartCoroutine (c);
	}

	public void EndHaptic(HandRole role)
	{
		if (!coroutineDic.ContainsKey (role))
			return;
		
		IEnumerator c = coroutineDic [role];
		StopCoroutine (c);
		coroutineDic.Remove (role);
	}

//	public void StartRightHaptic(ushort duration = 1, ushort interval = 1)
//	{
//		_rightCoroutine = HapticSequence (HandRole.RightHand, duration, interval);
//		StartCoroutine(_rightCoroutine);
//	}
//
//	public void StartLeftHaptic(ushort duration = 1, ushort interval = 1)
//	{
//		_leftCoroutine = HapticSequence (HandRole.LeftHand, duration, interval);
//		StartCoroutine(_leftCoroutine);
//	}
//
//	public void EndRightHaptic()
//	{
//		StopCoroutine(_rightCoroutine);
//	}
//
//	public void EndLeftHaptic()
//	{
//		StopCoroutine(_leftCoroutine);
//	}

	IEnumerator HapticSequence(HandRole role)
	{
		while (true)
		{
			ViveInput.TriggerHapticPulse(role, _duration);
			yield return new WaitForSeconds ( (float)_interval / 1000f);
		}
	}
}
