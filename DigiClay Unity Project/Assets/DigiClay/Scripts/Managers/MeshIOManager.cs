
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityExtension;
using DigiClay;

public class MeshIOManager : MonoBehaviour {

    public static MeshIOManager Instance;

    public ClayObject m_clayObject;

	[SerializeField]
	Mesh m_mesh;
    [SerializeField]
    ClayMesh m_clayMesh;

    public Mesh Mesh {
		get {
			return m_mesh;
		}
		set {
			m_mesh = value;
		}
	}

    public ClayMesh ClayMesh
    {
        get
        {
            return m_clayMesh;
        }

        set
        {
            m_clayMesh = value;
        }
    }

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
            
            var files = Directory.GetFiles(DigiClayConstant.CLAY_DATA_PATH, "*.obj");

            foreach (var f in files)
            {
                Debug.Log(f);
            }
            //TODO load mesh data
        });

        OnScreenUIManager.Instance.AddCommand("Create Clay Object", () => {

            ClayObject a = ScriptableObject.CreateInstance<ClayObject>();
            AssetDatabase.CreateAsset(a, DigiClayConstant.CLAY_DATA_PATH + "MyClay.asset");
            Debug.Log(AssetDatabase.GetAssetPath(a));
        });
    }

	public void ExportMesh()
	{
		Export();
	}

    public void Export(string meshName = "")
    {
        if (meshName.IsNullOrEmpty())
            meshName = System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");

        if (!Directory.Exists(DigiClayConstant.CLAY_DATA_PATH))
        {
            Directory.CreateDirectory(DigiClayConstant.CLAY_DATA_PATH);
            Debug.Log("CLAY_DATA_PATH Directory created.");
        }


        var lStream = new FileStream(DigiClayConstant.CLAY_DATA_PATH + meshName + ".obj", FileMode.Create);
        var lOBJData = Mesh.EncodeOBJ();
        OBJLoader.ExportOBJ(lOBJData, lStream);
        lStream.Close();
        Debug.Log("Mesh Saved.");

        AssetDatabase.Refresh();

        ClayObject co = ScriptableObject.CreateInstance<ClayObject>();
        co.ClayName = meshName;
        co.ClayMesh = ClayMesh;
        co.ModelFile = AssetDatabase.LoadMainAssetAtPath(DigiClayConstant.CLAY_DATA_PATH + meshName + ".obj");

        AssetDatabase.CreateAsset(co, DigiClayConstant.CLAY_DATA_PATH + meshName + " ClayData.asset");
        Debug.Log(AssetDatabase.GetAssetPath(co));

        AssetDatabase.SaveAssets();
    }
}
