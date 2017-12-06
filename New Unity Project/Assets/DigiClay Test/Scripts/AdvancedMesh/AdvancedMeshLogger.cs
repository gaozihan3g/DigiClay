using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

public class AdvancedMeshLogger : MonoBehaviour {

	public class TestClass
	{
		public int value;

		public TestClass(int v)
		{
			value = v;
		}
	}

	public class HolderClass
	{
		public TestClass testClass;

		public HolderClass(TestClass tc)
		{
			testClass = tc;
		}
	}


	void Awake()
	{
		TestClass tc = new TestClass (1);

		HolderClass hc = new HolderClass (tc);

		tc.value = 2;

		Debug.Log (tc.value);

		Debug.Log (hc.testClass.value);


		hc.testClass.value = 3;

		Debug.Log (tc.value);

		Debug.Log (hc.testClass.value);

	}





//	AdvancedMesh _advMesh;
//
//	// Use this for initialization
//	void Start () {
//		
//		var mesh = GetComponent<MeshFilter> ().mesh;
//
//		_advMesh = new AdvancedMesh (mesh);
//
//		_advMesh.PrintAllHalfEdges ();
//
//	}
	
//	void OnGUI() {
//		if (GUI.Button (new Rect (10, 10, 150, 100), "I am a button"))
//			_advMesh.Smooth ();
//	}
}
