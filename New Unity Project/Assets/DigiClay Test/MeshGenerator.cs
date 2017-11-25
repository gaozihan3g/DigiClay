using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    [SerializeField]
    [Range(0.05f, 0.5f)]
    float _radius = 1;

    [SerializeField]
    [Range(0.01f, 1f)]
    float _height = 1;

    [SerializeField]
    [Range(3, 60)]
    int _segment = 8;

    [SerializeField]
    [Range(1, 100)]
    int _verticalSegment = 10;

    [SerializeField]
    [Range(0.001f, 0.2f)]
    float _thickness = 0.01f;

    public Material[] _materials;

    [SerializeField]
    Transform _root = null;

    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;
    MeshCollider _meshCollider;

	float _delta;
	float _heightDelta;


    // Use this for initialization
    void Start() {

        //_root = GameObject.Find("GeometryRoot").transform;

        if (_root == null)
        {
            GameObject go = new GameObject("GeometryRoot");
            go.transform.parent = this.transform;
            _root = go.transform;
			_root.localPosition = Vector3.zero;

            _meshFilter = go.AddComponent<MeshFilter>();
            _meshRenderer = go.AddComponent<MeshRenderer>();
            _meshCollider = go.AddComponent<MeshCollider>();
        }
        else
        {
            _meshFilter = GetComponentInChildren<MeshFilter>();
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            _meshCollider = GetComponentInChildren<MeshCollider>();
        }

        Mesh mesh = new Mesh();
        mesh.name = "Generated Mesh";

        MeshGeneration(ref mesh);

        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;

        if (_materials.Length < mesh.subMeshCount)
            Debug.LogError("Materials are needed for submeshes!");

        _meshRenderer.materials = _materials;
    }


	void MeshGeneration(ref Mesh mesh)
	{
		List<Vector3> finalVertices = new List<Vector3>();
		List<int> outerTriangles = new List<int>();
		List<int> innerTriangles = new List<int>();
		List<int> edgeTriangles = new List<int>();
		int offset = 0;

		_delta = 2f * Mathf.PI / (float)_segment;
		_heightDelta = (float)_height / (float)_verticalSegment;

		GenerateOuterBottom (finalVertices, outerTriangles, ref offset);
//		GenerateInnerBottom (finalVertices, innerTriangles, ref offset);
		GenerateOuterSide (finalVertices, outerTriangles, ref offset);
//		GenerateInnerSide (finalVertices, innerTriangles, ref offset);
		GenerateEdge (finalVertices, edgeTriangles, ref offset);

		mesh.vertices = finalVertices.ToArray();

		mesh.subMeshCount = 3;
		//set outer submesh
		mesh.SetTriangles(outerTriangles.ToArray(), 0);
		mesh.SetTriangles(innerTriangles.ToArray(), 1);
		mesh.SetTriangles(edgeTriangles.ToArray(), 2);

		mesh.RecalculateNormals();
	}

	void GenerateOuterBottom(List<Vector3> finalVertices, List<int> finalTriangles, ref int offset)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		float theta = 0f;

		// origin
		newVertices.Add(Vector3.zero);

		for (int i = 0; i < _segment; ++i)
		{
			newVertices.Add(new Vector3(_radius * Mathf.Cos(theta), 0, _radius * Mathf.Sin(theta)));
			theta += _delta;
		}

		finalVertices.AddRange (newVertices);

		//add triangles
		for (int i = 1; i <= _segment; ++i)
		{
			CreateTriangle(newTriangles, 0, i, i % _segment + 1);
		}

		finalTriangles.AddRange (newTriangles);

		offset = finalVertices.Count;
	}

	void GenerateInnerBottom(List<Vector3> finalVertices, List<int> finalTriangles, ref int offset)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		float theta = 0f;
		float heightTheta = 0f;


		// inner origin
		Vector3 innerOrigin = new Vector3(0f, Mathf.Min(_thickness, 0.5f * _heightDelta), 0f);
		newVertices.Add(innerOrigin);

		float innerRadius = Mathf.Max(0, _radius - _thickness);

		theta = 0;
		for (int i = 0; i < _segment; ++i)
		{
			newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + innerOrigin.y, innerRadius * Mathf.Sin(theta)));
			theta += _delta;
		}

		finalVertices.AddRange (newVertices);

		//add triangles
		for (int i = 1; i <= _segment; ++i)
		{
			CreateTriangle(newTriangles, 0, i % _segment + 1, i, offset);
		}

		finalTriangles.AddRange (newTriangles);

		offset = finalVertices.Count;
	}

	void GenerateOuterSide(List<Vector3> finalVertices, List<int> finalTriangles, ref int offset)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		float theta = 0f;
		float heightTheta = 0f;

		for (int j = 0; j < _verticalSegment + 1; ++j)
		{
			for (int i = 0; i < _segment; ++i)
			{
				newVertices.Add(new Vector3(_radius * Mathf.Cos(theta), heightTheta, _radius * Mathf.Sin(theta)));
				theta += _delta;
			}
			heightTheta += _heightDelta;
		}

		finalVertices.AddRange (newVertices);

		for (int j = 0; j < _verticalSegment; ++j)
		{
			for (int i = 0; i < _segment; ++i)
			{
				CreateTriangle (newTriangles, i + _segment * j, (i + 1) % _segment + _segment * j, i + _segment * (j + 1), (i + 1) % _segment + _segment * (j + 1), offset);
			}
		}

		finalTriangles.AddRange (newTriangles);

		offset = finalVertices.Count;

	}

	void GenerateInnerSide(List<Vector3> finalVertices, List<int> finalTriangles, ref int offset)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		float theta = 0f;
		float heightTheta = 0f;

		Vector3 innerOrigin = new Vector3(0f, Mathf.Min(_thickness, 0.5f * _heightDelta), 0f);
		float innerRadius = Mathf.Max(0, _radius - _thickness);

		for (int j = 0; j < _verticalSegment + 1; ++j)
		{
			for (int i = 0; i < _segment; ++i)
			{
				newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? innerOrigin.y : 0), innerRadius * Mathf.Sin(theta)));
				theta += _delta;
			}
			heightTheta += _heightDelta;
		}

		finalVertices.AddRange (newVertices);

		for (int j = 0; j < _verticalSegment; ++j)
		{
			for (int i = 0; i < _segment; ++i)
			{
				CreateTriangle (newTriangles, (i + 1) % _segment + _segment * j, i + _segment * j, (i + 1) % _segment + _segment * (j + 1), i + _segment * (j + 1), offset);
			}
		}

		finalTriangles.AddRange (newTriangles);

		offset = finalVertices.Count;

	}

	void GenerateEdge(List<Vector3> finalVertices, List<int> finalTriangles, ref int offset)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		float theta = 0f;

		float innerRadius = Mathf.Max(0, _radius - _thickness);

		for (int i = 0; i < _segment; ++i)
		{
			newVertices.Add(new Vector3(_radius * Mathf.Cos(theta), _height, _radius * Mathf.Sin(theta)));
			newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), _height, innerRadius * Mathf.Sin(theta)));
			theta += _delta;
		}

		finalVertices.AddRange (newVertices);

		for (int i = 0; i < _segment; ++i)
		{
			CreateTriangle(newTriangles, 2 * i, 2 * ((i + 1) % _segment) , 2 * i + 1, 2 * ((i + 1) % _segment) + 1, offset);
		}

		finalTriangles.AddRange (newTriangles);

		offset = finalVertices.Count;
	}

	/// <summary>
	/// Creates the triangles from four points.
	/// C D,
	/// A B
	/// </summary>
	/// <param name="list">List.</param>
	/// <param name="a">The alpha component.</param>
	/// <param name="b">The blue component.</param>
	/// <param name="c">C.</param>
	/// <param name="d">D.</param>
	/// <param name="offset">Offset.</param>
	void CreateTriangle(List<int> list, int a, int b, int c, int d, int offset = 0)
	{
		CreateTriangle (list, a, c, b, offset);
		CreateTriangle (list, b, c, d, offset);
	}

    void CreateTriangle(List<int> list, int a, int b, int c, int offset = 0)
    {
        if (list == null)
            Debug.LogError("The triangle list is null");
        list.Add(a + offset);
        list.Add(b + offset);
        list.Add(c + offset);
    }
	
	// Update is called once per frame
	void Update () {

//        Mesh mesh = _meshFilter.sharedMesh;
//        mesh.Clear();
//
//        MeshGeneration(ref mesh);
//
//        _meshFilter.mesh = mesh;
//        _meshCollider.sharedMesh = mesh;
    }
}
