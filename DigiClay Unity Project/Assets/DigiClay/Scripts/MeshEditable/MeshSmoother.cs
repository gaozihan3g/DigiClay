using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

[ExecuteInEditMode]
public class MeshSmoother : MonoBehaviour {

    public int m_iterations = 1;
    public float alpha = 0.5f;
    public float beta = 0.75f;

    public bool isHC = false;

    [SerializeField]
	ClayMeshContext m_cmc;


    void Awake()
    {
		m_cmc = GetComponent<ClayMeshContext>();
    }

    public void SmoothMesh()
    {
		Mesh mesh = GetComponent<MeshFilter> ().mesh;

        if (isHC)
        {
            mesh = MeshSmoothing.HCFilter(mesh, m_iterations, alpha, beta, m_cmc.clayMesh.IsFeaturePoints.ToArray());
            Debug.Log("HC Smoothing " + m_iterations);
        }
        else
        {
            mesh = MeshSmoothing.LaplacianFilter(mesh, m_iterations, m_cmc.clayMesh.IsFeaturePoints.ToArray());
            Debug.Log("LaplacianSmoothing " + m_iterations);
        }
        

    }
}
