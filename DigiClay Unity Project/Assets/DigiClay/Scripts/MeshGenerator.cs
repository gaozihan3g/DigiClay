using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour
{
    [SerializeField, Range(DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS)]
    float m_radius = 1;
    [SerializeField, Range(DigiClayConstant.MIN_HEIGHT, DigiClayConstant.MAX_HEIGHT)]
    float m_height = 1;
    [SerializeField, Range(3, 60)]
    int m_segment = 8;
    [SerializeField, Range(1, 100)]
    int m_verticalSegment = 10;
    [SerializeField, Range(0f, 1f)]
    float m_thicknessRatio = 0.5f;
    // noise parameters
    [SerializeField, Range(0f, 1f)]
    float m_topBaseRatio = 0.5f;

    [SerializeField, Range(0f, 5f)]
    float m_centerNoiseScale = 0.02f;
    [SerializeField, Range(0f, 5f)]
    float m_rowNoiseScale = 0.02f;
    [SerializeField, Range(0f, 5f)]
    float m_individualNoiseScale = 0.02f;
    // angle scale is fixed: 0 - 2 * Pi


    [SerializeField, Range(0.01f, 5f)]
    float m_centerNoiseSpan = 0.02f;
    [SerializeField, Range(0.01f, 5f)]
    float m_rowNoiseSpan = 0.02f;
    [SerializeField, Range(0.01f, 5)]
    float m_individualNoiseSpan = 100f;
    [SerializeField, Range(0.01f, 5f)]
    float m_angleNoiseSpan = 0.02f;

    [SerializeField]
    ClayMeshContext m_prefab;

    public void CreateMesh()
    {
        // cleanup first
        for (int i = 0; i < transform.childCount; ++i)
            DestroyImmediate(transform.GetChild(i).gameObject);

        // create ClayMesh
        ClayMesh clayMesh = ClayMeshFactory();

        // assign ClayMesh
        ClayMeshContext cmc = Instantiate<ClayMeshContext>(m_prefab, transform);
        cmc.gameObject.name = "Clay Mesh " + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        cmc.clayMesh = clayMesh;

        // generate mesh based on ClayMesh
        clayMesh.GenerateMesh();
    }

    ClayMesh ClayMeshFactory()
    {
        ClayMesh cMesh = new ClayMesh(m_verticalSegment + 1, m_segment, m_height, m_thicknessRatio);

        float delta = 2f * Mathf.PI / (float)m_segment;
        float heightDelta = (float)m_height / (float)m_verticalSegment;
        float theta = 0f;
        float heightTheta = 0f;
        Perlin perlin = new Perlin();

        for (int j = 0; j < m_verticalSegment + 1; ++j)
        {
            theta = 0f;

            // model the shape as a ellipse, get radius based on height
            // x^2 / a^2 + y^2 / b^2 = 1
            float ellipsoidRadius = Mathf.Sqrt(Mathf.Max(0f, m_height * m_height - heightTheta * heightTheta)) * m_radius / m_height;
            float baseRadius = m_topBaseRatio * m_radius + (1f - m_topBaseRatio) * ellipsoidRadius;

            // get noise center
            float noiseParameter = (float)j / (float)m_verticalSegment;
            // 1. direction
            // 1D
            float noise0 = perlin.Noise(noiseParameter * m_angleNoiseSpan);
            float offsetAngle = Mathf.Lerp(0f, 2 * Mathf.PI, noise0);
            Vector3 offsetDir = new Vector3(Mathf.Cos(offsetAngle), 0f, Mathf.Sin(offsetAngle)).normalized;

            // 2. length
            // 1D
            float noise1 = perlin.Noise(noiseParameter * m_centerNoiseSpan);
            float offsetLength = noise1 * m_centerNoiseScale * baseRadius;

            // the noise center
            Vector3 noiseCenter = offsetDir * offsetLength;

            // get collective noise radius
            // 1D
            // 1. length
            float noise2 = perlin.Noise(noiseParameter * m_rowNoiseSpan);
            float rowNoiseRadius = noise2 * m_rowNoiseScale * baseRadius;

            for (int i = 0; i < m_segment; ++i)
            {
                // get individual noise
                // 3D

                float noise3 = perlin.Noise(
                    Mathf.Cos(theta) * m_individualNoiseSpan,
                    noiseParameter * m_individualNoiseSpan,
                    Mathf.Sin(theta) * m_individualNoiseSpan);

                float individualNoiseRadius = noise3 * m_individualNoiseScale * baseRadius;

                float finalRadius = baseRadius + rowNoiseRadius + individualNoiseRadius;

                var noisePos = noiseCenter + new Vector3(finalRadius * Mathf.Cos(theta), 0f, finalRadius * Mathf.Sin(theta));

                float result = noisePos.magnitude;

                //Debug.Log(string.Format("noise {0:F3}\t finalRadius {1:F3}\t result {2:F3}",
                          //noise,
                          //finalRadius, result));

                // fill noise radius matrix
                cMesh.RadiusMatrix.Add(result);

                // add feature points for smoothing
                cMesh.IsFeaturePoints.Add((j == 0 || j == m_verticalSegment) ? true : false);

                theta += delta;
            }
            heightTheta += heightDelta;
        }
        return cMesh;
    }
}
