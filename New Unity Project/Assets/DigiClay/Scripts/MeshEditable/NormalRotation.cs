using UnityEngine;

public class NormalRotation : MonoBehaviour
{
    public float speed = 100.0f;

    // Update is called once per frame
    void Update()
    {
        // obtain the normals from the Mesh
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] normals = mesh.normals;

        // edit the normals in an external array
        Quaternion rotation = Quaternion.AngleAxis(Time.deltaTime * speed, Vector3.up);
        for (int i = 0; i < normals.Length; i++)
            normals[i] = rotation * normals[i];

        // assign the array of normals to the mesh
        mesh.normals = normals;
    }
}