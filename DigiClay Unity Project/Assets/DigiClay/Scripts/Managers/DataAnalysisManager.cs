using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

public class DataAnalysisManager : MonoBehaviour {

    public enum AnalysisMode
    {
        Normal,
        Curvature
    }

    public AnalysisMode m_mode = AnalysisMode.Normal;
    public bool m_realtime = false;
    public static DataAnalysisManager Instance;
    public ClayObject m_clayA;
    public ClayObject m_clayB;

    public List<float> m_dataA;
    public List<float> m_dataB;

    [SerializeField]
    float m_correlation = 0f;

    public float CC
    {
        get
        {
            return m_correlation;
        }

        set
        {
            m_correlation = value;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (!m_realtime)
            return;

        RealtimeCalculation();
    }

    public void RealtimeCalculation()
    {
        if (m_mode == AnalysisMode.Normal)
            CalculatePositionCC(m_clayA.ClayMesh, MeshIOManager.Instance.ClayMesh);
        else
            CalculateCurvatureCC(m_clayA.ClayMesh, MeshIOManager.Instance.ClayMesh);
    }

    public void EditorModeCalculation()
    {
        if (m_mode == AnalysisMode.Normal)
            CalculatePositionCC(m_clayA.ClayMesh, m_clayB.ClayMesh);
        else
            CalculateCurvatureCC(m_clayA.ClayMesh, m_clayB.ClayMesh);
    }

    void GetCorrelation(List<float> init_dataA, List<float> init_dataB)
    {
        if (init_dataA.Count != init_dataB.Count)
            Debug.LogError("Two lists have different length!");

        // avg A
        float avgA = GetAvg(init_dataA);
        // avg B
        float avgB = GetAvg(init_dataB);

        m_dataA = GetNewData(avgA, init_dataA);
        m_dataB = GetNewData(avgB, init_dataB);

        CC = GetRho(m_dataA, m_dataB);
    }

    void CalculateCurvatureCC(ClayMesh clayMeshA, ClayMesh clayMeshB)
    {
        List<float> init_dataA = clayMeshA.RowAvgRadiusList;
        List<float> init_dataB = clayMeshB.RowAvgRadiusList;

        var curA = GetCurvature(init_dataA);
        var curB = GetCurvature(init_dataB);

        GetCorrelation(curA, curB);
    }

    void CalculatePositionCC(ClayMesh clayMeshA, ClayMesh clayMeshB)
    {
        List<float> init_dataA = clayMeshA.RowAvgRadiusList;
        List<float> init_dataB = clayMeshB.RowAvgRadiusList;

        GetCorrelation(init_dataA, init_dataB);
    }

    List<float> GetCurvature(List<float> r)
    {
        List<float> result = new List<float>();

        for (int i = 1; i < r.Count - 1; ++i)
        {
            float ddr = r[i - 1] + r[i + 1] - 2 * r[i];
            result.Add(ddr);
        }

        return result;
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
