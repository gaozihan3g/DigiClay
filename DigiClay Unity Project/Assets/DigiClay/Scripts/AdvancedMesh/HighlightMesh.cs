using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

[ExecuteInEditMode]
public class HighlightMesh : MonoBehaviour {

    public int axisDivision;
    public float height;
    public List<float> radiusList;

    MeshFilter m_meshFilter;
    private void Awake()
    {
        m_meshFilter = GetComponent<MeshFilter>();
    }

    private void Start()
    {
        
    }

    public void UpdateMesh()
    {
        int heightDivision = radiusList.Count - 1;

        float angleDelta = Mathf.PI * 2f / axisDivision;
        float heightDelta = height / heightDivision;

        float angleTheta = 0f;
        float heightTheta = 0f;

        var newVertices = new List<Vector3>();
        var newTriangles = new List<int>();

        for (int i = 0; i < heightDivision + 1; ++i)
        {
            for (int j = 0; j < axisDivision; ++j)
            {
                float r = radiusList[i];
                Vector3 p = new Vector3(r * Mathf.Cos(angleTheta), heightTheta, r * Mathf.Sin(angleTheta));

                newVertices.Add(p);

                angleTheta += angleDelta;
            }

            heightTheta += heightDelta;
        }

        for (int j = 0; j < heightDivision; ++j)
        {
            for (int i = 0; i < axisDivision; ++i)
            {
                ClayMesh.CreateTriangle(newTriangles,
                               i + axisDivision * j,
                               (i + 1) % axisDivision + axisDivision * j,
                               i + axisDivision * (j + 1),
                               (i + 1) % axisDivision + axisDivision * (j + 1));
            }
        }

        Mesh m = new Mesh();
        m.vertices = newVertices.ToArray();
        m.triangles = newTriangles.ToArray();

        m_meshFilter.sharedMesh = m;
    }
}
