using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour {

    [SerializeField]
    [Range(0.1f, 2f)]
    float _radius = 1;

    [SerializeField]
    [Range(0.1f, 2f)]
    float _height = 1;

    [SerializeField]
    [Range(3, 36)]
    int _segment = 8;

    [SerializeField]
    [Range(1, 30)]
    int _verticalSegment = 10;

    [SerializeField]
    [Range(0.01f, 2f)]
    float _thickness = 0.01f;

    public Material[] _materials;

    [SerializeField]
    Transform _root = null;

    MeshFilter _meshFilter;
    MeshRenderer _meshRenderer;
    MeshCollider _meshCollider;

    Matrix4x4 matrix4X;

    // Use this for initialization
    void Start() {

        //_root = GameObject.Find("GeometryRoot").transform;

        if (_root == null)
        {
            GameObject go = new GameObject("GeometryRoot");
            go.transform.parent = this.transform;
            _root = go.transform;

            _meshFilter = go.AddComponent<MeshFilter>();
            _meshRenderer = go.AddComponent<MeshRenderer>();
            _meshCollider = go.AddComponent<MeshCollider>();
            go.AddComponent<Deformable>();
        }
        else
        {
            _meshFilter = GetComponentInChildren<MeshFilter>();
            _meshRenderer = GetComponentInChildren<MeshRenderer>();
            _meshCollider = GetComponentInChildren<MeshCollider>();
        }

        matrix4X = _root.worldToLocalMatrix;

        Debug.Log(matrix4X);

        Mesh mesh = new Mesh();
        mesh.name = "Generated Mesh";

        MeshGeneration(ref mesh);

        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;

        if (_materials.Length < mesh.subMeshCount)
            Debug.LogError("Materials are needed for submeshes!");

        _meshRenderer.materials = _materials;
    }

    void MeshGeneration(ref Mesh mesh)
    {
        List<Vector3> outerVertices = new List<Vector3>();
        List<Vector3> innerVertices = new List<Vector3>();

        List<Vector3> newNormals = new List<Vector3>();

        List<int> outerTriangles = new List<int>();
        List<int> innerTriangles = new List<int>();
        List<int> edgeTriangles = new List<int>();

        float theta = 0f;
        float delta = 2f * Mathf.PI / (float)_segment;

        float heightTheta = 0f;
        float heightDelta = (float)_height / (float)_verticalSegment;

        // origin
        outerVertices.Add(Vector3.zero);
        // inner origin
        Vector3 innerOrigin = new Vector3(0f, Mathf.Min(_thickness, 0.5f * heightDelta), 0f);
        innerVertices.Add(innerOrigin);

        float innerRadius = Mathf.Max(0, _radius - _thickness);

        // add vertices
        for (int j = 0; j < _verticalSegment + 1; ++j)
        {
            for (int i = 0; i < _segment; ++i)
            {
                outerVertices.Add(new Vector3(_radius * Mathf.Cos(theta), heightTheta, _radius * Mathf.Sin(theta)));
                innerVertices.Add(new Vector3(innerRadius * Mathf.Cos(theta), heightTheta + (j == 0 ? innerOrigin.y : 0), innerRadius * Mathf.Sin(theta)));
                theta += delta;
            }
            heightTheta += heightDelta;
        }

        List<Vector3> allVertices = new List<Vector3>();
        allVertices.AddRange(outerVertices);
        allVertices.AddRange(innerVertices);
        mesh.vertices = allVertices.ToArray();

        int offset = outerVertices.Count;

        //add triangles

        //bottom
        for (int i = 0; i < _segment - 1; ++i)
        {
            CreateTriangle(outerTriangles, 0, i + 1, i + 2);
            CreateTriangle(innerTriangles, 0, i + 2, i + 1, offset);
        }

        //last one
        CreateTriangle(outerTriangles, 0, _segment, 1);
        CreateTriangle(innerTriangles, 0, 1, _segment, offset);


        //side
        for (int j = 0; j < _verticalSegment; ++j)
        {
            for (int i = 1; i < _segment; ++i)
            {
                CreateTriangle(outerTriangles, i + j * _segment, i + _segment + j * _segment, i + 1 + j * _segment);
                CreateTriangle(outerTriangles, i + 1 + j * _segment, i + _segment + j * _segment, i + 1 + _segment + j * _segment);

                CreateTriangle(innerTriangles, i + j * _segment, i + 1 + j * _segment, i + _segment + j * _segment, offset);
                CreateTriangle(innerTriangles, i + 1 + j * _segment, i + 1 + _segment + j * _segment, i + _segment + j * _segment, offset);
            }

            //last two
            CreateTriangle(outerTriangles, _segment + j * _segment, _segment + _segment + j * _segment, 1 + j * _segment);
            CreateTriangle(outerTriangles, 1 + j * _segment, _segment + _segment + j * _segment, 1 + _segment + j * _segment);

            CreateTriangle(innerTriangles, _segment + j * _segment, 1 + j * _segment, _segment + _segment + j * _segment, offset);
            CreateTriangle(innerTriangles, 1 + j * _segment, 1 + _segment + j * _segment, _segment + _segment + j * _segment, offset);
        }

        //edge
        int outerLastVertexIndex = offset - 1;
        int innerLastVertexIndex = 2 * offset - 1;

        //Debug.Log("outIndex " + outerLastVertexIndex + " innerIndex " + innerLastVertexIndex + " total " + mesh.vertexCount);

        for (int i = 1; i < _segment; ++i)
        {
            CreateTriangle(edgeTriangles, outerLastVertexIndex - _segment + i, innerLastVertexIndex - _segment + i, innerLastVertexIndex - _segment + i + 1);
            CreateTriangle(edgeTriangles, outerLastVertexIndex - _segment + i, innerLastVertexIndex - _segment + i + 1, outerLastVertexIndex - _segment + i + 1);
        }
        //last two
        CreateTriangle(edgeTriangles, outerLastVertexIndex, innerLastVertexIndex, innerLastVertexIndex - _segment + 1);
        CreateTriangle(edgeTriangles, outerLastVertexIndex, innerLastVertexIndex - _segment + 1, outerLastVertexIndex - _segment + 1);

        //Debug.Log(edgeTriangles.Count / 3);

        mesh.subMeshCount = 3;
        //set outer submesh
        mesh.SetTriangles(outerTriangles.ToArray(), 0);
        mesh.SetTriangles(innerTriangles.ToArray(), 1);
        mesh.SetTriangles(edgeTriangles.ToArray(), 2);

        mesh.RecalculateNormals();
    }

    void CreateTriangle(List<int> list, int a, int b, int c, int offset = 0)
    {
        if (list == null)
            Debug.LogError("The triangle list is null");
        list.Add(a + offset);
        list.Add(b + offset);
        list.Add(c + offset);
    }
	
	// Update is called once per frame
	void Update () {

        //Mesh mesh = _meshFilter.sharedMesh;
        //mesh.Clear();

        //MeshGeneration(ref mesh);

        //_meshFilter.mesh = mesh;
        //_meshCollider.sharedMesh = mesh;
    }
}
