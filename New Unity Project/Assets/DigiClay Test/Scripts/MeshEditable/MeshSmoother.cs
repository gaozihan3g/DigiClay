using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshSmoother : MonoBehaviour {

    public int m_iterations = 1;
    public int start = 0;
    public int end = 1;

    AdvancedMeshContext m_amc;


    void Awake()
    {
        m_amc = GetComponent<AdvancedMeshContext>();
    }

    public void SmoothMesh()
    {
        if (m_amc == null)
            m_amc = GetComponent<AdvancedMeshContext>();

        //m_amc.AdvMesh.Smooth(m_iterations);

        for (int i = start; i < end; ++i)
            m_amc.AdvMesh.SmoothVertex(i);
        
        Debug.Log("LaplacianSmoothing " + m_iterations);
    }
}
