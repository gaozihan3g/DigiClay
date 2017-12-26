using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigiClay
{
	[Serializable]
	public class ClayMesh {

		[SerializeField]
		Mesh m_mesh;

		[SerializeField]
		List<Vector2Int> m_uvSeams = new List<Vector2Int> ();

		public Mesh mesh {
			get {
				return m_mesh;
			}
			set {
				m_mesh = value;
			}
		}

		public List<Vector2Int> uvSeams {
			get {
				return m_uvSeams;
			}
			set {
				m_uvSeams = value;
			}
		}

		public ClayMesh()
		{
		}

		public void RecalculateNormals()
		{
			m_mesh.RecalculateNormals ();
			m_mesh.FixUVSeam (m_uvSeams.ToArray ());
		}
	}
}
