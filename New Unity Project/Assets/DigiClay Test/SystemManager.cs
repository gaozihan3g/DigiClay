using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SystemManager : MonoBehaviour {

	public enum EditMode
	{
		HeightControl,
		Sculpture,
		Paint
	}

	[SerializeField]
	private EditMode _mode = EditMode.Sculpture;

	public EditMode Mode {
		get {
			return _mode;
		}
		set {
			_mode = value;
		}
	}

	public static SystemManager Instance;


	void Awake()
	{
		if (Instance == null) {
			Instance = this;
		} else {
			Destroy (this);
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
