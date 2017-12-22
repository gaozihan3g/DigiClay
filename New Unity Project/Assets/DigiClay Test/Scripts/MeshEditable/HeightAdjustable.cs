using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshGenerator))]
public class HeightAdjustable : MonoBehaviour {

	[Range(DigiClayConstant.MIN_HEIGHT, DigiClayConstant.MAX_HEIGHT)]
	public float _height = 1f;

    public ControlWidget controlWidget;

	

	MeshFilter _meshFilter;
	MeshCollider _meshCollider;

	MeshGenerator _meshGenerator;
	float _originalHeight = 1f;

	Vector3[] _originalVertices;

	IEnumerator Start()
	{
		yield return new WaitForEndOfFrame ();

		_meshFilter = GetComponentInChildren<MeshFilter>();
		_meshCollider = GetComponentInChildren<MeshCollider>();

		// this could be null, if the mesh is not generated from code
		// added required component
		_meshGenerator = GetComponent<MeshGenerator> ();

		if (_meshGenerator != null) {
			_height = _meshGenerator.Height;
		}

		_originalVertices = _meshFilter.mesh.vertices;
	}

    private void OnEnable()
    {
        controlWidget.controlStartEvent.AddListener(HeightChangeBeginHandler);
        controlWidget.controlChangedEvent.AddListener(HeightChangeHandler);
    }


    void Update () {

	}

    public void HeightChangeBeginHandler()
    {
        Debug.Log("HeightChangeBeginHandler");
        _originalHeight = _height;
    }

    public void HeightChangeHandler(float newValue)
    {
        Debug.Log("HeightChangeHandler");

		if (SystemManager.Instance.Mode != SystemManager.EditMode.HeightControl)
			return;

		if (_meshFilter == null)
			return;

		Vector3[] newVerts = _meshFilter.mesh.vertices;

        _height = _originalHeight * newValue;

		for (int i = 0; i < newVerts.Length; ++i)
		{
            newVerts[i].y = _originalVertices[i].y * _height;
		}

		_meshFilter.mesh.vertices = newVerts;

		_meshFilter.mesh.RecalculateNormals();

		//TODO not ideal
		_meshCollider.sharedMesh = _meshFilter.mesh;
    }
}
