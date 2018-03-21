using System;
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

	[SerializeField]
	private bool[] m_ready = { false, false };

	[Range(0.01f, 5f), SerializeField]
	private float m_innerRadius = 0.1f;

	[Range(0.01f, 1f), SerializeField]
	private float m_ratio = 0.5f;

	[Range(0.01f, 5f), SerializeField]
	private float m_outerRadius = 0.5f;

	[Range(0f, 1f), SerializeField]
	private float m_strength = 1f;

	[SerializeField]
	float m_radiusDeltaAmount = 0.01f;
	[SerializeField]
	float m_ratioDeltaAmount = 0.05f;

	public float DeformRatio = 0.5f;
	public float RadialSmoothingRatio = 0.1f;
	public float LaplacianSmoothingRatio = 0.1f;

	// this is a flag for haptic
	public bool IsDeforming = false;

	public UnityEventDeform ValueChanged;

	public float Ratio {
		get {
			return m_ratio;
		}
		set {
			m_ratio = Mathf.Clamp01(value);
		}
	}

	public float InnerRadius {
		get {
			return m_innerRadius;
		}
		set {
			m_innerRadius = value;
			ValueChanged.Invoke (new DeformArgs(m_innerRadius, m_outerRadius, m_strength));
		}
	}

	public float OuterRadius {
		get {
			return m_outerRadius;
		}
		set {
			m_outerRadius = Mathf.Clamp(value, DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS);
			ValueChanged.Invoke (new DeformArgs(m_innerRadius, m_outerRadius, m_strength));
		}
	}

	public float Strength {
		get {
			return m_strength;
		}
		set {
			m_strength = value;
		}
	}

	public bool AreBothHandsReady {
		get {
			return m_ready[0] && m_ready[1];
		}
	}

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
		Init();
	}

	void Init()
	{

		ViveInput.AddPressUp (HandRole.RightHand, ControllerButton.Pad, () => {

			float x = ViveInput.GetAxis(HandRole.RightHand, ControllerAxis.PadX);
			float y = ViveInput.GetAxis(HandRole.RightHand, ControllerAxis.PadY);

			float angle = Mathf.Atan2(y,x) * Mathf.Rad2Deg;

			if (Mathf.Abs(angle) < 45f)
			{
				//right
				//+ inner
				Ratio += m_ratioDeltaAmount;
				InnerRadius = OuterRadius * Ratio;
			}
			else if (Mathf.Abs(angle) > 135f)
			{
				//left
				//- inner
				Ratio -= m_ratioDeltaAmount;
				InnerRadius = OuterRadius * Ratio;
			}
			else if (angle > 45f && angle < 135f)
			{
				//up
				//+ outer
				OuterRadius += m_radiusDeltaAmount;
				InnerRadius = OuterRadius * Ratio;
			}
			else if (angle > -135f && angle < -45f)
			{
				//down
				//- outer
				OuterRadius -= m_radiusDeltaAmount;
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

	void OnValidate()
	{
		InnerRadius = OuterRadius * Ratio;
	}

	public void SetHandStatus(HandRole role, bool value)
	{
		m_ready [(int)role] = value;
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
