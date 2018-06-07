using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LookAt : MonoBehaviour {

    public Transform m_target;

    private void Start()
    {
    }

    // Update is called once per frame
    void Update () {

        if (m_target != null)
            transform.LookAt(m_target);
	}
}
