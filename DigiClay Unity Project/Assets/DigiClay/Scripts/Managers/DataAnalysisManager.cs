using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

public class DataAnalysisManager : MonoBehaviour {

    public enum AnalysisMode
    {
        Editor,
        RealTime
    }

    public AnalysisMode m_mode = AnalysisMode.Editor;

    public ClayObject m_clayA;
    public ClayObject m_clayB;

    public List<float> m_dataA;
    public List<float> m_dataB;

    public float m_correlation = 0f;

    private void Update()
    {
        if (m_mode != AnalysisMode.RealTime)
            return;

        RealtimeCalculation();
    }

    public void RealtimeCalculation()
    {
        Correlation(m_clayA.ClayMesh, MeshIOManager.Instance.ClayMesh);
    }

    public void EditorModeCalculation()
    {
        Correlation(m_clayA.ClayMesh, m_clayB.ClayMesh);
    }
    void Correlation(ClayMesh clayMeshA, ClayMesh clayMeshB)
    {
        List<float> init_dataA = clayMeshA.RowAvgRadiusList;
        List<float> init_dataB = clayMeshB.RowAvgRadiusList;

        if (init_dataA.Count != init_dataB.Count)
            Debug.LogError("Two lists have different length!");

        // avg A
        float avgA = GetAvg(init_dataA);
        // avg B
        float avgB = GetAvg(init_dataB);

        m_dataA = GetNewData(avgA, init_dataA);
        m_dataB = GetNewData(avgB, init_dataB);

        m_correlation = GetRho(m_dataA, m_dataB);
    }

    float GetAvg(List<float> data)
    {
        float avg = 0f;
        foreach (float d in data)
            avg += d;
        avg /= data.Count;
        return avg;
    }

    List<float> GetNewData(float avg, List<float> data)
    {
        List<float> new_data = new List<float>();

        foreach (float d in data)
            new_data.Add(d - avg);

        return new_data;
    }

    float GetRho(List<float> dataA, List<float> dataB)
    {
        float r = 0f;
        float a = 0f;
        float b = 0f;
        float c = 0f;

        //a
        for (int i = 0; i < dataA.Count; ++i)
        {
            a += dataA[i] * dataB[i];
        }
        //b
        for (int i = 0; i < dataA.Count; ++i)
        {
            b += dataA[i] * dataA[i];
        }
        b = Mathf.Sqrt(b);
        //c
        for (int i = 0; i < dataB.Count; ++i)
        {
            c += dataB[i] * dataB[i];
        }

        c = Mathf.Sqrt(c);

        r = a / (b * c);

        return r;
    }
}
