using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DigiClay;

public class OnScreenUIManager : MonoBehaviour {

	public static OnScreenUIManager Instance;

	Dictionary<string, Action> _commandDict = new Dictionary<string, Action>();

	public DigiClay.Cursor Cursor;

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
}
