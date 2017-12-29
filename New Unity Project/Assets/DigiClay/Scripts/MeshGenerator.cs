﻿using System.Collections;
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

	//
	//noise parameters
	//

	[SerializeField, Range(0.01f, 0.1f)]
	float m_centerOffsetDistNoiseScale = 0.02f;

	[SerializeField, Range(0.1f, 20f)]
	float m_centerOffsetDistNoiseSpan = 0.02f;

	// angle scale is fixed: 0 - 2 * Pi
	[SerializeField, Range(0.1f, 20f)]
	float m_centerOffsetAngleNoiseSpan = 0.02f;


	[SerializeField, Range(0.01f, 0.2f)]
	float m_radiusNoiseScale = 0.02f;

	[SerializeField, Range(0.01f, 10f)]
	float m_radiusNoiseSpan = 0.02f;


	[SerializeField, Range(0.01f, 0.2f)]
	float m_individualNoiseScale = 0.02f;

	[SerializeField, Range(0.01f, 20f)]
	float m_individualNoiseSpan = 10f;

	//
	//
	//

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
    ClayMeshContext m_prefab;

    Perlin m_perlin = new Perlin();
    Vector3 m_innerOrigin;
	float m_innerRadius;
    float m_delta;
    float m_heightDelta;

    // helpers
    List<Vector3> m_finalVertices;
    List<int> m_outerTriangles;
    List<int> m_innerTriangles;
    List<int> m_edgeTriangles;
    List<Vector2> m_finalUVs;
    List<Vector2Int> m_uvSeams;
    int m_offset;
	List<bool> m_isFeaturePointList;
    //


	public void CreateMesh()
	{
		//cleanup first
		for (int i = 0; i < transform.childCount; ++i)
			DestroyImmediate (transform.GetChild (i).gameObject);

		ClayMesh clayMesh = ClayMeshFactory ();

		ClayMeshContext cmc = Instantiate<ClayMeshContext> (m_prefab, transform);
		cmc.gameObject.name = "Clay Mesh " + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
		cmc.clayMesh = clayMesh;

		Debug.Log ("ClayMesh Created.");
	}


	ClayMesh ClayMeshFactory()
	{
        m_finalVertices = new List<Vector3>();
        m_outerTriangles = new List<int>();
        m_innerTriangles = new List<int>();
        m_edgeTriangles = new List<int>();
        m_finalUVs = new List<Vector2>();
        m_uvSeams = new List<Vector2Int>();
        m_offset = 0;
		m_isFeaturePointList = new List<bool> ();

		ClayMesh cMesh = new ClayMesh ();

        Mesh mesh = new Mesh
        {
            name = "Generated Mesh"
        };

        mesh.MarkDynamic();

        m_delta = 2f * Mathf.PI / (float)m_segment;
        m_heightDelta = (float)m_height / (float)m_verticalSegment;

        m_innerOrigin = new Vector3(0f, Mathf.Min(m_thickness, 0.5f * m_heightDelta), 0f);
        m_innerRadius = Mathf.Max(0, m_radius - m_thickness);

		if (OuterBottom)
			GenerateOuterBottom ();
		if (InnerBottom)
			GenerateInnerBottom ();
		if (OuterSide)
            GenerateOuterSide ();
		if (InnerSide)
            GenerateInnerSide ();
		if (Edge)
            GenerateEdge ();

		mesh.vertices = m_finalVertices.ToArray();
		mesh.uv = m_finalUVs.ToArray ();

		int meshCount = 0;

		if (m_outerTriangles.Count != 0)
			++meshCount;
		if (m_innerTriangles.Count != 0)
			++meshCount;
		if (m_edgeTriangles.Count != 0)
			++meshCount;

		mesh.subMeshCount = meshCount;

		int subMeshIndex = 0;
		//set outer submesh

		if (m_outerTriangles.Count != 0)
			mesh.SetTriangles(m_outerTriangles.ToArray(), subMeshIndex++);
		if (m_innerTriangles.Count != 0)
			mesh.SetTriangles(m_innerTriangles.ToArray(), subMeshIndex++);
		if (m_edgeTriangles.Count != 0)
			mesh.SetTriangles(m_edgeTriangles.ToArray(), subMeshIndex++);

//        List<float> fp = new List<float>();
//        for (int i = 0; i < m_verticalSegment + 1; ++i)
//            fp.Add(m_radius);


		cMesh.mesh = mesh;
//		cMesh.uvSeams = m_uvSeams;
//        cMesh.Properties.Add("SpecialPoint", fp);
		cMesh.IsFeaturePoints = m_isFeaturePointList;
		cMesh.RecalculateNormals();
		return cMesh;
	}

	void GenerateOuterBottom()
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;

		// origin
		newVertices.Add(Vector3.zero);
		newUVs.Add (Vector2.zero);
		m_isFeaturePointList.Add (true);


		for (int i = 0; i < m_segment; ++i)
		{
            var pos = new Vector3(m_radius * Mathf.Cos(theta), 0, m_radius * Mathf.Sin(theta));

            //var noiseCoordinate = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * m_noiseSpan;

            //var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                     //* _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                     //* m_noiseScale;

            var finalPos = pos;

            newVertices.Add(finalPos);
			newUVs.Add (Vector2.one);
			m_isFeaturePointList.Add (true);
			theta += m_delta;
		}

		m_finalVertices.AddRange (newVertices);

		//add triangles
		for (int i = 1; i <= m_segment; ++i)
		{
			CreateTriangle(newTriangles, 0, i, i % m_segment + 1);
		}

        m_outerTriangles.AddRange (newTriangles);

		m_finalUVs.AddRange (newUVs);

		m_offset = m_finalVertices.Count;
	}

	void GenerateInnerBottom()
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;
		float heightTheta = 0f;

		newVertices.Add(m_innerOrigin);
