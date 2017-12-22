using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour {

    [SerializeField]
	[Range(DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS)]
    float _radius = 1;

    [SerializeField]
	[Range(DigiClayConstant.MIN_HEIGHT, DigiClayConstant.MAX_HEIGHT)]
	float _height = 1;

	public float Height {
		get {
			return _height;
		}
	}

    [SerializeField]
    [Range(3, 60)]
    int _segment = 8;

    [SerializeField]
    [Range(1, 100)]
    int _verticalSegment = 10;

    [SerializeField]
    [Range(0.001f, 0.2f)]
    float _thickness = 0.01f;

    public float noiseScale;
    public float noiseSpan;

    public Material[] _materials;

    [SerializeField]
    Transform _root = null;

//	AdvancedMeshContext _advMeshContext;
    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;
    MeshCollider _meshCollider;

//	public AdvancedMesh _advMesh;

	float _delta;
	float _heightDelta;

	[SerializeField]
	bool OuterBottom = true;
	[SerializeField]
	bool InnerBottom = true;
	[SerializeField]
	bool OuterSide = true;
	[SerializeField]
	bool InnerSide = true;
	[SerializeField]
	bool Edge = true;

    Perlin _perlin = new Perlin();

	void Awake()
	{
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
	}

    void Start() {

		Mesh generatedMesh = new Mesh();
        generatedMesh.name = "Generated Mesh";

        MeshGeneration(ref generatedMesh);

		_meshFilter.mesh = generatedMesh;
		_meshCollider.sharedMesh = generatedMesh;

		Debug.Assert (_materials.Length >= generatedMesh.subMeshCount, "Materials are needed for submeshes!");

		for (int i = 0; i < generatedMesh.subMeshCount; i++)
			_meshRenderer.sharedMaterials[i] = _materials[i];
    }


	void MeshGeneration(ref Mesh mesh)
	{
		List<Vector3> finalVertices = new List<Vector3>();
		List<int> outerTriangles = new List<int>();
		List<int> innerTriangles = new List<int>();
		List<int> edgeTriangles = new List<int>();
		List<Vector2> finalUVs = new List<Vector2> ();
		int offset = 0;

		_delta = 2f * Mathf.PI / (float)_segment;
		_heightDelta = (float)_height / (float)_verticalSegment;


		if (OuterBottom)
			GenerateOuterBottom (finalVertices, outerTriangles, finalUVs, ref offset);
		if (InnerBottom)
			GenerateInnerBottom (finalVertices, innerTriangles, finalUVs, ref offset);
		if (OuterSide)
			GenerateOuterSide (finalVertices, outerTriangles, finalUVs, ref offset);
		if (InnerSide)
			GenerateInnerSide (finalVertices, innerTriangles, finalUVs, ref offset);
		if (Edge)
			GenerateEdge (finalVertices, edgeTriangles, finalUVs, ref offset);

		mesh.vertices = finalVertices.ToArray();
		mesh.uv = finalUVs.ToArray ();

//		Debug.Log ("vertices : " + mesh.vertices.Length);
//		Debug.Log ("UVs : " + mesh.uv.Length);

		int meshCount = 0;

		if (outerTriangles.Count != 0)
			++meshCount;
		if (innerTriangles.Count != 0)
			++meshCount;
		if (edgeTriangles.Count != 0)
			++meshCount;

		mesh.subMeshCount = meshCount;

		int subMeshIndex = 0;
		//set outer submesh

		if (outerTriangles.Count != 0)
			mesh.SetTriangles(outerTriangles.ToArray(), subMeshIndex++);
		if (innerTriangles.Count != 0)
			mesh.SetTriangles(innerTriangles.ToArray(), subMeshIndex++);
		if (edgeTriangles.Count != 0)
			mesh.SetTriangles(edgeTriangles.ToArray(), subMeshIndex++);

		mesh.RecalculateNormals();
	}

	void GenerateOuterBottom(List<Vector3> finalVertices, List<int> finalTriangles, List<Vector2> finalUVs, ref int offset)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;

		// origin
		newVertices.Add(Vector3.zero);
		newUVs.Add (Vector2.zero);

		for (int i = 0; i < _segment; ++i)
		{
			newVertices.Add(new Vector3(_radius * Mathf.Cos(theta), 0, _radius * Mathf.Sin(theta)));
			newUVs.Add (Vector2.zero);
			theta += _delta;
		}

		finalVertices.AddRange (newVertices);

		//add triangles
		for (int i = 1; i <= _segment; ++i)
		{
			CreateTriangle(newTriangles, 0, i, i % _segment + 1);
		}

		finalTriangles.AddRange (newTriangles);

		finalUVs.AddRange (newUVs);

		offset = finalVertices.Count;
	}

	void GenerateInnerBottom(List<Vector3> finalVertices, List<int> finalTriangles, List<Vector2> finalUVs, ref int offset)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;
		float heightTheta = 0f;


		// inner origin
		Vector3 innerOrigin = new Vector3(0f, Mathf.Min(_thickness, 0.5f * _heightDelta), 0f);
		newVertices.Add(innerOrigin);
		newUVs.Add (Vector2.zero);

		float innerRadius = Mathf.Max(0, _radius - _thickness);

		theta = 0;
		for (int i = 0; i < _segment; ++i)
		{
			newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + innerOrigin.y, innerRadius * Mathf.Sin(theta)));
			newUVs.Add (Vector2.zero);
			theta += _delta;
		}

		finalVertices.AddRange (newVertices);

		//add triangles
		for (int i = 1; i <= _segment; ++i)
		{
			CreateTriangle(newTriangles, 0, i % _segment + 1, i, offset);
		}

		finalTriangles.AddRange (newTriangles);

		finalUVs.AddRange (newUVs);

		offset = finalVertices.Count;
	}

	void GenerateOuterSide(List<Vector3> finalVertices, List<int> finalTriangles, List<Vector2> finalUVs, ref int offset)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;
		float heightTheta = 0f;

		for (int j = 0; j < _verticalSegment + 1; ++j)
		{
			theta = 0f;

//			_segment + 1
			for (int i = 0; i < _segment + 1; ++i)
			{
                var pos = new Vector3(_radius * Mathf.Cos(theta), heightTheta, _radius * Mathf.Sin(theta));

                var finalPos = pos + new Vector3(pos.x, 0f, pos.z).normalized * _perlin.Noise(pos.x * noiseSpan, pos.y * noiseSpan, pos.z * noiseSpan) * noiseScale;

                newVertices.Add(finalPos);
				newUVs.Add (new Vector2 ( 1f / (float)_segment * i, 1f / (float)_verticalSegment * j));

				theta += _delta;
			}

			heightTheta += _heightDelta;
		}

