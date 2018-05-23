using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DigiClay;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public static UIManager Instance;

	Dictionary<string, Action> _commandDict = new Dictionary<string, Action>();

    public Slider m_scoreSlider;
    public Text m_scoreText;

	void Awake()
	{
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy (this);
		}
	}


    //OnScreenUIManager.Instance.AddCommand ("Sculpture", () => {
    //Mode = EditMode.Sculpture;
    //});

	public void AddCommand(string s, Action a)
	{
        if (!_commandDict.ContainsKey(s))
    		_commandDict.Add (s, a);
	}

	// Use this for initialization
	void Start () {
	}

	void OnGUI()
	{
		if (_commandDict == null)
			return;
		
		foreach (var kvp in _commandDict) {
			if (GUILayout.Button (kvp.Key)) {
				kvp.Value.Invoke ();
			}
		}
	}

    private void Update()
    {
        if (m_scoreSlider != null)
        {
            m_scoreSlider.value = Mathf.Clamp01(DataAnalysisManager.Instance.CC);
        }

        if (m_scoreText != null)
        {
            var s = Mathf.Clamp01(DataAnalysisManager.Instance.CC) * 100f;
            m_scoreText.text = s.ToString("F2");
        }
        
    }
}
