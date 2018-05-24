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
        public Texture RefImage;


        public void Link()
        {
            AssetDatabase.Refresh();
            ModelFile = AssetDatabase.LoadMainAssetAtPath(DigiClayConstant.CLAY_DATA_PATH + ClayName + ".obj");
            AssetDatabase.SaveAssets();
        }
    }

}