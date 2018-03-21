﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;
using DigiClay;

public class DeformManager : MonoBehaviour {

	public static DeformManager Instance;

	Stack<UndoArgs> m_undoStack = new Stack<UndoArgs>();
	Stack<UndoArgs> m_redoStack = new Stack<UndoArgs>();

	[Serializable]
	public class UnityEventDeform : UnityEvent<DeformArgs> { }

	public class DeformArgs
	{
		public float innerRadius;
		public float outerRadius;
		public float strength;

		public DeformArgs(float inner, float outer, float str)
		{
			innerRadius = inner;
			outerRadius = outer;
			strength = str;
		}
	}

	public class UndoArgs
	{
		public DeformableBase deformable;
		public float height;
		public float thicknessRatio;
		public float[] radiusList;
		public int timeStamp;

		public UndoArgs(DeformableBase bd, float h, float t, float[] rl, int ts)
		{
			deformable = bd;
			height = h;
			thicknessRatio = t;
			radiusList = rl;
			timeStamp = ts;
		}
	}

	public UnityEventDeform ValueChanged;

	[SerializeField]
	private bool[] m_ready = { false, false };

	public bool IsBothHandReady {
		get {
			return m_ready[0] && m_ready[1];
		}
	}

	public void SetHandStatus(HandRole role, bool value)
	{
		m_ready [(int)role] = value;
	}

	[Range(0.01f, 5f), SerializeField]
	private float _innerRadius = 0.1f;

	[Range(0.01f, 1f), SerializeField]
	private float m_ratio = 0.5f;

	[Range(0.01f, 5f), SerializeField]
	private float _outerRadius = 0.5f;

	[Range(0f, 1f), SerializeField]
	private float _strength = 1f;

	public float Ratio {
		get {
			return m_ratio;
		}
		set {
			if (value < 0f)
				value = 0f;
			
			if (value > 1f)
				value = 1f;
			
			m_ratio = value;
		}
	}

	public float InnerRadius {
		get {
			return _innerRadius;
		}
		set {
			_innerRadius = value;

			ValueChanged.Invoke (new DeformArgs(_innerRadius, _outerRadius, _strength));
		}
	}

	public float OuterRadius {
		get {
			return _outerRadius;
		}
		set {
			
			if (value > DigiClayConstant.MAX_RADIUS)
				value = DigiClayConstant.MAX_RADIUS;
			
			if (value < DigiClayConstant.MIN_RADIUS)
				value = DigiClayConstant.MIN_RADIUS;

			_outerRadius = value;
			ValueChanged.Invoke (new DeformArgs(_innerRadius, _outerRadius, _strength));
		}
	}

	public float Strength {
		get {
			return _strength;
		}
		set {
			_strength = value;
		}
	}

	public float deltaAmount = 0.01f;
	public float ratioDeltaAmount = 0.05f;

	public float DeformRatio = 0.5f;
	public float RadialSmoothingRatio = 0.1f;
	public float LaplacianSmoothingRatio = 0.1f;

    void Awake()
    {
		Debug.Log ("DeformManager Awake" + gameObject.name + " frame " + Time.frameCount );

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

	void Start()
	{
		//initialization
		Debug.Log("DefromManager Start");

		ViveInput.AddPressUp (HandRole.RightHand, ControllerButton.Pad, () => {

			float x = ViveInput.GetAxis(HandRole.RightHand, ControllerAxis.PadX);
			float y = ViveInput.GetAxis(HandRole.RightHand, ControllerAxis.PadY);

			float angle = Mathf.Atan2(y,x) * Mathf.Rad2Deg;

			if (Mathf.Abs(angle) < 45f)
			{
				//right
				//+ inner
				Ratio += ratioDeltaAmount;
				InnerRadius = OuterRadius * Ratio;
			}
			else if (Mathf.Abs(angle) > 135f)
			{
				//left
				//- inner
				Ratio -= ratioDeltaAmount;
				InnerRadius = OuterRadius * Ratio;
			}
			else if (angle > 45f && angle < 135f)
			{
				//up
				//+ outer
				OuterRadius += deltaAmount;
				InnerRadius = OuterRadius * Ratio;
			}
			else if (angle > -135f && angle < -45f)
			{
				//down
				//- outer
				OuterRadius -= deltaAmount;
				InnerRadius = OuterRadius * Ratio;
			}
		});

		ViveInput.AddPressUp (HandRole.LeftHand, ControllerButton.Pad, () => {

			float x = ViveInput.GetAxis(HandRole.LeftHand, ControllerAxis.PadX);
			float y = ViveInput.GetAxis(HandRole.LeftHand, ControllerAxis.PadY);

			float angle = Mathf.Atan2(y,x) * Mathf.Rad2Deg;

			if (Mathf.Abs(angle) < 45f)
			{
				//right
				PerformRedo();
			}
			else if (Mathf.Abs(angle) > 135f)
			{
				//left
				//undo
				PerformUndo();
			}
			else if (angle > 45f && angle < 135f)
			{
				//up
				MeshGenerator.Instance.CreateMesh();
			}
			else if (angle > -135f && angle < -45f)
			{
				//down
				MeshIOManager.Instance.ExportMesh();
			}
		});

		ViveInput.AddPressUp (HandRole.RightHand, ControllerButton.Menu, () => {
		});
	}
		

	public void RegisterUndo(UndoArgs args)
	{
		m_undoStack.Push (args);
		Debug.Log ("undo registered " + args.timeStamp + " stack size " + m_undoStack.Count);
	}

	public void ClearRedo()
	{
		m_redoStack.Clear ();
	}

	public void RegisterRedo(UndoArgs args)
	{
		m_redoStack.Push (args);
		Debug.Log ("redo registered " + args.timeStamp + " stack size " + m_redoStack.Count);
	}

	public void PerformUndo()
	{
		if (m_undoStack.Count == 0)
			return;

		var args = m_undoStack.Pop ();
		Debug.Log ("undo performed " + args.timeStamp + " stack size " + m_undoStack.Count);

		args.deformable.UndoDeform (args);
	}

	public void PerformRedo()
	{
		if (m_redoStack.Count == 0)
			return;

		var args = m_redoStack.Pop ();
		Debug.Log ("redo performed " + args.timeStamp + " stack size " + m_redoStack.Count);
		args.deformable.RedoDeform (args);
	}
}
