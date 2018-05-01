using UnityEngine;
using System;

public class CodeExecutionTime : MonoBehaviour {

    public int total;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI()
	{
        if (GUILayout.Button("Execute"))
        {
            CodeExecutionTime.Execute(()=> { GetSum(total); });
        }
	}

    public static double Execute(Action callback)
    {
        var before = DateTime.Now;

        callback.Invoke();

        var after = DateTime.Now;

        var span = after.Subtract(before);

        Debug.Log(span.TotalMilliseconds);

        return span.TotalMilliseconds;
    }

    void GetSum(int t)
    {
        int sum = 0;

        for (int i = 0; i < t; ++i)
            sum += i;
    }
}
