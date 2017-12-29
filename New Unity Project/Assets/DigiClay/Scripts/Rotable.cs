using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotable : MonoBehaviour {

	[Range(10f, 1000f)]
    public float speed = 100f;


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

        transform.Rotate(Vector3.up * Time.deltaTime * speed);

    }
}
