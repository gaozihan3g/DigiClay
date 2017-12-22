
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExtension;

public class MeshIOManager : MonoBehaviour {

    public static MeshIOManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    void Start()
    {
        OnScreenUIManager.Instance.AddCommand("Load All Mesh", () => {
            
            var files = Directory.GetFiles(DigiClayConstant.OUTPUT_PATH, "*.obj");

            foreach (var f in files)
            {
                Debug.Log(f);
            }
            //TODO load mesh data
        });
    }

    public void ExportMesh(Mesh mesh, string meshName = "")
    {
        if (meshName.IsNullOrEmpty())
            meshName = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        if (!Directory.Exists(DigiClayConstant.OUTPUT_PATH))
        {
            Directory.CreateDirectory(DigiClayConstant.OUTPUT_PATH);
            Debug.Log("Directory created.");
        }


        var lStream = new FileStream(DigiClayConstant.OUTPUT_PATH + meshName + ".obj", FileMode.Create);
        var lOBJData = mesh.EncodeOBJ();
        OBJLoader.ExportOBJ(lOBJData, lStream);
        lStream.Close();
        Debug.Log("Mesh Saved.");
    }
}
