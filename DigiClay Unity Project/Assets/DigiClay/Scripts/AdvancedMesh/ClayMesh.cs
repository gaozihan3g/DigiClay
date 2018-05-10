using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigiClay
{
    //ClayMesh:
    //[0 , Row * Column - 1] - Outer
    //[Row * Column, 2 * Row * Column - 1] - Inner
    //[2 * Row * Column] - Outer Bottom Center
    //[2 * Row * Column + 1, 2 * Row * Column + Column] - Outer Bottom Edge

    [Serializable]
    public class ClayMesh
    {
        public enum VertexType
        {
            OuterSide,
            InnerSide,
            OuterBottomCenter,
            OuterBottomEdge,
            InnerBottomCenter,
            InnerBottomEdge
        }

        [SerializeField]
        int m_row;
        [SerializeField]
        int m_column;
        [SerializeField]
        float m_thickness;
		// thickness matrix
		[SerializeField]
		Matrix4x4 m_thicknessMatrix;

        [SerializeField]
        float m_height;
		[SerializeField]
		List<float> m_radiusList = new List<float>();
        [SerializeField]
        float[] m_rowAvgRadiusList;

		Dictionary<int, HashSet<int>> m_connectionNetwork;

        [SerializeField]
        Mesh m_mesh;
		[SerializeField]
        float m_angleDelta;
		[SerializeField]
        float m_heightDelta;

        List<Vector3> m_finalVertices;
        List<int> m_outerTriangles;
        List<int> m_innerTriangles;
        List<int> m_edgeTriangles;
        List<Vector2> m_finalUVs;
        //List<Vector2Int> m_uvSeams;
        int m_vertexIndexOffset;

        int Row
        {
            get
            {
                return m_row;
            }
        }

        int Column
        {
            get
            {
                return m_column;
            }
        }

		float[] RowAvgRadiusList
		{
			get
			{
				return m_rowAvgRadiusList;
			}
		}

		public float Height
		{
			get
			{
				return m_height;
			}

			set
			{
				m_height = Mathf.Clamp(value, DigiClayConstant.MIN_HEIGHT, DigiClayConstant.MAX_HEIGHT);
				m_heightDelta = (float)m_height / (float)(Row - 1);
			}
		}

		public float Thickness
		{
			get
			{
				return m_thickness;
			}

			set
			{
				m_thickness = Mathf.Clamp(value, DigiClayConstant.MIN_THICKNESS, DigiClayConstant.MAX_RADIUS);

				m_thicknessMatrix = new Matrix4x4 (
					new Vector4 (1f, 0f, 0f, 0f),
					new Vector4 (0f, 1f, 0f, 0f),
					new Vector4 (0f, 0f, 1f, 0f),
					new Vector4 (0f, 0f, 0f, 1f)
                    );
			}
		}

		public List<float> RadiusList {
			get {
				return m_radiusList;
			}
			set {
				m_radiusList = value;
			}
		}

		public Mesh Mesh
		{
			get
			{
				return m_mesh;
			}
		}

		public ClayMesh(int row, int column, float height, float thickness, List<float> radiusList)
        {
            m_row = row;
            m_column = column;
            Height = height;
            Thickness = thickness;
			RadiusList = radiusList;

            m_rowAvgRadiusList = new float[row];
            m_angleDelta = 2f * Mathf.PI / (float)Column;
        }

		int Get2DRowIndex(int i)
		{
			return i / Column;
		}

		int Get2DColumnIndex(int i)
		{
			return i % Column;
		}

		int Get1DIndex(int i, int j)
		{
			return i * Column + j;
		}

		void BuildConnectionNetwork()
		{
			m_connectionNetwork = new Dictionary<int, HashSet<int>> ();

			for (int k = 0; k < RadiusList.Count; ++k) {
				
				if (IsFeaturePoint(k))
					continue;

				HashSet<int> hs = new HashSet<int> ();

				// get [i,j]
				int i = Get2DRowIndex(k);
				int j = Get2DColumnIndex(k);

				// [i-1, j]
				hs.Add (Get1DIndex ((i-1+Row) % Row, j));
				// [i+1, j]
				hs.Add (Get1DIndex ((i+1) % Row, j));
				// [i, j-1]
				hs.Add (Get1DIndex (i, (j-1+Column) % Column));
				// [i, j+1]
				hs.Add (Get1DIndex (i, (j+1) % Column));
				// [i-1, j-1] ?
//				hs.Add (Get1DIndex ((i-1+Row) % Row, (j-1+Column) % Column));
				// [i+1, j+1] ?
//				hs.Add (Get1DIndex ((i+1) % Row, (j+1) % Column));

				m_connectionNetwork.Add (k, hs);
			}
		}

		void TestConnectionNetwork()
		{
			foreach (var kvp in m_connectionNetwork) {
				string s = kvp.Key.ToString () + " |";
				foreach (var v in kvp.Value) {
					s += " " + v.ToString ();
				}
				Debug.Log (s);
			}
		}

        public void GenerateMesh()
        {
            m_finalVertices = new List<Vector3>();
            m_outerTriangles = new List<int>();
            m_innerTriangles = new List<int>();
            m_edgeTriangles = new List<int>();
            m_finalUVs = new List<Vector2>();
            //m_uvSeams = new List<Vector2Int>();
            m_vertexIndexOffset = 0;

            //mesh
			m_mesh = new Mesh();
			m_mesh.name = "Generated Mesh";
            m_mesh.MarkDynamic();

            GenerateOuterSide();
            GenerateInnerSide();
            GenerateEdge();
            GenerateOuterBottom();
            GenerateInnerBottom();

            m_mesh.vertices = m_finalVertices.ToArray();
            m_mesh.uv = m_finalUVs.ToArray();

            int meshCount = 0;

            if (m_outerTriangles.Count != 0)
                ++meshCount;
            if (m_innerTriangles.Count != 0)
                ++meshCount;
            if (m_edgeTriangles.Count != 0)
                ++meshCount;

            m_mesh.subMeshCount = meshCount;

            int subMeshIndex = 0;

            if (m_outerTriangles.Count != 0)
                m_mesh.SetTriangles(m_outerTriangles.ToArray(), subMeshIndex++);
            if (m_innerTriangles.Count != 0)
                m_mesh.SetTriangles(m_innerTriangles.ToArray(), subMeshIndex++);
            if (m_edgeTriangles.Count != 0)
                m_mesh.SetTriangles(m_edgeTriangles.ToArray(), subMeshIndex++);
            //end mesh

            m_mesh.RecalculateNormals();
        }

        public void UpdateMesh()
        {
            Vector3[] vertices = Mesh.vertices;

            for (int i = 0; i < vertices.Length; ++i)
            {
				if (GetVertexTypeFromIndex (i) == VertexType.OuterSide) {
					//get row column index
					int rowIndex = Get2DRowIndex (i);
					int columnIndex = Get2DColumnIndex (i);
					//get r
					float r = RadiusList [i];
					//get theta
					float angleTheta = m_angleDelta * columnIndex;
					//get heightTheta
					float heightTheta = m_heightDelta * rowIndex;

					vertices [i] = new Vector3 (r * Mathf.Cos (angleTheta), heightTheta, r * Mathf.Sin (angleTheta));
				} else if (GetVertexTypeFromIndex (i) == VertexType.InnerSide) {
                    // inner side
                    // vertices [i] = m_thicknessMatrix.MultiplyPoint3x4 (vertices [i - RadiusList.Count]);

                    int columnIndex = Get2DColumnIndex(i - RadiusList.Count);
                    //get theta
                    float angleTheta = m_angleDelta * columnIndex;

                    float innerRadius = Mathf.Max(0f, (RadiusList[i - RadiusList.Count] - Thickness));

                    vertices[i] = new Vector3(innerRadius * Mathf.Cos(angleTheta),
                                                      vertices[i - RadiusList.Count].y,
                                                      innerRadius * Mathf.Sin(angleTheta));

                } else if (GetVertexTypeFromIndex (i) == VertexType.OuterBottomCenter) {
					// do nothing
				} else if (GetVertexTypeFromIndex (i) == VertexType.OuterBottomEdge) {
					// outer bottom
					// i th of outer side
					vertices [i] = vertices [i - (RadiusList.Count * 2 + 1)];
				} else if (GetVertexTypeFromIndex (i) == VertexType.InnerBottomCenter) {
					// inner bottom
					vertices [i].y = m_heightDelta;
				} else if (GetVertexTypeFromIndex (i) == VertexType.InnerBottomEdge) {
					//InnerBottomEdge
					// i th in the second row of inner side
					vertices [i] = vertices [i - (RadiusList.Count + 2)];
				}
            }

			m_mesh.vertices = vertices;
			m_mesh.RecalculateNormals();
			m_mesh.RecalculateBounds();
        }

        public void RadialSmooth(List<float> weightList)
        {
            RecalculateAvgRadius();

            for (int i = 0; i < RadiusList.Count; ++i)
            {
				if (Mathf.Approximately(weightList[i], 0f))
					continue;
				
                // old
                float oldR = RadiusList[i];
                // target
                float targetR = GetRowAvgRadiusForVertex(i);
                // weight
				float weight = DeformManager.Instance.RadialSmoothingRatio;
                // new
                RadiusList[i] = targetR * weight + oldR * (1f - weight);
            }
        }

		public void LaplacianSmooth(List<float> weightList)
		{
			if (m_connectionNetwork == null)
				BuildConnectionNetwork();
			
			for (int i = 0; i < RadiusList.Count; ++i) {
				
				// early out if feature point
				if (IsFeaturePoint(i))
					continue;
				// early out if weight is 0
				if (Mathf.Approximately (weightList [i], 0f))
					continue;

				// get network
				var adjVectices = m_connectionNetwork[i];

				float sum = 0f;

				foreach (var v in adjVectices) {
					sum += RadiusList [v];
				}

				float weight = DeformManager.Instance.LaplacianSmoothingRatio;

				RadiusList [i] = weight * sum / adjVectices.Count
					+ (1f - weight) * RadiusList [i];
			}
		}

        public void Deform(float sign, float length, float[] orgRadiusList, List<float> weightList)
        {
            for (int i = 0; i < RadiusList.Count; ++i)
            {
                //early out
                if (Mathf.Approximately(weightList[i], 0f))
                    continue;
                //deform
                Deform(i, orgRadiusList[i], sign, length * DeformManager.Instance.DeformRatio, weightList[i]);
            }
        }

        void Deform(int i, float originLength, float sign, float length, float weight)
        {
//			if (i == 10)
//				Debug.Log (string.Format("i:{0} originLength:{1:F3} \t sign:{2} length:{3:F3} weight:{4:F3}", i, originLength, sign, length, weight));
			RadiusList[i] = Mathf.Clamp(originLength + sign * length * weight, DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS);
        }

		public bool IsFeaturePoint(int i)
		{
			int r = Get2DRowIndex(i);
			return (r == 0 || r == Row - 1);
		}

        VertexType GetVertexTypeFromIndex(int i)
        {
            if (i < 0 || i >= Mesh.vertexCount)
                throw new ArgumentException();

            if (i < Row * Column)
                return VertexType.OuterSide;
            else if (i < 2 * Row * Column)
                return VertexType.InnerSide;
            else if (i == 2 * Row * Column)
                return VertexType.OuterBottomCenter;
            else if (i < 2 * Row * Column + Column + 1)
                return VertexType.OuterBottomEdge;
            else if (i == 2 * Row * Column + Column + 1)
                return VertexType.InnerBottomCenter;
            else
                return VertexType.InnerBottomEdge;
        }

        void RecalculateAvgRadius()
        {
            float avgRadius = 0f;

            for (int i = 0; i < Row; ++i)
            {
                avgRadius = 0f;
                for (int j = 0; j < Column; ++j)
                {
					avgRadius += RadiusList[Get1DIndex(i,j)];
                }
                avgRadius /= Column;
                RowAvgRadiusList[i] = avgRadius;
            }
        }

        float GetRowAvgRadiusForVertex(int i)
        {
            if (i > RadiusList.Count)
                throw new IndexOutOfRangeException();
			return RowAvgRadiusList[Get2DRowIndex(i)];
        }

        /// <summary>
        /// Mesh Radius Grid
        /// _segment * (_verticalSegment + 1)
        /// </summary>
        void GenerateOuterSide()
        {
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUVs = new List<Vector2>();

            float angleTheta = 0f;
            float heightTheta = 0f;
            Vector3 origin = Vector3.zero;
            //int index = 0;

			for (int i = 0; i < Row; ++i)
            {
				for (int j = 0; j < Column; ++j)
                {
                    //float r = m_baseRadiusList[j] + m_noiseRadiusMatrix[j * m_column + i];
					float r = RadiusList[Get1DIndex(i,j)];
                    Vector3 p = new Vector3(r * Mathf.Cos(angleTheta), heightTheta, r * Mathf.Sin(angleTheta));

                    newVertices.Add(p);

                    // create uv, symmetric
                    // TODO seamless 0 - 1
                    float u;

                    if (j < Column / 2)
                        u = 2f * (float)j / (float)Column;
                    else
                        u = -2f * (float)j / (float)Column + 2f;

                    newUVs.Add(new Vector2(u, 1f / (float)(Row - 1) * i));

                    angleTheta += m_angleDelta;
                }

                heightTheta += m_heightDelta;
            }

            //create triangles
            //seamless
            for (int j = 0; j < Row - 1; ++j)
            {
                for (int i = 0; i < Column; ++i)
                {
                    CreateTriangle(newTriangles,
                                   i + Column * j,
                                   (i + 1) % Column + Column * j,
                                   i + Column * (j + 1),
                                   (i + 1) % Column + Column * (j + 1),
                                    m_vertexIndexOffset);
                }
            }

            m_finalVertices.AddRange(newVertices);
            m_outerTriangles.AddRange(newTriangles);
            m_finalUVs.AddRange(newUVs);
            m_vertexIndexOffset = m_finalVertices.Count;
        }


        /// <summary>
        /// seg * vSeg
        /// </summary>
        void GenerateInnerSide()
        {
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUVs = new List<Vector2>();

            float angleTheta = 0f;

            // based on outer side
            for (int i = 0; i < m_finalVertices.Count; i++)
            {
                // this makes sure that the inner side do not overlap
                float innerRadius = Mathf.Max(0f, (RadiusList[i] - Thickness));
                Vector3 innerVertex = new Vector3(innerRadius * Mathf.Cos(angleTheta),
                                                  m_finalVertices[i].y, // + (i < m_segment ? m_bottomThickness : 0f),
                                                  innerRadius * Mathf.Sin(angleTheta));
                //if (m_finalVertices[i].sqrMagnitude > m_thickness * m_thickness)
                //innerVertex = m_finalVertices[i] - m_normals[i] * m_thickness;

                angleTheta += m_angleDelta;
                newVertices.Add(innerVertex);
                newUVs.Add(m_finalUVs[i]);
            }

            //create triangles
            //seamless
            for (int j = 1; j < Row - 1; ++j) //
            {
                for (int i = 0; i < Column; ++i)
                {
                    CreateTriangle(newTriangles,
                                   (i + 1) % Column + Column * j,
                                   i + Column * j,
                                   (i + 1) % Column + Column * (j + 1),
                                   i + Column * (j + 1),
                                    m_vertexIndexOffset);
                }
            }

            m_finalVertices.AddRange(newVertices);
            m_innerTriangles.AddRange(newTriangles);
            m_finalUVs.AddRange(newUVs);
            m_vertexIndexOffset = m_finalVertices.Count;
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
            for (int i = (Row - 1) * Column; i < Row * Column; ++i)
            {
                outerEdgeIndex.Add(i);
                innerEdgeIndex.Add(i + Row * Column);
            }

            //create triangles
            //seamless
            for (int i = 0; i < Column; ++i)
            {
                CreateTriangle(newTriangles,
                               outerEdgeIndex[i],
                               outerEdgeIndex[(i + 1) % Column],
                               innerEdgeIndex[i],
                               innerEdgeIndex[(i + 1) % Column],
                               0);
            }

            m_outerTriangles.AddRange(newTriangles);
        }

        /// <summary>
        /// 1 + segment
        /// </summary>
        void GenerateOuterBottom()
        {
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUVs = new List<Vector2>();

            // Outer Bottom Center
            newVertices.Add(Vector3.zero);
            newUVs.Add(Vector2.zero);

            for (int i = 0; i < Column; ++i)
            {
                newVertices.Add(m_finalVertices[i]);
                newUVs.Add(Vector2.one);
            }

            //add triangles
            for (int i = 1; i < Column + 1; ++i)
            {
                CreateTriangle(newTriangles, 0, i, i % Column + 1, m_vertexIndexOffset);
            }

            m_finalVertices.AddRange(newVertices);
            m_outerTriangles.AddRange(newTriangles);
            m_finalUVs.AddRange(newUVs);
            m_vertexIndexOffset = m_finalVertices.Count;
        }

        /// <summary>
        /// 1 + segment
        /// </summary>
        void GenerateInnerBottom()
        {
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUVs = new List<Vector2>();

            // Inner Bottom Center
			newVertices.Add(new Vector3(0f, m_heightDelta, 0f));
            newUVs.Add(Vector2.zero);

            for (int i = 0; i < Column; ++i)
            {
                newVertices.Add(m_finalVertices[i + RadiusList.Count + Column]); // add second bottom row verts
                newUVs.Add(Vector2.one);
            }

            //add triangles
            for (int i = 1; i < Column + 1; ++i)
            {
                CreateTriangle(newTriangles, 0, i % Column + 1, i, m_vertexIndexOffset);
            }

            m_finalVertices.AddRange(newVertices);
            m_innerTriangles.AddRange(newTriangles);
            m_finalUVs.AddRange(newUVs);
            m_vertexIndexOffset = m_finalVertices.Count;
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
            CreateTriangle(list, a, c, b, offset);
            CreateTriangle(list, b, c, d, offset);
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
}