//		newUVs.Add (new Vector2(.5f, 1f));
		newUVs.Add (Vector2.zero);
		m_isFeaturePointList.Add (true);

		theta = 0;
		for (int i = 0; i < m_segment; ++i)
		{

            var pos = new Vector3(m_innerRadius * Mathf.Cos(theta), heightTheta + m_innerOrigin.y, m_innerRadius * Mathf.Sin(theta));

            //var noiseCoordinate = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * m_noiseSpan;

            //var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                     //* _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                     //* m_noiseScale;

            var finalPos = pos;

            newVertices.Add(finalPos);

			//newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + innerOrigin.y, innerRadius * Mathf.Sin(theta)));

//			newUVs.Add (new Vector2( 1f / (float)m_segment * i, 0f));
			newUVs.Add(Vector2.one);
			m_isFeaturePointList.Add (true);
			theta += m_delta;
		}

		m_finalVertices.AddRange (newVertices);

		//add triangles
		for (int i = 1; i <= m_segment; ++i)
		{
//			CreateTriangle(newTriangles, 0, i + 1, i, offset);
			CreateTriangle(newTriangles, 0, i % m_segment + 1, i, m_offset);
		}

        m_innerTriangles.AddRange (newTriangles);

		m_finalUVs.AddRange (newUVs);

		m_offset = m_finalVertices.Count;
	}

    void GenerateOuterSide()
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;
		float heightTheta = 0f;

		Vector3 origin = Vector3.zero;

        //Mesh Grid
        // (_segment + 1) * (_verticalSegment + 1)
		for (int j = 0; j < m_verticalSegment + 1; ++j)
		{
			theta = 0f;

			//random the center
			// 1. direction
			float offsetAngle = Mathf.Lerp (0f, 2 * Mathf.PI, m_perlin.Noise (heightTheta * m_centerOffsetAngleNoiseSpan));
			Vector3 offsetDir = new Vector3 (Mathf.Cos (offsetAngle), 0f, Mathf.Sin (offsetAngle)).normalized;

			// 2. length
			float offsetLength = m_perlin.Noise (heightTheta * m_centerOffsetDistNoiseSpan) * m_centerOffsetDistNoiseScale;
			Vector3 noiseCenter = offsetDir * offsetLength;

			// store the origin, in order to fix the total offset
			if (j == 0)
				origin = noiseCenter;

			float baseRadius = Mathf.Max( 0.5f * m_radius, Mathf.Sqrt ( Mathf.Max(0f, m_height * m_height - heightTheta * heightTheta) ) * m_radius / m_height);

			//random the radius
			// 1. length
			float noiseRadius = baseRadius + m_perlin.Noise (heightTheta * m_radiusNoiseSpan) * m_radiusNoiseScale;

			for (int i = 0; i < m_segment; ++i)
			{

				float individualRadius = noiseRadius + m_perlin.Noise (
					Mathf.Cos (theta) * m_individualNoiseSpan,
					heightTheta * m_individualNoiseSpan,
					Mathf.Sin (theta) * m_individualNoiseSpan) * m_individualNoiseScale;


				var pos = new Vector3(individualRadius * Mathf.Cos(theta), heightTheta, individualRadius * Mathf.Sin(theta));


//				var pos = new Vector3(noiseRadius * Mathf.Cos(theta), heightTheta, noiseRadius * Mathf.Sin(theta));
//
//				// random individual vertex
//				Vector3 noiseIndividual = Vector3.zero;
//
//				// j == 0 / m_verticalSegment ?
//
//				var noiseCoordinate = new Vector3(Mathf.Cos(theta), heightTheta, Mathf.Sin(theta)) * m_individualNoiseSpan;
//
//				noiseIndividual = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
//					* m_perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
//					* m_individualNoiseScale;

				var finalPos = pos + noiseCenter - origin;

                newVertices.Add(finalPos);

				m_isFeaturePointList.Add ((j == 0 || j == 1 || j == m_verticalSegment - 1 || j == m_verticalSegment) ? true : false);

				float u;

				if (i < m_segment / 2)
					u = 2f * (float)i / (float)m_segment;
				else
//					u = 2f * (float)i / (float)m_segment - 1f;
					u = -2f * (float)i / (float)m_segment + 2f;


				newUVs.Add (new Vector2 ( u, 1f / (float)m_verticalSegment * j));

				theta += m_delta;
			}

            // store vertex pairs for normal fix
//            int x = 0 + j * (m_segment + 1) + m_offset;
//            int y = m_segment + j * (m_segment + 1) + m_offset;

//            m_uvSeams.Add(new Vector2Int(x, y));

			heightTheta += m_heightDelta;
		}

		m_finalVertices.AddRange (newVertices);


		//seamless
		for (int j = 0; j < m_verticalSegment; ++j)
		{
			for (int i = 0; i < m_segment; ++i)
			{
				CreateTriangle (newTriangles, i + m_segment * j, (i + 1) % m_segment + m_segment * j, i + m_segment * (j + 1), (i + 1) % m_segment + m_segment * (j + 1), m_offset);
			}
		}

