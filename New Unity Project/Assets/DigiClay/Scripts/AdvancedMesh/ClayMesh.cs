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

		[SerializeField]
		Mesh m_mesh;

        [SerializeField]
        List<float> m_radiusList = new List<float>();

		[SerializeField]
		int m_row;

        [SerializeField]
        int m_column;

        [SerializeField]
        float m_thickness;

        [SerializeField, HideInInspector]
        List<bool> m_isFeaturePoints = new List<bool>();

        [SerializeField]
        float[] m_rowAvgRadius;

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

        public List<float> RadiusList
        {
            get
            {
                return m_radiusList;
            }

            set
            {
                m_radiusList = value;
            }
        }

        public float[] RowAvgRadius
        {
            get
            {
                return m_rowAvgRadius;
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
                m_thickness = value;
            }
        }

        public ClayMesh(int row, int column)
		{
            m_row = row;
            m_column = column;
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
                    avgRadius += m_radiusList[i * Column + j];
                }
                avgRadius /= m_column;
                m_rowAvgRadius[i] = avgRadius;
            }
        }

        public float GetRowAvgRadiusForVertex(int i)
        {
            if (i > m_radiusList.Count)
                throw new IndexOutOfRangeException();
            return m_rowAvgRadius[i / m_column];
        }
    }
}
