﻿using System;
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
	public class ClayMesh {

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
        float m_thicknessRatio;
        [SerializeField]
        float m_height;
        [SerializeField]
        List<float> m_baseRadiusList = new List<float>();
        [SerializeField]
        List<float> m_noiseRadiusMatrix = new List<float>();

        [SerializeField, HideInInspector]
        List<bool> m_isFeaturePoints = new List<bool>();

        //deprecated
        [SerializeField]
        float[] m_rowAvgRadius;

        [SerializeField]
        Mesh m_mesh;

        //		[SerializeField]
        //		List<Vector2Int> m_uvSeams = new List<Vector2Int> ();

        float m_delta;
        float m_heightDelta;
        List<Vector3> m_finalVertices;
        List<int> m_outerTriangles;
        List<int> m_innerTriangles;
        List<int> m_edgeTriangles;
        List<Vector2> m_finalUVs;
        //List<Vector2Int> m_uvSeams;
        int m_offset;

        public int Row {
			get {
				return m_row;
			}
		}

		public int Column {
			get {
				return m_column;
			}
		}


		public Mesh Mesh {
			get {
				return m_mesh;
			}
			set {
				m_mesh = value;
			}
		}


        public List<bool> IsFeaturePoints
        {
            get
            {
                return m_isFeaturePoints;
            }

            set
            {
                m_isFeaturePoints = value;
            }
        }

        public List<float> NoiseRadiusMatrix
        {
            get
            {
                return m_noiseRadiusMatrix;
            }

            set
            {
                m_noiseRadiusMatrix = value;
            }
        }

        public float[] RowAvgRadius
        {
            get
            {
                return m_rowAvgRadius;
            }
        }

        public float ThicknessRatio
        {
            get
            {
                return m_thicknessRatio;
            }

            set
            {
                m_thicknessRatio = value;
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
                m_height = value;
            }
        }

        public List<float> BaseRadiusList
        {
            get
            {
                return m_baseRadiusList;
            }

            set
            {
                m_baseRadiusList = value;
            }
        }

        public ClayMesh(int row, int column, float height, float thickness)
		{
            m_row = row;
            m_column = column;
            m_height = height;
            m_thicknessRatio = thickness;

            m_rowAvgRadius = new float[row];
        }

		public void RecalculateNormals()
		{
			m_mesh.RecalculateNormals ();
//			m_mesh.FixUVSeam (m_uvSeams.ToArray ());
		}

        public void RecalculateAvgRadius()
        {
            float avgRadius = 0f;

            for (int i = 0; i < Row; ++i)
            {
                avgRadius = 0f;
                for (int j = 0; j < Column; ++j)
                {
                    avgRadius += m_noiseRadiusMatrix[i * Column + j];
                }
                avgRadius /= m_column;
                m_rowAvgRadius[i] = avgRadius;
            }
        }

        public float GetRowAvgRadiusForVertex(int i)
        {
            if (i > m_noiseRadiusMatrix.Count)
                throw new IndexOutOfRangeException();
            return m_rowAvgRadius[i / m_column];
        }

        public VertexType GetVertexTypeFromIndex(int i)
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

        public void GenerateMesh()
        {
            m_finalVertices = new List<Vector3>();
            m_outerTriangles = new List<int>();
            m_innerTriangles = new List<int>();
            m_edgeTriangles = new List<int>();
            m_finalUVs = new List<Vector2>();
            //m_uvSeams = new List<Vector2Int>();
            m_offset = 0;

            //mesh
            m_mesh = new Mesh
            {
                name = "Generated Mesh"
            };

            m_mesh.MarkDynamic();

            m_delta = 2f * Mathf.PI / (float)m_column;
            m_heightDelta = (float)m_height / (float)(m_row - 1);

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
            //set outer submesh

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
        { }

        /// <summary>
        /// Mesh Radius Grid
        /// _segment * (_verticalSegment + 1)
        /// </summary>
        void GenerateOuterSide()
        {
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUVs = new List<Vector2>();

            float theta = 0f;
            float heightTheta = 0f;
            Vector3 origin = Vector3.zero;
            //int index = 0;

            for (int j = 0; j < m_row; ++j)
            {
                for (int i = 0; i < m_column; ++i)
                {
                    float r = m_baseRadiusList[j] + m_noiseRadiusMatrix[j * m_column + i];
                    Vector3 p = new Vector3(r * Mathf.Cos(theta), heightTheta, r * Mathf.Sin(theta));
                    
                    newVertices.Add(p);

                    // create uv, symmetric
                    // TODO seamless 0 - 1
                    float u;

                    if (i < m_column / 2)
                        u = 2f * (float)i / (float)m_column;
                    else
                        u = -2f * (float)i / (float)m_column + 2f;

                    newUVs.Add(new Vector2(u, 1f / (float)(m_row - 1) * j));

                    theta += m_delta;
                }

                heightTheta += m_heightDelta;
            }

            //create triangles
            //seamless
            for (int j = 0; j < m_row - 1; ++j)
            {
                for (int i = 0; i < m_column; ++i)
                {
                    CreateTriangle(newTriangles,
                                    i + m_column * j,
                                    (i + 1) % m_column + m_column * j,
                                    i + m_column * (j + 1),
                                    (i + 1) % m_column + m_column * (j + 1),
                                    m_offset);
                }
            }

            m_finalVertices.AddRange(newVertices);
            m_outerTriangles.AddRange(newTriangles);
            m_finalUVs.AddRange(newUVs);
            m_offset = m_finalVertices.Count;
        }


        /// <summary>
        /// seg * vSeg
        /// </summary>
        void GenerateInnerSide()
        {
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUVs = new List<Vector2>();

            // based on outer side
            for (int i = 0; i < m_finalVertices.Count; i++)
            {
                // this makes sure that the inner side do not overlap
                Vector3 innerVertex = new Vector3(m_finalVertices[i].x * (1 - m_thicknessRatio),
                                                  m_finalVertices[i].y, // + (i < m_segment ? m_bottomThickness : 0f),
                                                  m_finalVertices[i].z * (1 - m_thicknessRatio));
                //if (m_finalVertices[i].sqrMagnitude > m_thickness * m_thickness)
                //innerVertex = m_finalVertices[i] - m_normals[i] * m_thickness;

                newVertices.Add(innerVertex);
                newUVs.Add(m_finalUVs[i]);
            }

            //create triangles
            //seamless
            for (int j = 1; j < m_row - 1; ++j) //
            {
                for (int i = 0; i < m_column; ++i)
                {
                    CreateTriangle(newTriangles,
                                   (i + 1) % m_column + m_column * j,
                                    i + m_column * j,
                                   (i + 1) % m_column + m_column * (j + 1),
                                    i + m_column * (j + 1),
                                    m_offset);
                }
            }

            m_finalVertices.AddRange(newVertices);
            m_innerTriangles.AddRange(newTriangles);
            m_finalUVs.AddRange(newUVs);
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
            for (int i = (m_row - 1) * m_column; i < m_row * m_column; ++i)
            {
                outerEdgeIndex.Add(i);
                innerEdgeIndex.Add(i + m_row * m_column);
            }

            //create triangles
            //seamless
            for (int i = 0; i < m_column; ++i)
            {
                CreateTriangle(newTriangles,
                               outerEdgeIndex[i],
                               outerEdgeIndex[(i + 1) % m_column],
                               innerEdgeIndex[i],
                               innerEdgeIndex[(i + 1) % m_column],
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

            // origin
            newVertices.Add(Vector3.zero);
            newUVs.Add(Vector2.zero);

            for (int i = 0; i < m_column; ++i)
            {
                newVertices.Add(m_finalVertices[i]);
                newUVs.Add(Vector2.one);
            }

            //add triangles
            for (int i = 1; i < m_column + 1; ++i)
            {
                CreateTriangle(newTriangles, 0, i, i % m_column + 1, m_offset);
            }

            m_finalVertices.AddRange(newVertices);
            m_outerTriangles.AddRange(newTriangles);
            m_finalUVs.AddRange(newUVs);
            m_offset = m_finalVertices.Count;
        }

        /// <summary>
        /// 1 + segment
        /// </summary>
        void GenerateInnerBottom()
        {
            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUVs = new List<Vector2>();

            // origin
            newVertices.Add(new Vector3(0f, m_height / (m_row - 1), 0f));
            newUVs.Add(Vector2.zero);

            for (int i = 0; i < m_column; ++i)
            {
                newVertices.Add(m_finalVertices[i + m_noiseRadiusMatrix.Count + m_column]); // add second bottom row verts
                newUVs.Add(Vector2.one);
            }

            //add triangles
            for (int i = 1; i < m_column + 1; ++i)
            {
                CreateTriangle(newTriangles, 0, i % m_column + 1, i, m_offset);
            }

            m_finalVertices.AddRange(newVertices);
            m_innerTriangles.AddRange(newTriangles);
            m_finalUVs.AddRange(newUVs);
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