//		for (int i = 0; i < newVertices.Count; ++i) {
//			Debug.Log ("i = " + i + " vertices : " + newVertices[i] + " uv: " + newUVs[i]);
//		}


		finalVertices.AddRange (newVertices);

		int newSeg = _segment + 1;

		for (int j = 0; j < _verticalSegment; ++j)
		{
			for (int i = 0; i < _segment; ++i)
			{
				CreateTriangle (newTriangles, i + newSeg * j, i + 1 + newSeg * j, i + newSeg * (j + 1), i + 1 + newSeg * (j + 1), offset);
			}
		}

		finalTriangles.AddRange (newTriangles);

		finalUVs.AddRange (newUVs);

		offset = finalVertices.Count;

	}

	void GenerateInnerSide(List<Vector3> finalVertices, List<int> finalTriangles, List<Vector2> finalUVs, ref int offset)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;
		float heightTheta = 0f;

		Vector3 innerOrigin = new Vector3(0f, Mathf.Min(_thickness, 0.5f * _heightDelta), 0f);
		float innerRadius = Mathf.Max(0, _radius - _thickness);

		for (int j = 0; j < _verticalSegment + 1; ++j)
		{
			theta = 0f;

			for (int i = 0; i < _segment; ++i)
			{
				newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? innerOrigin.y : 0), innerRadius * Mathf.Sin(theta)));
				newUVs.Add (Vector2.zero);
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

		finalUVs.AddRange (newUVs);

		offset = finalVertices.Count;

	}

	void GenerateEdge(List<Vector3> finalVertices, List<int> finalTriangles, List<Vector2> finalUVs, ref int offset)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;

		float innerRadius = Mathf.Max(0, _radius - _thickness);

		for (int i = 0; i < _segment; ++i)
		{
			newVertices.Add(new Vector3(_radius * Mathf.Cos(theta), _height, _radius * Mathf.Sin(theta)));
			newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), _height, innerRadius * Mathf.Sin(theta)));
			newUVs.Add (Vector2.zero);
			newUVs.Add (Vector2.zero);
			theta += _delta;
		}

		finalVertices.AddRange (newVertices);

		for (int i = 0; i < _segment; ++i)
		{
			CreateTriangle(newTriangles, 2 * i, 2 * ((i + 1) % _segment) , 2 * i + 1, 2 * ((i + 1) % _segment) + 1, offset);
		}

		finalTriangles.AddRange (newTriangles);

		finalUVs.AddRange (newUVs);

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

		//only in editor mode
		if (!Application.isPlaying)
		{
	        Mesh mesh = _meshFilter.sharedMesh;
	        mesh.Clear();
	
	        MeshGeneration(ref mesh);
	
	        _meshFilter.mesh = mesh;
	        _meshCollider.sharedMesh = mesh;
		}
    }
}
