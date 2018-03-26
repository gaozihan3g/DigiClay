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
	//a ref, this might be null
	[SerializeField]
	protected ClayMeshContext m_clayMeshContext;
	protected MeshFilter m_meshFilter;
	protected MeshCollider m_meshCollider;

	protected Vector3[] m_orgVertices;
	protected List<float> m_weightList;
	protected float[] m_orgRadiusList;

	// hand position
	protected Vector3 m_orgHandLocalPos;
	protected Vector3 m_orgHandWorldPos;
	protected Vector3 m_prevHandWorldPos;
	protected Vector3 m_curHandWorldPos;
	protected HandRole m_role;

	[HideInInspector]
	public UnityEventDeformable OnDeformStart = new UnityEventDeformable();
	[HideInInspector]
	public UnityEventDeformable OnDeformEnd = new UnityEventDeformable();

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

	#region IColliderEventDragStartHandler implementation
	public virtual void OnColliderEventDragStart (ColliderButtonEventData eventData)
	{
		// hand position
		m_role = (HandRole)(eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster>().viveRole.roleValue);
		m_orgHandWorldPos = eventData.eventCaster.transform.position;
		m_orgHandLocalPos = m_orgHandWorldPos - transform.position;
		m_prevHandWorldPos = m_orgHandWorldPos;

        RegisterOrgClayMesh();

		DeformManager.Instance.IsDeforming (m_role, true);

		if (OnDeformStart != null)
		{
			OnDeformStart.Invoke(this);
		}
	}

	public virtual void OnColliderEventDragUpdate (ColliderButtonEventData eventData)
	{
		m_clayMeshContext.clayMesh.UpdateMesh();
		UpdateHapticStrength(m_role, m_prevHandWorldPos, m_curHandWorldPos);
		m_prevHandWorldPos = m_curHandWorldPos;
	}


	public virtual void OnColliderEventDragEnd (ColliderButtonEventData eventData)
	{
		m_meshCollider.sharedMesh = m_meshFilter.sharedMesh;

		DeformManager.Instance.IsDeforming (m_role, false);

		if (OnDeformEnd != null)
		{
			OnDeformEnd.Invoke(this);
		}
	}
	#endregion

	protected virtual void Awake()
	{
		m_meshFilter = GetComponent<MeshFilter>();
		m_meshCollider = GetComponent<MeshCollider>();
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
	}

	public void UndoDeform(DeformManager.UndoArgs args)
	{
		var clayMesh = m_clayMeshContext.clayMesh;

		// register current to REDO
		var curRadiusList = new float[clayMesh.RadiusList.Count];
		clayMesh.RadiusList.CopyTo(curRadiusList);

		DeformManager.Instance.RegisterRedo(new DeformManager.UndoArgs(this, clayMesh.Height,
			clayMesh.ThicknessRatio, curRadiusList, Time.frameCount));


		// update with args
		clayMesh.Height = args.height;
		clayMesh.ThicknessRatio = args.thicknessRatio;

		if (args.radiusList != null)
			clayMesh.RadiusList = new List<float>(args.radiusList);

		clayMesh.UpdateMesh();

		m_meshFilter.sharedMesh = clayMesh.Mesh;
		m_meshCollider.sharedMesh = clayMesh.Mesh;
	}

	public void RedoDeform(DeformManager.UndoArgs args)
	{
		var clayMesh = m_clayMeshContext.clayMesh;

		// register current to UNDO
		var curRadiusList = new float[clayMesh.RadiusList.Count];
		clayMesh.RadiusList.CopyTo(curRadiusList);

		DeformManager.Instance.RegisterUndo(new DeformManager.UndoArgs(this, clayMesh.Height,
			clayMesh.ThicknessRatio, curRadiusList, Time.frameCount));

		// update with args
		clayMesh.Height = args.height;
		clayMesh.ThicknessRatio = args.thicknessRatio;

		if (args.radiusList != null)
			clayMesh.RadiusList = new List<float>(args.radiusList);

		clayMesh.UpdateMesh();

		m_meshFilter.sharedMesh = clayMesh.Mesh;
		m_meshCollider.sharedMesh = clayMesh.Mesh;
	}

	// Use this for initialization
	protected virtual void Start () {
		Init ();
	}

	void Init()
	{
		m_innerRadius = DeformManager.Instance.InnerRadius;
		m_outerRadius = DeformManager.Instance.OuterRadius;
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

    protected void UpdateWeightList(Vector3 handLocalPos)
    {
        m_weightList = new List<float>();

        for (int i = 0; i < m_orgVertices.Length; ++i)
        {
            float dist = 0f;
            dist = Mathf.Abs(m_orgVertices[i].y - handLocalPos.y);

            float weight = Falloff(m_innerRadius, m_outerRadius, dist);
            m_weightList.Add(weight);
        }
    }

    protected void UpdateHapticStrength(HandRole role, Vector3 oldP, Vector3 newP)
	{
		float dist = Vector3.Distance (oldP, newP);
		HapticManager.Instance.SetRoleStrength(role, dist / DigiClayConstant.HAPTIC_MAX_DISTANCE);
	}

    //TODO move to base
    protected void RegisterOrgClayMesh()
    {
        m_orgVertices = m_meshFilter.sharedMesh.vertices;
        m_orgRadiusList = new float[m_clayMeshContext.clayMesh.RadiusList.Count];
        m_clayMeshContext.clayMesh.RadiusList.CopyTo(m_orgRadiusList);

        //register undo
        DeformManager.Instance.RegisterUndo(new DeformManager.UndoArgs(this, m_clayMeshContext.clayMesh.Height,
            m_clayMeshContext.clayMesh.ThicknessRatio, m_orgRadiusList, Time.frameCount));
        DeformManager.Instance.ClearRedo();
    }
}
