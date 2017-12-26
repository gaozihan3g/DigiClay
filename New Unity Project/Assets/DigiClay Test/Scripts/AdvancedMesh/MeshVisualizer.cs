using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshVisualizer : MonoBehaviour {

    public enum DisplayType
    {
        All,
        Weights,
        Individuals,
        Normals
    }

    public DisplayType displayType = DisplayType.All;

    public Color ColorA = Color.red;
    public Color ColorB = Color.blue;

    public List<int> _vertexIndices;
	public List<Color> _vertexColors;
	public float size = 0.01f;
    public float length = 0.05f;

	public MeshFilter _meshFilter;
	Mesh _mesh;

    float[] weights;

    public float[] Weights
    {
        get
        {
            return weights;
        }

        set
        {
            weights = value;
        }
    }

    // Use this for initialization
    void Awake() {

        if (_meshFilter == null)
            _meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
		_mesh = _meshFilter.mesh;
        weights = new float[_mesh.vertexCount];
	}

    void OnDrawGizmos()
	{
		if (_mesh == null)
			return;

        switch (displayType)
        {
            case DisplayType.All:
                DisplayAll();
                break;
            case DisplayType.Weights:
                DisplayWeights();
                break;
            case DisplayType.Individuals:
                DisplayIndividuals();
                break;
            case DisplayType.Normals:
                DisplayNormals();
                break;
        }
    }

    void DisplayAll()
    {
        for (int i = 0; i < _mesh.vertexCount; ++i)
        {
            Gizmos.color = ColorA;
			Gizmos.DrawSphere(_mesh.vertices[i] * transform.localScale.x + transform.position, size * transform.localScale.x);
        }
    }

    void DisplayWeights()
    {
        for (int i = 0; i < _mesh.vertexCount; ++i)
        {
            float t = weights[i];
            Gizmos.color = Color.Lerp(ColorA, ColorB, t);
			Gizmos.DrawSphere(_mesh.vertices[i] * transform.localScale.x + transform.position, size * transform.localScale.x);
        }
    }

    void DisplayIndividuals()
    {
        for (int i = 0; i < _vertexIndices.Count; i++)
        {
            Gizmos.color = _vertexColors[i];
            Gizmos.DrawSphere(_mesh.vertices[_vertexIndices[i]] * transform.localScale.x + transform.position, size * transform.localScale.x);
        }
    }

    void DisplayNormals()
    {
        for (int i = 0; i < _mesh.vertexCount; i++)
        {
            Gizmos.color = Color.Lerp(ColorA, ColorB, _mesh.uv[i].x);

            Vector3 from = _mesh.vertices[i] * transform.localScale.x + transform.position;

            Vector3 direction = _mesh.normals[i] * length;
            Gizmos.DrawRay(from, direction);
        }
        //Debug.Log(string.Format("from {0} to {1}", _mesh.vertices[0] * transform.localScale.x + transform.position, _mesh.vertices[0] * transform.localScale.x + transform.position + _mesh.normals[0] * length));
    }

	public void DeformCallBack(DeformableBase bd)
    {
        weights = bd.WeightList.ToArray();
    }
}
