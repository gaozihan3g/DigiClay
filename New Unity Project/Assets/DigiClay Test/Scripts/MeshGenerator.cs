using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

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
    [Range(1, 10)]
    int _edgeSegment = 5;

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

    Vector3 _innerOrigin;
    float _innerRadius;

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
        List<Vector2Int> uvSeams = new List<Vector2Int>();
		int offset = 0;

		_delta = 2f * Mathf.PI / (float)_segment;
		_heightDelta = (float)_height / (float)_verticalSegment;

        _innerOrigin = new Vector3(0f, Mathf.Min(_thickness, 0.5f * _heightDelta), 0f);
        _innerRadius = Mathf.Max(0, _radius - _thickness);

		if (OuterBottom)
			GenerateOuterBottom (finalVertices, outerTriangles, finalUVs, ref offset);
		if (InnerBottom)
			GenerateInnerBottom (finalVertices, innerTriangles, finalUVs, ref offset);
		if (OuterSide)
            GenerateOuterSide (finalVertices, outerTriangles, finalUVs, ref offset, ref uvSeams);
		if (InnerSide)
            GenerateInnerSide (finalVertices, innerTriangles, finalUVs, ref offset, ref uvSeams);
		if (Edge)
            GenerateEdge (finalVertices, outerTriangles, finalUVs, ref offset);

		mesh.vertices = finalVertices.ToArray();
		mesh.uv = finalUVs.ToArray ();

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
        mesh.FixUVSeam(uvSeams.ToArray());
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
            var pos = new Vector3(_radius * Mathf.Cos(theta), 0, _radius * Mathf.Sin(theta));

            var noiseCoordinate = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * noiseSpan;

            var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                     * _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                     * noiseScale;

            var finalPos = pos + noise;

            newVertices.Add(finalPos);
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

		newVertices.Add(_innerOrigin);
		newUVs.Add (Vector2.zero);

		theta = 0;
		for (int i = 0; i < _segment; ++i)
		{

            var pos = new Vector3(_innerRadius * Mathf.Cos(theta), heightTheta + _innerOrigin.y, _innerRadius * Mathf.Sin(theta));

            var noiseCoordinate = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * noiseSpan;

            var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                     * _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                     * noiseScale;

            var finalPos = pos + noise;

            newVertices.Add(finalPos);

			//newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + innerOrigin.y, innerRadius * Mathf.Sin(theta)));

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

    void GenerateOuterSide(List<Vector3> finalVertices, List<int> finalTriangles, List<Vector2> finalUVs, ref int offset, ref List<Vector2Int> uvSeams)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;
		float heightTheta = 0f;

        //Mesh Grid
        // (_segment + 1) * (_verticalSegment + 1)
		for (int j = 0; j < _verticalSegment + 1; ++j)
		{
			theta = 0f;

			for (int i = 0; i < _segment + 1; ++i)
			{
                var pos = new Vector3(_radius * Mathf.Cos(theta), heightTheta, _radius * Mathf.Sin(theta));

                var noiseCoordinate = new Vector3(Mathf.Cos(theta), heightTheta, Mathf.Sin(theta)) * noiseSpan;

                var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                         * _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                         * noiseScale;

                var finalPos = pos + noise;

                newVertices.Add(finalPos);

				newUVs.Add (new Vector2 ( 1f / (float)_segment * i, 1f / (float)_verticalSegment * j));

				theta += _delta;
			}

            // store vertex pairs for normal fix
            int x = 0 + j * (_segment + 1) + offset;
            int y = _segment + j * (_segment + 1) + offset;

            uvSeams.Add(new Vector2Int(x, y));

			heightTheta += _heightDelta;
		}

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

    void GenerateInnerSide(List<Vector3> finalVertices, List<int> finalTriangles, List<Vector2> finalUVs, ref int offset, ref List<Vector2Int> uvSeams)
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;
		float heightTheta = 0f;

		for (int j = 0; j < _verticalSegment + 1; ++j)
		{
			theta = 0f;

			for (int i = 0; i < _segment + 1; ++i)
			{

                ///
                var pos = new Vector3(_innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? _innerOrigin.y : 0), _innerRadius * Mathf.Sin(theta));

                var noiseCoordinate = new Vector3(Mathf.Cos(theta), heightTheta, Mathf.Sin(theta)) * noiseSpan;

                var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                         * _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                         * noiseScale;

                var finalPos = pos + noise;

                newVertices.Add(finalPos);

				//newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? innerOrigin.y : 0), innerRadius * Mathf.Sin(theta)));
                newUVs.Add(new Vector2(1f / (float)_segment * i, 1f / (float)_verticalSegment * j));
				//newUVs.Add (Vector2.zero);
				theta += _delta;
			}

            // store vertex pairs for normal fix
            int x = 0 + j * (_segment + 1) + offset;
            int y = _segment + j * (_segment + 1) + offset;

            uvSeams.Add(new Vector2Int(x, y));

			heightTheta += _heightDelta;
		}

		finalVertices.AddRange (newVertices);

		//for (int j = 0; j < _verticalSegment; ++j)
		//{
		//	for (int i = 0; i < _segment; ++i)
		//	{
		//		CreateTriangle (newTriangles, (i + 1) % _segment + _segment * j, i + _segment * j, (i + 1) % _segment + _segment * (j + 1), i + _segment * (j + 1), offset);
		//	}
		//}


        ///
        int newSeg = _segment + 1;

        for (int j = 0; j < _verticalSegment; ++j)
        {
            for (int i = 0; i < _segment; ++i)
            {
                CreateTriangle(newTriangles, i + 1 + newSeg * j, i + newSeg * j, i + 1 + newSeg * (j + 1), i + newSeg * (j + 1), offset);
            }
        }
        ///




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
        float edgeTheta = 0f;

        float edgeDelta = (_radius - _innerRadius) / _edgeSegment;
        float radius;


        // height curve:
        // y = 2 * Mathf.Sqrt(x * (1 - x));

        for (int j = 0; j < _edgeSegment + 1; ++j)
        {
            theta = 0f;
            radius = _innerRadius + edgeTheta;

            float x = (float)j / (float)_edgeSegment;

            Debug.Log("x " + x);

            float y = 2 * Mathf.Sqrt(x * (1 - x));

            float edgeSegHeight = y * _heightDelta;


            for (int i = 0; i < _segment + 1; ++i)
            {
                var pos = new Vector3(radius * Mathf.Cos(theta), _height + edgeSegHeight, radius * Mathf.Sin(theta));
                var noiseCoordinate = new Vector3(Mathf.Cos(theta), _height, Mathf.Sin(theta)) * noiseSpan;
                var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                         * _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                         * noiseScale;
                var finalPos = pos + noise;
                newVertices.Add(finalPos);
                newUVs.Add(Vector2.zero);
                theta += _delta;
            }
            edgeTheta += edgeDelta;
        }

		finalVertices.AddRange (newVertices);

        int newSeg = _segment + 1;

        for (int j = 0; j < _edgeSegment; ++j)
        {
            for (int i = 0; i < _segment; ++i)
            {
                CreateTriangle(newTriangles, i + 1 + newSeg * j, i + newSeg * j, i + 1 + newSeg * (j + 1), i + newSeg * (j + 1), offset);
            }
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
	

    //The functions are not called constantly like they are in play mode.
    //- Update is only called when something in the scene changed.
    //- OnGUI is called when the Game View recieves an Event.
    //- OnRenderObject and the other rendering callback functions are called on every repaint of the Scene View or Game View.
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
