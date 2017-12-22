using HTC.UnityPlugin.ColliderEvent;
using HTC.UnityPlugin.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Pose = HTC.UnityPlugin.PoseTracker.Pose;
using System.Collections;
using HTC.UnityPlugin.Vive;

public class BasicDeformable : MonoBehaviour
    , IColliderEventDragStartHandler
    , IColliderEventDragUpdateHandler
    , IColliderEventDragEndHandler
{
    [Serializable]
    public class UnityEventDeformable : UnityEvent<BasicDeformable> { }

    [SerializeField]
    private ColliderButtonEventData.InputButton m_grabButton = ColliderButtonEventData.InputButton.Trigger;

    public UnityEventDeformable deformStart = new UnityEventDeformable();
    public UnityEventDeformable deformEnd = new UnityEventDeformable();

    [Range(0.01f, 5f)]
	[SerializeField]
	private float _innerRadius = 0.1f;

    [Range(0.01f, 5f)]
	[SerializeField]
	private float _outerRadius = 0.5f;

    [Range(0f, 1f)]
	[SerializeField]
	private float _strength = 0.1f;

    MeshFilter _meshFilter;
    MeshCollider _meshCollider;

    Vector3[] _originalVertices;

    Vector3 _originalLocalPos;
	Transform _originalTransform;

	public float maxDist = 0.1f;

	bool _isSymmetric;
	int _role;

    Dictionary<int, float> verticeWeightDictionary = new Dictionary<int, float>();

    List<float> weightList;

    public List<float> WeightList
    {
        get
        {
            return weightList;
        }

        set
        {
            weightList = value;
        }
    }

    void Awake()
    {
        _meshFilter = GetComponentInChildren<MeshFilter>();
        _meshCollider = GetComponentInChildren<MeshCollider>();
    }

	void OnEnable()
	{
		Debug.Log ("Basic OnEnable " + gameObject.name + " frame " + Time.frameCount );
		DeformManager.Instance.ValueChanged.AddListener (DeformParameterChangedHandler);
	}

	void OnDisable()
	{
		DeformManager.Instance.ValueChanged.RemoveListener (DeformParameterChangedHandler);
	}

	void DeformParameterChangedHandler(DeformManager.DeformArgs args)
	{
		_innerRadius = args.innerRadius;
		_outerRadius = args.outerRadius;
		_strength = args.strength;
	}


    public virtual void OnColliderEventDragStart(ColliderButtonEventData eventData)
    {
        if (eventData.button != m_grabButton) { return; }

        var casterWorldPosition = eventData.eventCaster.transform.position;

        _originalVertices = _meshFilter.mesh.vertices;

		//register undo
		DeformManager.Instance.RegisterUndo(this, _originalVertices);

        // get all influenced vertices list, with falloff weights
        // list <float>
        Vector3[] vertices = _meshFilter.mesh.vertices;

        weightList = new List<float>();

        _originalLocalPos = transform.worldToLocalMatrix.MultiplyPoint(casterWorldPosition);

		_originalTransform = eventData.eventCaster.transform;

		_isSymmetric = DeformManager.Instance.Symmetric;

        for (int i = 0; i < vertices.Length; ++i)
        {
            float dist = 0f;

			if(_isSymmetric)
			{
				dist = Mathf.Abs(vertices[i].y - _originalLocalPos.y);
			}
			else
			{
				dist = Vector3.Distance(vertices[i], _originalLocalPos);
			}

			float weight = Falloff( _innerRadius, _outerRadius, dist);
			weightList.Add(weight);

            //TODO symmetric deform
        }

        if (deformStart != null)
        {
            deformStart.Invoke(this);
        }

		_role = eventData.eventCaster.gameObject.GetComponent<ViveColliderEventCaster> ().viveRole.roleValue;

		HapticManager.Instance.StartHaptic ((HandRole)_role);
//		HapticManager.Instance.StartRightHaptic ();
    }

    public virtual void OnColliderEventDragUpdate(ColliderButtonEventData eventData)
    {
		if (eventData.button != m_grabButton) { return; }

        var currentWorldPosition = eventData.eventCaster.transform.position;
        var originalWorldPosition = transform.localToWorldMatrix.MultiplyPoint(_originalLocalPos);

		Transform currentTransform = eventData.eventCaster.transform;

        Debug.DrawLine(originalWorldPosition, currentWorldPosition, Color.red);

        Vector3 offsetVector = currentWorldPosition - originalWorldPosition;

		float offsetDistance = Vector3.Distance(originalWorldPosition, currentWorldPosition);
		// 0m - 0.1m

		//Haptic
		HapticManager.Instance.Strength = Mathf.InverseLerp(0, maxDist, offsetDistance);


        //Debug.Log(string.Format("origin {0} | current {1} | offset {2}", originalWorldPosition.ToString("F3"), currentWorldPosition.ToString("F3"), offsetVector.ToString("F3")));

        Vector3[] vertices = _meshFilter.mesh.vertices;


        for (int i = 0; i < vertices.Length; ++i)
        {
            //early out if weight is 0
            if (weightList[i] == 0f)
                continue;


			Vector3 finalOffset;


			if(_isSymmetric)
			{


				Vector3 vertNormalDir = new Vector3 (vertices [i].x, 0f, vertices [i].z).normalized;

				float length = Vector3.ProjectOnPlane (offsetVector, Vector3.up).magnitude;

				var currentLocalPos = transform.worldToLocalMatrix.MultiplyPoint (currentWorldPosition);

				float sign = (currentLocalPos.sqrMagnitude > _originalLocalPos.sqrMagnitude) ? 1f : -1f;

				finalOffset = vertNormalDir * length * sign * _strength * weightList[i];
			}
			else
			{
				finalOffset = offsetVector * _strength * weightList[i];

				// TODO rotation
//				Matrix4x4 mx = Matrix4x4.identity;
//
//				finalOffset = offsetVector * _strength * weightList[i];
//
//				Quaternion q = Quaternion.FromToRotation (_originalTransform.forward, currentTransform.forward);
//
//				mx.SetTRS (finalOffset,
//					Quaternion.Lerp (Quaternion.identity, q, weightList [i]),
//					Vector3.one);
//
//				vertices [i] = mx.MultiplyPoint (_originalVertices [i]);
			}

			vertices[i] = _originalVertices[i] + finalOffset;

            //Debug.Log(string.Format("origin pos: {0}, weight: {1}, offsetVector: {2}, finalOffset: {3}, finalPos: {4}", _originalVertices[i].ToString("F3"), weightList[i], offsetVector.ToString("F3"), finalOffset.ToString("F3"), vertices[i].ToString("F3")));
        }

        _meshFilter.mesh.vertices = vertices;
        _meshFilter.mesh.RecalculateNormals();
    }

    public virtual void OnColliderEventDragEnd(ColliderButtonEventData eventData)
    {
		if (eventData.button != m_grabButton) { return; }

		//TODO remesh!

        _meshCollider.sharedMesh = _meshFilter.mesh;

        if (deformEnd != null)
        {
            deformEnd.Invoke(this);
        }
			
		HapticManager.Instance.EndHaptic ((HandRole)_role);
    }

	public void UndoDeform(Vector3[] vertices)
	{
		_meshFilter.mesh.vertices = vertices;
		_meshFilter.mesh.RecalculateNormals();
		_meshCollider.sharedMesh = _meshFilter.mesh;
	}

    float Falloff(float inner, float outer, float value)
    {
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


//for (int i = 0; i < vertices.Length; ++i)
//{
//	//early out if weight is 0
//	if (weightList[i] == 0f)
//		continue;
//
//	Vector3 finalOffset;
//
//	if(_isSymmetric)
//	{
//		Vector3 vertNormalDir = new Vector3 (vertices [i].x, 0f, vertices [i].z).normalized;
//
//		float length = Vector3.ProjectOnPlane (offsetVector, Vector3.up).magnitude;
//
//		var currentLocalPos = transform.worldToLocalMatrix.MultiplyPoint (currentWorldPosition);
//
//		float sign = (currentLocalPos.sqrMagnitude > _originalLocalPos.sqrMagnitude) ? 1f : -1f;
//
//		finalOffset = vertNormalDir * length * sign * _strength * weightList[i];
//	}
//	else
//	{
//		finalOffset = offsetVector * _strength * weightList[i];
//	}
//
//	vertices[i] = _originalVertices[i] + finalOffset;
//
//	//Debug.Log(string.Format("origin pos: {0}, weight: {1}, offsetVector: {2}, finalOffset: {3}, finalPos: {4}", _originalVertices[i].ToString("F3"), weightList[i], offsetVector.ToString("F3"), finalOffset.ToString("F3"), vertices[i].ToString("F3")));
//}