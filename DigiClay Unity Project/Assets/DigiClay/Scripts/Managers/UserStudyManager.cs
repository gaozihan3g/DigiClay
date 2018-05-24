using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;
using System.IO;
using System.Text;
using UnityEditor;

[ExecuteInEditMode]
public class UserStudyManager : MonoBehaviour {

    public class UserSession
    {
        public int id;
        public List<float> time;
        public List<float> cc;
    }

    public string filename = "data.txt";
    public Material refMat;
    public ClayObject[] refClayData;

    // timer

    // random sequence

    // button: next start end

    public float[] timeList;
    public float[] ccList;

    [SerializeField]
    List<int> m_randomSequence;
    [SerializeField]
    int m_currentIndex = 0;
    [SerializeField]
    int m_currentTaskIndex = 0;

    public float timer = 0f;
    public float cc;

    bool m_isTimeCounting = false;
    float startTime;

    List<int> GetRandomSequence(int n)
    {
        List<int> result = new List<int>();

        List<int> pool = new List<int>();

        for (int i = 0; i < n; ++i)
        {
            pool.Add(i);
        }

        for (int i = 0; i < n; ++i)
        {
            int r = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[r]);
            pool.RemoveAt(r);
        }

        return result;
    }


    public bool IsTimeCounting
    {
        get
        {
            return m_isTimeCounting;
        }

        set
        {
            if (m_isTimeCounting != value && value)
                startTime = Time.time;

            if (m_isTimeCounting != value && !value)
                timeList[CurrentTaskIndex] = timer;

            m_isTimeCounting = value;
        }
    }

    public int CurrentTaskIndex
    {
        get
        {
            return m_currentTaskIndex;
        }
    }

    public int CurrentIndex
    {
        get
        {
            return m_currentIndex;
        }

        set
        {
            m_currentIndex = value;

            if (m_currentIndex == m_randomSequence.Count)
                return;

            m_currentTaskIndex = m_randomSequence[m_currentIndex];
        }
    }

    public void Init()
    {
        int dataLength = refClayData.Length;
        m_randomSequence = GetRandomSequence(dataLength);
        CurrentIndex = 0;
        timeList = new float[dataLength];
        ccList = new float[dataLength];
        refMat.mainTexture = null;
    }

    public void StartTask()
    {
        if (m_currentIndex == refClayData.Length)
        {
            Debug.LogWarning("All tasks recorded!");
            return;
        }

        SetupTask(CurrentTaskIndex);
    }

    public void EndTask()
    {
        if (m_currentIndex == refClayData.Length)
        {
            Debug.LogWarning("All tasks recorded!");
            return;
        }

        IsTimeCounting = false;
        
        // cc
        ccList[CurrentTaskIndex] = DataAnalysisManager.Instance.CC;

        // last
        ++CurrentIndex;
    }

    public void SaveData()
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < refClayData.Length; ++i)
        {
            string s = timeList[i].ToString() + "\t" + ccList[i].ToString() + "\t";
            sb.Append(s);
        }

        sb.Append("\n");

        File.AppendAllText(DigiClayConstant.STAT_DATA_PATH + filename, sb.ToString());

        AssetDatabase.Refresh();
    }

    void SetupTask(int i)
    {
        // time
        IsTimeCounting = true;
        // image
        refMat.mainTexture = refClayData[i].RefImage;
        // cc
        DataAnalysisManager.Instance.TargetClay = refClayData[i];
    }


    private void Update()
    {
        if (IsTimeCounting)
        {
            timer = Time.time - startTime;
            cc = DataAnalysisManager.Instance.CC;
        }
    }

}
