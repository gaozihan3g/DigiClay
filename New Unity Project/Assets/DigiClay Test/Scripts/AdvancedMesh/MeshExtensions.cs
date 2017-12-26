using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DigiClay
{

    public static class MeshExtensions
    {
        public static void FixUVSeam(this Mesh mesh, Vector2Int[] uvSeamPairs)
        {
            Vector3[] normals = mesh.normals;

            for (int i = 0; i < uvSeamPairs.Length; ++i)
            {
                //Debug.Log(uvSeamPairs[i]);

                int index0 = uvSeamPairs[i].x;
                int index1 = uvSeamPairs[i].y;

                var meanNormal = 0.5f * (normals[index0] + normals[index1]);

                normals[index0] = meanNormal;
                normals[index1] = meanNormal;
            }

            mesh.normals = normals;

            //Debug.Log("UV seam fixed");
        }
    }
}
