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
        float m_thicknessRatio;
        [SerializeField]
        float m_height;
        [SerializeField]
        List<float> m_radiusMatrix = new List<float>();
        [SerializeField]
        float[] m_rowAvgRadiusList;
        [SerializeField, HideInInspector]
        List<bool> m_isFeaturePoints = new List<bool>();
        [SerializeField]
        Mesh m_mesh;

        float m_angleDelta;
        float m_heightDelta;
        List<Vector3> m_finalVertices;
        List<int> m_outerTriangles;
        List<int> m_innerTriangles;
        List<int> m_edgeTriangles;
        List<Vector2> m_finalUVs;
        //List<Vector2Int> m_uvSeams;
        int m_vertexIndexOffset;

        //TODO need this?
        public float m_radialSmoothingRatio = 0.5f;

        public int Row
        {
            get
            {
                return m_row;
            }
        }

        public int Column
        {
            get
            {
                return m_column;
            }
        }


        public Mesh Mesh
        {
            get
            {
                return m_mesh;
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

        public List<float> RadiusMatrix
        {
            get
            {
                return m_radiusMatrix;
            }
        }

        public float[] RowAvgRadiusList
        {
            get
            {
                return m_rowAvgRadiusList;
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
                m_heightDelta = (float)m_height / (float)(Row - 1);
            }
        }

        public ClayMesh(int row, int column, float height, float thickness)
        {
            m_row = row;
            m_column = column;
            Height = height;
            ThicknessRatio = thickness;

            m_rowAvgRadiusList = new float[row];
            m_angleDelta = 2f * Mathf.PI / (float)Column;
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
            m_mesh = new Mesh
            {
                name = "Generated Mesh"
            };

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
        {
            Vector3[] vertices = Mesh.vertices;
            ///
            for (int i = 0; i < vertices.Length; ++i)
            {
                if (GetVertexTypeFromIndex(i) == ClayMesh.VertexType.OuterSide)
                {
                    //get row column index
                    int rowIndex = i / Column;
                    int columnIndex = i % Column;
                    //get r
                    float r = RadiusMatrix[i];
                    //get theta
                    float angleTheta = m_angleDelta * columnIndex;
                    //get heightTheta
                    float heightTheta = m_heightDelta * rowIndex;

                    heightTheta += m_heightDelta;
                    vertices[i] = new Vector3(r * Mathf.Cos(angleTheta), heightTheta, r * Mathf.Sin(angleTheta));
                }
                else if (GetVertexTypeFromIndex(i) == ClayMesh.VertexType.InnerSide)
                {
                    // inner side
                    vertices[i] = vertices[i - RadiusMatrix.Count] * ThicknessRatio;
                }
                else if (GetVertexTypeFromIndex(i) == ClayMesh.VertexType.OuterBottomEdge)
                {
                    // outer bottom
                    vertices[i] = vertices[i - RadiusMatrix.Count * 2];
                }
                else if (GetVertexTypeFromIndex(i) == ClayMesh.VertexType.InnerBottomEdge)
                {
                    // inner bottom
                    vertices[i] = vertices[i - (RadiusMatrix.Count * 2 + 1 + Column)];
                }
            }

            Mesh.vertices = vertices;
        }

        public void RadialSmooth(List<float> weightList)
        {
            RecalculateAvgRadius();

            for (int i = 0; i < RadiusMatrix.Count; ++i)
            {
                // old
                float oldR = RadiusMatrix[i];
                // target
                float targetR = GetRowAvgRadiusForVertex(i);
                // weight
                float weight = weightList[i];
                // new
                RadiusMatrix[i] = targetR * weight + oldR * (1f - weight);
            }
        }

        public void Deform(int i, float originLength, float sign, float length, float weight)
        {
            RadiusMatrix[i] = originLength + sign * length * weight;
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
                    avgRadius += RadiusMatrix[i * Column + j];
                }
                avgRadius /= Column;
                RowAvgRadiusList[i] = avgRadius;
            }
        }

        float GetRowAvgRadiusForVertex(int i)
        {
            if (i > RadiusMatrix.Count)
                throw new IndexOutOfRangeException();
            return RowAvgRadiusList[i / Column];
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

            for (int j = 0; j < Row; ++j)
            {
                for (int i = 0; i < Column; ++i)
                {
                    //float r = m_baseRadiusList[j] + m_noiseRadiusMatrix[j * m_column + i];
                    float r = RadiusMatrix[j * Column + i];
                    Vector3 p = new Vector3(r * Mathf.Cos(angleTheta), heightTheta, r * Mathf.Sin(angleTheta));

                    newVertices.Add(p);

                    // create uv, symmetric
                    // TODO seamless 0 - 1
                    float u;

                    if (i < Column / 2)
                        u = 2f * (float)i / (float)Column;
                    else
                        u = -2f * (float)i / (float)Column + 2f;

                    newUVs.Add(new Vector2(u, 1f / (float)(Row - 1) * j));

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

            // origin
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

            // origin
            newVertices.Add(new Vector3(0f, Height / (Row - 1), 0f));
            newUVs.Add(Vector2.zero);

            for (int i = 0; i < Column; ++i)
            {
                newVertices.Add(m_finalVertices[i + RadiusMatrix.Count + Column]); // add second bottom row verts
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
