using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;
using DigiClay;

namespace DigiClay
{
	public class DeformTools
	{
		public enum ToolState
		{
			Idle,
			OneHand,
			TwoHand,
			Thickness,
			Smooth
		}
	}
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

			public DeformArgs(float inner, float outer)
			{
				innerRadius = inner;
				outerRadius = outer;
			}
		}

		public class UndoArgs
		{
			public DeformableBase deformable;
			public float height;
			public float thickness;
			public float[] radiusList;
			public int timeStamp;

			public UndoArgs(DeformableBase bd, float h, float t, float[] rl, int ts)
			{
				deformable = bd;
				height = h;
				thickness = t;
				radiusList = rl;
				timeStamp = ts;
			}
		}

		public DeformTools.ToolState ToolState;

		[Range(DigiClayConstant.CURSOR_MIN_RADIUS, DigiClayConstant.CURSOR_MAX_RADIUS), SerializeField]
		float m_innerRadius = 0.1f;
		[Range(0f, 1f), SerializeField]
		float m_ratio = 0.5f;
        [Range(DigiClayConstant.CURSOR_MIN_RADIUS, DigiClayConstant.CURSOR_MAX_RADIUS), SerializeField]
        float m_outerRadius = 0.5f;
		[Range(0.01f, 1f)]
		public float DeformRatio = 0.5f;
		[Range(0.01f, 1f)]
		public float RadialSmoothingRatio = 0.1f;
		[Range(0.01f, 1f)]
		public float LaplacianSmoothingRatio = 0.1f;
		[SerializeField]
		float m_radiusDeltaAmount = 0.01f;
		[SerializeField]
		float m_ratioDeltaAmount = 0.05f;

		// this is a flag for haptic
		bool[] m_isDeforming = new bool[2];
		bool[] m_isTouching = new bool[2];


		public void IsDeforming(HandRole role, bool b)
		{
			int i = (int)role;
			m_isDeforming [i] = b;
			HapticCheck(role);
		}

		public void IsTouching(HandRole role, bool b)
		{
			int i = (int)role;
			m_isTouching [i] = b;
			HapticCheck(role);
		}

		void HapticCheck(HandRole role)
		{
			int i = (int)role;

            if (m_isDeforming[i])
                HapticManager.Instance.StartHaptic(role);

            if (!m_isDeforming[i])
                HapticManager.Instance.EndHaptic(role);


            //if (m_isTouching[i])
            //	HapticManager.Instance.StartHaptic (role);

            //if (!m_isDeforming[i] && !m_isTouching[i])
            //	HapticManager.Instance.EndHaptic (role);
        }

		[HideInInspector]
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
				ValueChanged.Invoke (new DeformArgs(m_innerRadius, m_outerRadius));
			}
		}

		public float OuterRadius {
			get {
				return m_outerRadius;
			}
			set {
				m_outerRadius = Mathf.Clamp(value, DigiClayConstant.CURSOR_MIN_RADIUS, DigiClayConstant.CURSOR_MAX_RADIUS);
				ValueChanged.Invoke (new DeformArgs(m_innerRadius, m_outerRadius));
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
					//MeshGenerator.Instance.CreateMesh();
				}
				else if (angle > -135f && angle < -45f)
				{
					//down
					//MeshIOManager.Instance.ExportMesh();
				}
			});

			ViveInput.AddPressUp (HandRole.RightHand, ControllerButton.Menu, () => {
				//ToolState = (DeformTools.ToolState)(((int)ToolState + 1) % 5);
			});
		}

		void OnValidate()
		{
			InnerRadius = OuterRadius * Ratio;
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

}

