using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.ColliderEvent;
using System;
using UnityEngine.Events;
using DigiClay;
using HTC.UnityPlugin.Vive;

public abstract class DeformableBase : MonoBehaviour
, IColliderEventDragStartHandler
, IColliderEventDragUpdateHandler
, IColliderEventDragEndHandler
{
	[Serializable]
	public class UnityEventDeformable : UnityEvent<DeformableBase> {}

	[SerializeField]
	protected ColliderButtonEventData.InputButton m_deformButton = ColliderButtonEventData.InputButton.Trigger;

	[SerializeField]
	protected float m_innerRadius = 0.1f;

	[SerializeField]
	protected float m_outerRadius = 0.5f;

	[SerializeField]
	protected float m_strength = 0.1f;

	//a ref, this might be null
	[SerializeField]
	protected ClayMeshContext m_clayMeshContext;
	protected MeshFilter m_meshFilter;
	protected MeshCollider m_meshCollider;

	protected Vector3[] m_originalVertices;
	protected List<float> m_weightList;

	public List<float> WeightList
	{
		get
		{
			return m_weightList;
		}

		set
		{
			m_weightList = value;
		}
	}

	public UnityEventDeformable OnDeformStart = new UnityEventDeformable();
	public UnityEventDeformable OnDeformEnd = new UnityEventDeformable();

	#region IColliderEventDragStartHandler implementation
	public virtual void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
		throw new System.NotImplementedException ();
	}

	public virtual void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
	{
		throw new System.NotImplementedException ();
	}


	public virtual void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
		throw new System.NotImplementedException ();
	}
	#endregion

	protected virtual void Awake()
	{
		m_meshFilter = GetComponentInChildren<MeshFilter>();
		m_meshCollider = GetComponentInChildren<MeshCollider>();

		// this might be null
		m_clayMeshContext = GetComponent<ClayMeshContext>();
	}

	protected virtual void OnEnable()
	{
		if (DeformManager.Instance != null)
			DeformManager.Instance.ValueChanged.AddListener (DeformParameterChangedHandler);
	}

	protected virtual void OnDisable()
	{
		if (DeformManager.Instance != null)
			DeformManager.Instance.ValueChanged.RemoveListener (DeformParameterChangedHandler);
	}

	protected void DeformParameterChangedHandler(DeformManager.DeformArgs args)
	{
		m_innerRadius = args.innerRadius;
		m_outerRadius = args.outerRadius;
		m_strength = args.strength;
	}

	public void UndoDeform(Vector3[] vertices)
	{
		m_meshFilter.mesh.vertices = vertices;
		m_meshFilter.mesh.RecalculateNormals();

//		if (m_clayMeshContext != null)
//			m_clayMeshContext.clayMesh.RecalculateNormals ();
//		else
//			m_meshFilter.mesh.RecalculateNormals();
			
		m_meshCollider.sharedMesh = m_meshFilter.mesh;
	}

	// Use this for initialization
	protected virtual void Start () {
		Init ();
	}

	void Init()
	{
		m_innerRadius = DeformManager.Instance.InnerRadius;
		m_outerRadius = DeformManager.Instance.OuterRadius;
		m_strength = DeformManager.Instance.Strength;
	}
	
	// Update is called once per frame
	void Update () {
	}

	protected float Falloff(float inner, float outer, float value)
	{
		// TODO non linear
		//   - inner - ~ - outer -
		// 1 - 1     - ~ - 0     - 0

		if (value < inner)
			return 1f;
		if (value > outer)
			return 0f;

		//linear
//		return (1f - Mathf.InverseLerp(inner, outer, value));

		//return (outer - value) / (outer - inner);

        //non-linear
        return NonLinear((value - inner) / (outer - inner));
	}

    protected float NonLinear(float x)
    {
        return 2f * x * x * x - 3f * x * x + 1f;
    }

	protected void TriggerHaptic(HandRole role, Vector3 previous, Vector3 current)
	{
		float dist = Vector3.Distance (previous, current);

		Debug.Log (string.Format ("{0} {1:F3} {2}", role, dist, Time.frameCount));

		float t = dist / DeformManager.Instance.MaxDist;

		float duration = Mathf.Lerp (DeformManager.Instance.MinDuration, DeformManager.Instance.MaxDuration, t);

		ViveInput.TriggerHapticPulse(role, (ushort)duration);
	}
}
