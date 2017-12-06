using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace DigiClay
{
	[System.Serializable]
	public class AdvancedMesh
	{
		#region HalfEdge
		// CW, same with the unity mesh triangle
		public class HalfEdge
		{
			int toVertexIndex;
			int fromVertexIndex;

			public int ToVertexIndex {
				get {
					return toVertexIndex;
				}
			}

			int faceIndex;

			public int FaceIndex {
				get {
					return faceIndex;
				}
				set {
					faceIndex = value;
				}
			}

			HalfEdge next;

			public HalfEdge Next {
				get {
					return next;
				}
				set {
					next = value;
				}
			}

			HalfEdge prev;

			public HalfEdge Prev {
				get {
					return prev;
				}
				set {
					prev = value;
				}
			}

			HalfEdge opposite;

			public HalfEdge Opposite {
				get {
					return opposite;
				}
				set {
					opposite = value;
				}
			}

			public HalfEdge(int from, int to)
			{
				fromVertexIndex = from;
				toVertexIndex = to;

				//-1 means no face
				faceIndex = -1;
				opposite = null;
				next = null;

				//optional
				prev = null;
			}

			override public string ToString()
			{
				return "[" + fromVertexIndex + " - " + toVertexIndex + "]"
					+ "\t\t next:" + "[" + next.fromVertexIndex + " - " + next.toVertexIndex + "]"
					+ "\t\t opposite:" + "[" + opposite.fromVertexIndex + " - " + opposite.toVertexIndex + "]"
					+ "\t\t face index:" + "[" + faceIndex + "]"
					;
			}
		}
		#endregion


		//unity mesh ref
		[SerializeField]
		Mesh _mesh;

		//all Half Edges
		List<HalfEdge> _allHalfEdges;

		//Half Edge Refs for each Vertex
		HalfEdge[] _outgoingHalfEdgeOfAVertex;

		//Half Edge Refs for each Face
		HalfEdge[] _halfEdgeOfAFace;

		//temp
		Dictionary<Vector2Int, HalfEdge> _tmpHalfEdgesDic;

		public Mesh mesh {
			get {
				return _mesh;
			}
			set {
				_mesh = value;
				//calculate
//				GenerateHalfEdgesFromMesh(value);
			}
		}

		public AdvancedMesh(Mesh mesh)
		{
			_mesh = mesh;
			GenerateHalfEdgesFromMesh (mesh);
		}

		void GenerateHalfEdgesFromMesh(Mesh mesh)
		{
			_allHalfEdges = new List<HalfEdge> ();

			_outgoingHalfEdgeOfAVertex = new HalfEdge[mesh.vertexCount];

			int numOfFaces = mesh.triangles.Length / 3;

			_halfEdgeOfAFace = new HalfEdge[numOfFaces];

			_tmpHalfEdgesDic = new Dictionary<Vector2Int, HalfEdge> ();

			//1
			//generate half edges from all triangles
			//next and prev are ready.
			for (int i = 0; i < numOfFaces; ++i)
			{
				GenerateHalfEdgesFromAFace (i,
					mesh.triangles [i * 3 + 0],
					mesh.triangles [i * 3 + 1],
					mesh.triangles [i * 3 + 2]);
			}

			//2
			//link half edges based on faces
			for (int i = 0; i < numOfFaces; ++i)
			{
				LinkHalfEdges (i,
					mesh.triangles [i * 3 + 0],
					mesh.triangles [i * 3 + 1],
					mesh.triangles [i * 3 + 2]);
			}

			//3
			//link boundary edges
			LinkBoundaryHalfEdges();

			_tmpHalfEdgesDic.Clear ();

			Debug.Log ("Advanced Mesh Initialized. Half Edges Count: " + _allHalfEdges.Count);

		}

		void GenerateHalfEdgesFromAFace(int index, int a, int b, int c)
		{
			// a -> b
			HalfEdge ab = new HalfEdge(a, b);
			HalfEdge ba = new HalfEdge(b, a);
			// b -> c
			HalfEdge bc = new HalfEdge(b, c);
			HalfEdge cb = new HalfEdge(c, b);
			// c -> a
			HalfEdge ca = new HalfEdge(c, a);
			HalfEdge ac = new HalfEdge(a, c);

			ab.Opposite = ba;
			ba.Opposite = ab;

			bc.Opposite = cb;
			cb.Opposite = bc;

			ca.Opposite = ac;
			ac.Opposite = ca;


			//this might cause overwrite, so prevent it
			if (_outgoingHalfEdgeOfAVertex [a] == null)
				_outgoingHalfEdgeOfAVertex [a] = ab;
			if (_outgoingHalfEdgeOfAVertex [b] == null)
				_outgoingHalfEdgeOfAVertex [b] = bc;
			if (_outgoingHalfEdgeOfAVertex [c] == null)
				_outgoingHalfEdgeOfAVertex [c] = ca;

			_halfEdgeOfAFace [index] = ab;

			if (!_tmpHalfEdgesDic.ContainsKey(new Vector2Int (a, b)))
				_tmpHalfEdgesDic.Add (new Vector2Int (a, b), ab);
			if (!_tmpHalfEdgesDic.ContainsKey(new Vector2Int (b, a)))
				_tmpHalfEdgesDic.Add (new Vector2Int (b, a), ba);
			if (!_tmpHalfEdgesDic.ContainsKey(new Vector2Int (b, c)))
				_tmpHalfEdgesDic.Add (new Vector2Int (b, c), bc);
			if (!_tmpHalfEdgesDic.ContainsKey(new Vector2Int (c, b)))
				_tmpHalfEdgesDic.Add (new Vector2Int (c, b), cb);
			if (!_tmpHalfEdgesDic.ContainsKey(new Vector2Int (c, a)))
				_tmpHalfEdgesDic.Add (new Vector2Int (c, a), ca);
			if (!_tmpHalfEdgesDic.ContainsKey(new Vector2Int (a, c)))
				_tmpHalfEdgesDic.Add (new Vector2Int (a, c), ac);
		}

		void LinkHalfEdges(int index, int a, int b, int c)
		{
			HalfEdge ab;
			_tmpHalfEdgesDic.TryGetValue (new Vector2Int (a, b), out ab);
			HalfEdge bc;
			_tmpHalfEdgesDic.TryGetValue (new Vector2Int (b, c), out bc);
			HalfEdge ca;
			_tmpHalfEdgesDic.TryGetValue (new Vector2Int (c, a), out ca);

			ab.Next = bc;
			bc.Next = ca;
			ca.Next = ab;

			ab.FaceIndex = index;
			bc.FaceIndex = index;
			ca.FaceIndex = index;

			_allHalfEdges.Add (ab);
			_allHalfEdges.Add (bc);
			_allHalfEdges.Add (ca);

			_tmpHalfEdgesDic.Remove (new Vector2Int (a, b));
			_tmpHalfEdgesDic.Remove (new Vector2Int (b, c));
			_tmpHalfEdgesDic.Remove (new Vector2Int (c, a));
		}

		void LinkBoundaryHalfEdges()
		{
			foreach (KeyValuePair<Vector2Int, HalfEdge> boundaryKvp in _tmpHalfEdgesDic) {

				HalfEdge edge = boundaryKvp.Value;

				HalfEdge x = _tmpHalfEdgesDic.Where(kvp => kvp.Key.x == boundaryKvp.Key.y).First().Value;

				edge.Next = x;

				_allHalfEdges.Add (edge);
			}
		}

		public void PrintAllHalfEdges()
		{
			foreach (var he in _allHalfEdges)
				Debug.Log (he.ToString ());
		}

		public List<Vector3> GetOneRingOfAVertex(int vertexIndex)
		{
			var oneRingVertices = new List<Vector3> ();

			HalfEdge h = _outgoingHalfEdgeOfAVertex [vertexIndex];
			HalfEdge hStop = h;

//			Debug.Log("[vertex " + vertexIndex + " position : " +  _mesh.vertices [vertexIndex] + "]");

			do {
				int index = h.ToVertexIndex;

				var data = _mesh.vertices [index];
				oneRingVertices.Add(data);
//				Debug.Log (" # vertex " + index + " : " + data);

				var opp = h.Opposite;
				var next = opp.Next;
				h = next;
			}
			while (h != hStop);

			return oneRingVertices;
		}

		public void SmoothVertex(int index)
		{
			var oneRingVertices = GetOneRingOfAVertex (index);

			Vector3[] vertices = _mesh.vertices;

			Vector3 average = new Vector3 ();

			foreach (var vertex in oneRingVertices)
				average += vertex;

			average /= oneRingVertices.Count;

			Debug.Log ("Origin: " + _mesh.vertices [index] + " Average: " + average);

			vertices [index] = average;

			_mesh.vertices = vertices;

			Debug.Log ("After: " + _mesh.vertices [index]);
		}

		public void Smooth(int times = 1)
		{
			for (int i = 0; i < times; ++i)
				for (int j = 0; j < _mesh.vertexCount; ++j)
					SmoothVertex (j);
		}

	}
}