//		int newSeg = m_segment + 1;
//
//		for (int j = 0; j < m_verticalSegment; ++j)
//		{
//			for (int i = 0; i < m_segment; ++i)
//			{
//				CreateTriangle (newTriangles, i + newSeg * j, i + 1 + newSeg * j, i + newSeg * (j + 1), i + 1 + newSeg * (j + 1), m_offset);
//			}
//		}

        m_outerTriangles.AddRange (newTriangles);

		m_finalUVs.AddRange (newUVs);

		m_offset = m_finalVertices.Count;

	}

    void GenerateInnerSide()
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;
		float heightTheta = 0f;

		for (int j = 0; j < m_verticalSegment + 1; ++j)
		{
			theta = 0f;

			for (int i = 0; i < m_segment; ++i)
			{

                ///
                var pos = new Vector3(m_innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? m_innerOrigin.y : 0), m_innerRadius * Mathf.Sin(theta));

                Vector3 noise = Vector3.zero;

                if (!(j == 0 || j == m_verticalSegment))
                {
                    var noiseCoordinate = new Vector3(Mathf.Cos(theta), heightTheta, Mathf.Sin(theta)) * m_individualNoiseSpan;

                    noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                             * m_perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                             * m_individualNoiseScale;
                }

                var finalPos = pos + noise;

                newVertices.Add(finalPos);
				m_isFeaturePointList.Add ((j == 0 || j == m_verticalSegment) ? true : false);

				//newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? innerOrigin.y : 0), innerRadius * Mathf.Sin(theta)));
                newUVs.Add(new Vector2(1f / (float)m_segment * i, 1f / (float)m_verticalSegment * j));
				//newUVs.Add (Vector2.zero);
				theta += m_delta;
			}

            // store vertex pairs for normal fix
