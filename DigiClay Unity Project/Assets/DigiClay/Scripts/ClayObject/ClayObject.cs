using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DigiClay
{
    [CreateAssetMenu(menuName = "DigiClay/ClayData", fileName = "Clay")]
    public class ClayObject : ScriptableObject {

        public string ClayName;
        public ClayMesh ClayMesh;
        public Object ModelFile;


        public void Link()
        {
            ModelFile = AssetDatabase.LoadMainAssetAtPath(DigiClayConstant.OUTPUT_PATH + ClayName + ".obj");
        }
    }

}