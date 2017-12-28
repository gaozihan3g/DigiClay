using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class UITextLabel : MonoBehaviour {

	public float min = 1f;
	public float max = 100f;

	Text _text;

	void Awake()
	{
		if (_text == null)
			_text = GetComponent<Text> ();
	}

	public void SetTextFromLabel(float v) {
		_text.text = Mathf.Lerp (min, max, v).ToString ("###");
    }
}