//            int x = 0 + j * (m_segment + 1) + m_offset;
//            int y = m_segment + j * (m_segment + 1) + m_offset;
//
//            m_uvSeams.Add(new Vector2Int(x, y));

			heightTheta += m_heightDelta;
		}

		m_finalVertices.AddRange (newVertices);

		//seamless
		for (int j = 0; j < m_verticalSegment; ++j)
		{
			for (int i = 0; i < m_segment; ++i)
			{
				CreateTriangle (newTriangles, (i + 1) % m_segment + m_segment * j, i + m_segment * j, (i + 1) % m_segment + m_segment * (j + 1), i + m_segment * (j + 1), m_offset);
			}
		}


//        int newSeg = m_segment + 1;
//
//        for (int j = 0; j < m_verticalSegment; ++j)
//        {
//            for (int i = 0; i < m_segment; ++i)
//            {
//                CreateTriangle(newTriangles, i + 1 + newSeg * j, i + newSeg * j, i + 1 + newSeg * (j + 1), i + newSeg * (j + 1), m_offset);
//            }
//        }

        m_innerTriangles.AddRange (newTriangles);

		m_finalUVs.AddRange (newUVs);

		m_offset = m_finalVertices.Count;
	}

	void GenerateEdge()
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
		float theta = 0f;
        float edgeTheta = 0f;
        float edgeDelta = m_thickness / m_edgeSegment;
        float radius;

        // a list of outer edge vertices

        // a list of inner edge vertices


        for (int j = 0; j < m_edgeSegment + 1; ++j)
        {
            theta = 0f;
            radius = m_innerRadius + edgeTheta;

            float x = (float)j / (float)m_edgeSegment;

			// height curve:
			// y = 2 * Mathf.Sqrt(x * (1 - x));
            float y = 2 * Mathf.Sqrt(x * (1 - x));

            float edgeSegHeight = y * m_heightDelta;


            for (int i = 0; i < m_segment + 1; ++i)
            {
                var pos = new Vector3(radius * Mathf.Cos(theta), m_height + edgeSegHeight, radius * Mathf.Sin(theta));

                //var noiseCoordinate = new Vector3(Mathf.Cos(theta), m_height, Mathf.Sin(theta)) * m_noiseSpan;
                //var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
                                                         //* _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
                                                         //* m_noiseScale;

                var finalPos = pos;
                newVertices.Add(finalPos);
				m_isFeaturePointList.Add (true);
                newUVs.Add(Vector2.zero);
                theta += m_delta;
            }
            edgeTheta += edgeDelta;
        }

		m_finalVertices.AddRange (newVertices);

        int newSeg = m_segment + 1;

        for (int j = 0; j < m_edgeSegment; ++j)
        {
            for (int i = 0; i < m_segment; ++i)
            {
                CreateTriangle(newTriangles, i + 1 + newSeg * j, i + newSeg * j, i + 1 + newSeg * (j + 1), i + newSeg * (j + 1), m_offset);
            }
        }

        m_outerTriangles.AddRange (newTriangles);

		m_finalUVs.AddRange (newUVs);

		m_offset = m_finalVertices.Count;
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