using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SystemManager : MonoBehaviour {

	public enum EditMode
	{
		HeightControl,
		Sculpture,
		Paint,
		Smooth
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

	void Start()
	{
		OnScreenUIManager.Instance.AddCommand ("Sculpture", () => {
			Mode = EditMode.Sculpture;
		});

		OnScreenUIManager.Instance.AddCommand ("Height", () => {
			Mode = EditMode.HeightControl;
		});

		OnScreenUIManager.Instance.AddCommand ("Paint", () => {
			Mode = EditMode.Paint;
		});
		OnScreenUIManager.Instance.AddCommand ("Smooth", () => {
			Mode = EditMode.Smooth;
		});

	}
}
