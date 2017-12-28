using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using mattatz.MeshSmoothingSystem;

[ExecuteInEditMode]
public class MeshSmoother : MonoBehaviour {

    public int m_iterations = 1;
    public int start = 0;
    public int end = 1;

	ClayMeshContext m_cmc;


    void Awake()
    {
		m_cmc = GetComponent<ClayMeshContext>();
    }

    public void SmoothMesh()
    {
//        if (m_amc == null)
//            m_amc = GetComponent<AdvancedMeshContext>();

        //m_amc.AdvMesh.Smooth(m_iterations);

//        for (int i = start; i < end; ++i)
//            m_amc.AdvMesh.SmoothVertex(i);

		Mesh mesh = GetComponent<MeshFilter> ().mesh;

		Debug.Log ("m_cmc.clayMesh.IsFeaturePoints " + m_cmc.clayMesh.IsFeaturePoints.Count);
		Debug.Log ("mesh count" + mesh.vertexCount);

		mesh = MeshSmoothing.LaplacianFilter(mesh, m_iterations, m_cmc.clayMesh.IsFeaturePoints.ToArray());
        
        Debug.Log("LaplacianSmoothing " + m_iterations);
    }
}
