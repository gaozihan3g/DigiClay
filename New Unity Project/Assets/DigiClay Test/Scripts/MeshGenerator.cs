using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour {

    [SerializeField]
	[Range(DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS)]
	float m_radius = 1;

    [SerializeField]
	[Range(DigiClayConstant.MIN_HEIGHT, DigiClayConstant.MAX_HEIGHT)]
	float m_height = 1;

    [SerializeField]
    [Range(3, 60)]
	int m_segment = 8;

    [SerializeField]
    [Range(1, 100)]
	int m_verticalSegment = 10;

    [SerializeField]
    [Range(1, 10)]
	int m_edgeSegment = 5;

    [SerializeField]
    [Range(0.001f, 0.2f)]
	float m_thickness = 0.01f;

	[SerializeField]
	[Range(0.001f, 0.2f)]
    float m_noiseScale = 0.02f;
	[SerializeField]
	[Range(0.001f, 30f)]
    float m_noiseSpan = 10f;

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

	[SerializeField]
	ClayMeshContext prefab;

	Perlin _perlin = new Perlin();
	Vector3 _innerOrigin;
	float _innerRadius;
	float _delta;
	float _heightDelta;

	public void CreateMesh()
	{
		//cleanup first
		for (int i = 0; i < transform.childCount; ++i)
			DestroyImmediate (transform.GetChild (i).gameObject);

		ClayMesh clayMesh = ClayMeshFactory ();

		ClayMeshContext cmc = GameObject.Instantiate<ClayMeshContext> (prefab, transform);
		cmc.gameObject.name = "Clay Mesh " + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
		cmc.clayMesh = clayMesh;

		Debug.Log ("ClayMesh Created.");
	}


	ClayMesh ClayMeshFactory()
	{
		ClayMesh cMesh = new ClayMesh ();

		Mesh mesh = new Mesh();
		mesh.name = "Generated Mesh";

		List<Vector3> finalVertices = new List<Vector3>();
		List<int> outerTriangles = new List<int>();
		List<int> innerTriangles = new List<int>();
		List<int> edgeTriangles = new List<int>();
		List<Vector2> finalUVs = new List<Vector2> ();
        List<Vector2Int> uvSeams = new List<Vector2Int>();
		int offset = 0;

		_delta = 2f * Mathf.PI / (float)m_segment;
		_heightDelta = (float)m_height / (float)m_verticalSegment;

        _innerOrigin = new Vector3(0f, Mathf.Min(m_thickness, 0.5f * _heightDelta), 0f);
        _innerRadius = Mathf.Max(0, m_radius - m_thickness);

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

		cMesh.mesh = mesh;
		cMesh.uvSeams = uvSeams;
		cMesh.RecalculateNormals();
		return cMesh;
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


		for (int i = 0; i < m_segment; ++i)
		{
            var pos = new Vector3(m_radius * Mathf.Cos(theta), 0, m_radius * Mathf.Sin(theta));

            var noiseCoordinate = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * m_noiseSpan;

            var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                     * _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                     * m_noiseScale;

            var finalPos = pos + noise;

            newVertices.Add(finalPos);
			newUVs.Add (Vector2.one);
			theta += _delta;
		}

		finalVertices.AddRange (newVertices);

		//add triangles
		for (int i = 1; i <= m_segment; ++i)
		{
			CreateTriangle(newTriangles, 0, i, i % m_segment + 1);
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
//		newUVs.Add (new Vector2(.5f, 1f));
		newUVs.Add (Vector2.zero);

		theta = 0;
		for (int i = 0; i < m_segment; ++i)
		{

            var pos = new Vector3(_innerRadius * Mathf.Cos(theta), heightTheta + _innerOrigin.y, _innerRadius * Mathf.Sin(theta));

            var noiseCoordinate = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * m_noiseSpan;

            var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                     * _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                     * m_noiseScale;

            var finalPos = pos + noise;

            newVertices.Add(finalPos);

			//newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + innerOrigin.y, innerRadius * Mathf.Sin(theta)));

//			newUVs.Add (new Vector2( 1f / (float)m_segment * i, 0f));
			newUVs.Add(Vector2.one);
			theta += _delta;
		}

		finalVertices.AddRange (newVertices);

		//add triangles
		for (int i = 1; i <= m_segment; ++i)
		{
//			CreateTriangle(newTriangles, 0, i + 1, i, offset);
			CreateTriangle(newTriangles, 0, i % m_segment + 1, i, offset);
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
		for (int j = 0; j < m_verticalSegment + 1; ++j)
		{
			theta = 0f;

			for (int i = 0; i < m_segment + 1; ++i)
			{
                var pos = new Vector3(m_radius * Mathf.Cos(theta), heightTheta, m_radius * Mathf.Sin(theta));

                var noiseCoordinate = new Vector3(Mathf.Cos(theta), heightTheta, Mathf.Sin(theta)) * m_noiseSpan;

                var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                         * _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                         * m_noiseScale;

                var finalPos = pos + noise;

                newVertices.Add(finalPos);

				newUVs.Add (new Vector2 ( 1f / (float)m_segment * i, 1f / (float)m_verticalSegment * j));

				theta += _delta;
			}

            // store vertex pairs for normal fix
            int x = 0 + j * (m_segment + 1) + offset;
            int y = m_segment + j * (m_segment + 1) + offset;

            uvSeams.Add(new Vector2Int(x, y));

			heightTheta += _heightDelta;
		}

		finalVertices.AddRange (newVertices);

		int newSeg = m_segment + 1;

		for (int j = 0; j < m_verticalSegment; ++j)
		{
			for (int i = 0; i < m_segment; ++i)
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

		for (int j = 0; j < m_verticalSegment + 1; ++j)
		{
			theta = 0f;

			for (int i = 0; i < m_segment + 1; ++i)
			{

                ///
                var pos = new Vector3(_innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? _innerOrigin.y : 0), _innerRadius * Mathf.Sin(theta));

                var noiseCoordinate = new Vector3(Mathf.Cos(theta), heightTheta, Mathf.Sin(theta)) * m_noiseSpan;

                var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                         * _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                         * m_noiseScale;

                var finalPos = pos + noise;

                newVertices.Add(finalPos);

				//newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? innerOrigin.y : 0), innerRadius * Mathf.Sin(theta)));
                newUVs.Add(new Vector2(1f / (float)m_segment * i, 1f / (float)m_verticalSegment * j));
				//newUVs.Add (Vector2.zero);
				theta += _delta;
			}

            // store vertex pairs for normal fix
            int x = 0 + j * (m_segment + 1) + offset;
            int y = m_segment + j * (m_segment + 1) + offset;

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
        int newSeg = m_segment + 1;

        for (int j = 0; j < m_verticalSegment; ++j)
        {
            for (int i = 0; i < m_segment; ++i)
            {
                CreateTriangle(newTriangles, i + 1 + newSeg * j, i + newSeg * j, i + 1 + newSeg * (j + 1), i + newSeg * (j + 1), offset);
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
        float edgeTheta = 0f;
        float edgeDelta = (m_radius - _innerRadius) / m_edgeSegment;
        float radius;

        for (int j = 0; j < m_edgeSegment + 1; ++j)
        {
            theta = 0f;
            radius = _innerRadius + edgeTheta;

            float x = (float)j / (float)m_edgeSegment;

			// height curve:
			// y = 2 * Mathf.Sqrt(x * (1 - x));
            float y = 2 * Mathf.Sqrt(x * (1 - x));

            float edgeSegHeight = y * _heightDelta;


            for (int i = 0; i < m_segment + 1; ++i)
            {
                var pos = new Vector3(radius * Mathf.Cos(theta), m_height + edgeSegHeight, radius * Mathf.Sin(theta));
                var noiseCoordinate = new Vector3(Mathf.Cos(theta), m_height, Mathf.Sin(theta)) * m_noiseSpan;
                var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                         * _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                         * m_noiseScale;
                var finalPos = pos + noise;
                newVertices.Add(finalPos);
                newUVs.Add(Vector2.zero);
                theta += _delta;
            }
            edgeTheta += edgeDelta;
        }

		finalVertices.AddRange (newVertices);

        int newSeg = m_segment + 1;

        for (int j = 0; j < m_edgeSegment; ++j)
        {
            for (int i = 0; i < m_segment; ++i)
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
}
