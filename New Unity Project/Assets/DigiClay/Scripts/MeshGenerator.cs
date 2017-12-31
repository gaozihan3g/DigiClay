using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour {

    [SerializeField, Range(DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS)]
	float m_radius = 1;

    [SerializeField, Range(DigiClayConstant.MIN_HEIGHT, DigiClayConstant.MAX_HEIGHT)]
	float m_height = 1;

    [SerializeField, Range(3, 60)]
	int m_segment = 8;

    [SerializeField, Range(1, 100)]
	int m_verticalSegment = 10;

    [SerializeField, Range(0.001f, 0.2f)]
	float m_thickness = 0.01f;

    // noise parameters
    [SerializeField, Range(0.01f, 1f)]
    float m_topRadiusRatio = 0.5f;

	[SerializeField, Range(0.01f, 0.1f)]
    float m_centerDistNoiseScale = 0.02f;

	[SerializeField, Range(0.1f, 20f)]
    float m_centerDistNoiseSpan = 0.02f;

	// angle scale is fixed: 0 - 2 * Pi
	[SerializeField, Range(0.1f, 20f)]
    float m_centerAngleNoiseSpan = 0.02f;

	[SerializeField, Range(0.01f, 0.2f)]
	float m_radiusNoiseScale = 0.02f;

	[SerializeField, Range(0.01f, 10f)]
	float m_radiusNoiseSpan = 0.02f;

	[SerializeField, Range(0.01f, 0.2f)]
	float m_individualNoiseScale = 0.02f;

	[SerializeField, Range(0.01f, 20f)]
	float m_individualNoiseSpan = 10f;

	//

    [SerializeField]
    bool m_outerSide = true;
    [SerializeField]
    bool m_innerSide = true;
    [SerializeField]
    bool m_edge = true;
	[SerializeField]
    bool m_outerBottom = true;
	[SerializeField]
    bool m_innerBottom = true;

	[SerializeField]
    ClayMeshContext m_prefab;

    // helpers
    Perlin m_perlin = new Perlin();
    Vector3 m_innerOrigin;
    float m_innerRadius;
    float m_delta;
    float m_heightDelta;
    List<Vector3> m_finalVertices;
    List<int> m_outerTriangles;
    List<int> m_innerTriangles;
    List<int> m_edgeTriangles;
    List<Vector2> m_finalUVs;
    List<Vector2Int> m_uvSeams;
    int m_offset;
    List<bool> m_featurePoints;
    int[,] m_meshGrid;
    List<Vector3> m_normals;
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
		m_featurePoints = new List<bool> ();
        m_normals = new List<Vector3>();

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

        if (m_outerSide)
            GenerateOuterSide();
        if (m_innerSide)
            GenerateInnerSide();
        if (m_edge)
            GenerateEdge();
		if (m_outerBottom)
			GenerateOuterBottom ();
		if (m_innerBottom)
			GenerateInnerBottom ();


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

		cMesh.mesh = mesh;
        cMesh.MeshGrid = m_meshGrid;
//		cMesh.uvSeams = m_uvSeams;
		cMesh.IsFeaturePoints = m_featurePoints;
		cMesh.RecalculateNormals();
		return cMesh;
	}


    void GenerateOuterSide()
	{
		List<Vector3> newVertices = new List<Vector3>();
		List<int> newTriangles = new List<int>();
		List<Vector2> newUVs = new List<Vector2> ();
        List<bool> newFeaturePoints = new List<bool>();

		float theta = 0f;
		float heightTheta = 0f;
        Vector3 origin = Vector3.zero;
        int index = 0;

        //Mesh Grid
        // _segment * (_verticalSegment + 1)
        m_meshGrid = new int[m_verticalSegment + 1, m_segment];

		for (int j = 0; j < m_verticalSegment + 1; ++j)
		{
			theta = 0f;

			//random the center
			// 1. direction
			float offsetAngle = Mathf.Lerp (0f, 2 * Mathf.PI, m_perlin.Noise (heightTheta * m_centerAngleNoiseSpan));
			Vector3 offsetDir = new Vector3 (Mathf.Cos (offsetAngle), 0f, Mathf.Sin (offsetAngle)).normalized;

			// 2. length
			float offsetLength = m_perlin.Noise (heightTheta * m_centerDistNoiseSpan) * m_centerDistNoiseScale;
			Vector3 noiseCenter = offsetDir * offsetLength;

			// store the origin, in order to fix the total offset
			if (j == 0)
				origin = noiseCenter;

            // model the shape as a ellipse, get radius based on height
            // x^2 / a^2 + y^2 / b^2 = 1
            float baseRadius = Mathf.Max( m_topRadiusRatio * m_radius, Mathf.Sqrt ( Mathf.Max(0f, m_height * m_height - heightTheta * heightTheta) ) * m_radius / m_height);

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

				var finalPos = pos + noiseCenter - origin;

                newVertices.Add(finalPos);

                // create index for grid
                // TODO can it be simplified ?
                m_meshGrid[j, i] = index++;


                // add feature points for smoothing
                newFeaturePoints.Add ((j == 0 || j == 1 || j == m_verticalSegment - 1 || j == m_verticalSegment) ? true : false);

                // create normal, just for inner points
                Vector3 normal = new Vector3(finalPos.x, 0f, finalPos.z).normalized; 
                m_normals.Add(normal);

                // create uv, symmetric
                // TODO seamless 0 - 1
				float u;

				if (i < m_segment / 2)
					u = 2f * (float)i / (float)m_segment;
				else
					u = -2f * (float)i / (float)m_segment + 2f;

				newUVs.Add (new Vector2 ( u, 1f / (float)m_verticalSegment * j));

				theta += m_delta;
			}

			heightTheta += m_heightDelta;
		}

        //create triangles
		//seamless
		for (int j = 0; j < m_verticalSegment; ++j)
		{
			for (int i = 0; i < m_segment; ++i)
			{
				CreateTriangle (newTriangles,
                                i + m_segment * j,
                                (i + 1) % m_segment + m_segment * j,
                                i + m_segment * (j + 1),
                                (i + 1) % m_segment + m_segment * (j + 1),
                                m_offset);
			}
		}

        m_finalVertices.AddRange(newVertices);
        m_outerTriangles.AddRange (newTriangles);
		m_finalUVs.AddRange (newUVs);
        m_featurePoints.AddRange(newFeaturePoints);
		m_offset = m_finalVertices.Count;

	}

    void GenerateInnerSide()
	{
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Vector2> newUVs = new List<Vector2>();
        List<bool> newFeaturePoints = new List<bool>();

        // based on outer side
        for (int i = 0; i < m_finalVertices.Count; i++)
        {
            Vector3 innerVertex = m_finalVertices[i] - m_normals[i] * m_thickness;
            newVertices.Add(innerVertex);
            newUVs.Add(m_finalUVs[i]);
        }

        //create triangles
        //seamless
        for (int j = 0; j < m_verticalSegment; ++j)
        {
            for (int i = 0; i < m_segment; ++i)
            {
                CreateTriangle(newTriangles,
                               (i + 1) % m_segment + m_segment * j,
                                i + m_segment * j,
                               (i + 1) % m_segment + m_segment * (j + 1),
                                i + m_segment * (j + 1),
                               m_meshGrid.Length);
            }
        }

        m_finalVertices.AddRange(newVertices);
        m_innerTriangles.AddRange(newTriangles);
        m_finalUVs.AddRange(newUVs);
        m_featurePoints.AddRange(newFeaturePoints);
        m_offset = m_finalVertices.Count;
	}

    /// <summary>
    /// This is especial since it only create new triangles. NO new verts are added.
    /// </summary>
	void GenerateEdge()
	{
		List<int> newTriangles = new List<int>();
        List<int> outerEdgeIndex = new List<int>();
        List<int> innerEdgeIndex = new List<int>();

        // outer & inner edge vertices
        for (int i = m_verticalSegment * m_segment; i < (m_verticalSegment + 1) * m_segment; ++i)
        {
            outerEdgeIndex.Add(i);
            innerEdgeIndex.Add(i + (m_verticalSegment + 1) * m_segment);
        }

        //create triangles
        //seamless
        for (int i = 0; i < m_segment; ++i)
        {
            CreateTriangle(newTriangles,
                           outerEdgeIndex[i],
                           outerEdgeIndex[(i + 1) % m_segment],
                           innerEdgeIndex[i],
                           innerEdgeIndex[(i + 1) % m_segment],
                           0);
        }

        m_outerTriangles.AddRange(newTriangles);

	}

    void GenerateOuterBottom()
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Vector2> newUVs = new List<Vector2>();
        List<bool> newFeaturePoints = new List<bool>();

        // origin
        newVertices.Add(Vector3.zero);
        newUVs.Add(Vector2.zero);
        newFeaturePoints.Add(true);


        for (int i = 0; i < m_segment; ++i)
        {
            newVertices.Add(m_finalVertices[i]);
            newUVs.Add(Vector2.one);
            newFeaturePoints.Add(true);
        }

        //add triangles
        for (int i = 1; i < m_segment + 1; ++i)
        {
            CreateTriangle(newTriangles, 0, i, i % m_segment + 1, m_offset);
        }

        m_finalVertices.AddRange(newVertices);
        m_outerTriangles.AddRange(newTriangles);
        m_finalUVs.AddRange(newUVs);
        m_featurePoints.AddRange(newFeaturePoints);
        m_offset = m_finalVertices.Count;
    }

    void GenerateInnerBottom()
    {
        List<Vector3> newVertices = new List<Vector3>();
        List<int> newTriangles = new List<int>();
        List<Vector2> newUVs = new List<Vector2>();
        List<bool> newFeaturePoints = new List<bool>();

        // origin
        //newVertices.Add(new Vector3(0f, m_thickness, 0f));
        newVertices.Add(Vector3.zero);
        newUVs.Add(Vector2.zero);
        newFeaturePoints.Add(true);


        for (int i = 0; i < m_segment; ++i)
        {
            newVertices.Add(m_finalVertices[i + m_meshGrid.Length]);
            newUVs.Add(Vector2.one);
            newFeaturePoints.Add(true);
        }

        //add triangles
        for (int i = 1; i < m_segment + 1; ++i)
        {
            CreateTriangle(newTriangles, 0, i % m_segment + 1, i, m_offset);
        }

        m_finalVertices.AddRange(newVertices);
        m_innerTriangles.AddRange(newTriangles);
        m_finalUVs.AddRange(newUVs);
        m_featurePoints.AddRange(newFeaturePoints);
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



// store vertex pairs for normal fix
//            int x = 0 + j * (m_segment + 1) + m_offset;
//            int y = m_segment + j * (m_segment + 1) + m_offset;

//            m_uvSeams.Add(new Vector2Int(x, y));


//      int newSeg = m_segment + 1;
//
//      for (int j = 0; j < m_verticalSegment; ++j)
//      {
//          for (int i = 0; i < m_segment; ++i)
//          {
//              CreateTriangle (newTriangles, i + newSeg * j, i + 1 + newSeg * j, i + newSeg * (j + 1), i + 1 + newSeg * (j + 1), m_offset);
//          }
//      }

//void GenerateInnerSide()
//{
//    List<Vector3> newVertices = new List<Vector3>();
//    List<int> newTriangles = new List<int>();
//    List<Vector2> newUVs = new List<Vector2>();
//    float theta = 0f;
//    float heightTheta = 0f;

//    for (int j = 0; j < m_verticalSegment + 1; ++j)
//    {
//        theta = 0f;

//        for (int i = 0; i < m_segment; ++i)
//        {

//            ///
//            var pos = new Vector3(m_innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? m_innerOrigin.y : 0), m_innerRadius * Mathf.Sin(theta));

//            Vector3 noise = Vector3.zero;

//            if (!(j == 0 || j == m_verticalSegment))
//            {
//                var noiseCoordinate = new Vector3(Mathf.Cos(theta), heightTheta, Mathf.Sin(theta)) * m_individualNoiseSpan;

//                noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
//                                                         * m_perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
//                                                         * m_individualNoiseScale;
//            }

//            var finalPos = pos + noise;

//            newVertices.Add(finalPos);
//            m_isFeaturePointList.Add((j == 0 || j == m_verticalSegment) ? true : false);

//            //newVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? innerOrigin.y : 0), innerRadius * Mathf.Sin(theta)));
//            newUVs.Add(new Vector2(1f / (float)m_segment * i, 1f / (float)m_verticalSegment * j));
//            //newUVs.Add (Vector2.zero);
//            theta += m_delta;
//        }

//        // store vertex pairs for normal fix
//        //            int x = 0 + j * (m_segment + 1) + m_offset;
//        //            int y = m_segment + j * (m_segment + 1) + m_offset;
//        //
//        //            m_uvSeams.Add(new Vector2Int(x, y));

//        heightTheta += m_heightDelta;
//    }

//    m_finalVertices.AddRange(newVertices);

//    //seamless
//    for (int j = 0; j < m_verticalSegment; ++j)
//    {
//        for (int i = 0; i < m_segment; ++i)
//        {
//            CreateTriangle(newTriangles, (i + 1) % m_segment + m_segment * j, i + m_segment * j, (i + 1) % m_segment + m_segment * (j + 1), i + m_segment * (j + 1), m_offset);
//        }
//    }


//    //        int newSeg = m_segment + 1;
//    //
//    //        for (int j = 0; j < m_verticalSegment; ++j)
//    //        {
//    //            for (int i = 0; i < m_segment; ++i)
//    //            {
//    //                CreateTriangle(newTriangles, i + 1 + newSeg * j, i + newSeg * j, i + 1 + newSeg * (j + 1), i + newSeg * (j + 1), m_offset);
//    //            }
//    //        }

//    m_innerTriangles.AddRange(newTriangles);

//    m_finalUVs.AddRange(newUVs);

//    m_offset = m_finalVertices.Count;
//}

// a list of inner edge vertices
//      for (int j = 0; j < m_edgeSegment + 1; ++j)
//      {
//          theta = 0f;
//          radius = m_innerRadius + edgeTheta;

//          float x = (float)j / (float)m_edgeSegment;

//  // height curve:
//  // y = 2 * Mathf.Sqrt(x * (1 - x));
//          float y = 2 * Mathf.Sqrt(x * (1 - x));

//          float edgeSegHeight = y * m_heightDelta;


//          for (int i = 0; i < m_segment + 1; ++i)
//          {
//              var pos = new Vector3(radius * Mathf.Cos(theta), m_height + edgeSegHeight, radius * Mathf.Sin(theta));

//              //var noiseCoordinate = new Vector3(Mathf.Cos(theta), m_height, Mathf.Sin(theta)) * m_noiseSpan;
//              //var noise = new Vector3(Mathf.Cos(theta), 0f, Mathf.Sin(theta)).normalized
//                                                       //* _perlin.Noise(noiseCoordinate.x, noiseCoordinate.y, noiseCoordinate.z)
//                                                       //* m_noiseScale;

//              var finalPos = pos;
//              newVertices.Add(finalPos);
//              m_isFeaturePointList.Add (true);
//              newUVs.Add(Vector2.zero);
//              theta += m_delta;
//          }
//          edgeTheta += edgeDelta;
//      }
//m_finalVertices.AddRange (newVertices);
// a list of inner edge vertices
        //      for (int j = 0; j < m_edgeSegment + 1; ++j)
        //      {
        //          theta = 0f;
        //          radius = m_innerRadius + edgeTheta;

        //          float x = (float)j / (float)m_edgeSegment;

        //  // height curve:
        //  // y = 2 * Mathf.Sqrt(x * (1 - x));
        //          float y = 2 * Mathf.Sqrt(x * (1 - x));

        //          float edgeSegHeight = y * m_heightDelta;



        //string output = "";
        //foreach(var a in outerEdgeIndex)
        //{
        //    output += a + " ";
        //}
        //Debug.Log(output);
        //output = "";
        //foreach (var a in innerEdgeIndex)
        //{
        //    output += a + " ";
        //}
        //Debug.Log(output);

// a list of inner edge vertices
//      for (int j = 0; j < m_edgeSegment + 1; ++j)
//      {
//          theta = 0f;
//          radius = m_innerRadius + edgeTheta;

//          float x = (float)j / (float)m_edgeSegment;

//  // height curve:
//  // y = 2 * Mathf.Sqrt(x * (1 - x));
//          float y = 2 * Mathf.Sqrt(x * (1 - x));

//          float edgeSegHeight = y * m_heightDelta;



//string output = "";
//foreach(var a in outerEdgeIndex)
//{
//    output += a + " ";
//}
//Debug.Log(output);
//output = "";
//foreach (var a in innerEdgeIndex)
//{
//    output += a + " ";
//}
//Debug.Log(output);

//   [SerializeField]
//   [Range(1, 10)]
//int m_edgeSegment = 5;


//      int newSeg = m_segment + 1;

//      for (int j = 0; j < m_edgeSegment; ++j)
//      {
//          for (int i = 0; i < m_segment; ++i)
//          {
//              CreateTriangle(newTriangles, i + 1 + newSeg * j, i + newSeg * j, i + 1 + newSeg * (j + 1), i + newSeg * (j + 1), m_offset);
//          }
//      }

//      m_outerTriangles.AddRange (newTriangles);

//m_finalUVs.AddRange (newUVs);

//m_offset = m_finalVertices.Count;