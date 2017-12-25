using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.ColliderEvent;
using System;
using UnityEngine.Events;

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

	protected void Awake()
	{
		m_meshFilter = GetComponentInChildren<MeshFilter>();
		m_meshCollider = GetComponentInChildren<MeshCollider>();
	}

	protected void OnEnable()
	{
		DeformManager.Instance.ValueChanged.AddListener (DeformParameterChangedHandler);
	}

	protected void OnDisable()
	{
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
		m_meshCollider.sharedMesh = m_meshFilter.mesh;
	}

	// Use this for initialization
	void Start () {
		
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
		return (1f - Mathf.InverseLerp(inner, outer, value));
	}
}
