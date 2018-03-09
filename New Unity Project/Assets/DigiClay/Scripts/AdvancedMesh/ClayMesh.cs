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
        List<float> m_radiusMatrix = new List<float>();

        [SerializeField, HideInInspector]
        List<bool> m_isFeaturePoints = new List<bool>();

        //deprecated
        [SerializeField]
        float[] m_rowAvgRadius;

        [SerializeField]
        Mesh m_mesh;

        //		[SerializeField]
        //		List<Vector2Int> m_uvSeams = new List<Vector2Int> ();

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

        public List<float> RadiusMatrix
        {
            get
            {
                return m_radiusMatrix;
            }

            set
            {
                m_radiusMatrix = value;
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
                    avgRadius += m_radiusMatrix[i * Column + j];
                }
                avgRadius /= m_column;
                m_rowAvgRadius[i] = avgRadius;
            }
        }

        public float GetRowAvgRadiusForVertex(int i)
        {
            if (i > m_radiusMatrix.Count)
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

        public void CreateMesh()
        { }

        public void UpdateMesh()
        { }

    }
}
