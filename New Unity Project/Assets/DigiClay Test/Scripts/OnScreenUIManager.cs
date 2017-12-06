using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OnScreenUIManager : MonoBehaviour {

	public static OnScreenUIManager Instance;

	Dictionary<string, Action> _commandDict = new Dictionary<string, Action>();

	void Awake()
	{
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy (this);
		}
	}

	public void AddCommand(string s, Action a)
	{
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
