using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DigiClay;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour
{
	public static MeshGenerator Instance;

    [SerializeField, Range(DigiClayConstant.MIN_RADIUS, DigiClayConstant.MAX_RADIUS)]
    float m_radius = 1;
    [SerializeField, Range(DigiClayConstant.MIN_HEIGHT, DigiClayConstant.MAX_HEIGHT)]
    float m_height = 1;
    [SerializeField, Range(3, 360)]
    int m_segment = 8;
    [SerializeField, Range(1, 1000)]
    int m_verticalSegment = 10;
    [SerializeField, Range(0f, 1f)]
    float m_thicknessRatio = 0.5f;
    // noise parameters
    [SerializeField, Range(0f, 1f)]
    float m_topBaseRatio = 0.5f;

    // all scale are percentage
    [SerializeField, Range(0f, 1f)]
    float m_centerNoiseScale = 0.02f;
    [SerializeField, Range(0f, 1f)]
    float m_rowNoiseScale = 0.02f;
    [SerializeField, Range(0f, 1f)]
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
    ClayMeshContext m_prefab = null;

    [HideInInspector]
    public int Vertices;
    [HideInInspector]
    public int Triangles;

    public Transform m_clayRoot;

    public int Segment
    {
        get
        {
            return m_segment;
        }
    }

    public int VerticalSegment
    {
        get
        {
            return m_verticalSegment;
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

    public void CreateMesh()
    {
        // cleanup first
        for (int i = 0; i < m_clayRoot.childCount; ++i)
            DestroyImmediate(m_clayRoot.GetChild(i).gameObject);

        // create ClayMesh
        ClayMesh clayMesh = ClayMeshFactory();

        // assign ClayMesh
        ClayMeshContext cmc = Instantiate<ClayMeshContext>(m_prefab, m_clayRoot);
        cmc.gameObject.name = "Clay Mesh " + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        cmc.clayMesh = clayMesh;

        // generate mesh based on ClayMesh
        clayMesh.GenerateMesh();

        Vertices = clayMesh.Mesh.vertexCount;
        Triangles = clayMesh.Mesh.triangles.Length / 3;
    }

    ClayMesh ClayMeshFactory()
    {
        float delta = 2f * Mathf.PI / (float)m_segment;
        float heightDelta = (float)m_height / (float)m_verticalSegment;
        float theta = 0f;
        float heightTheta = 0f;
        Perlin perlin = new Perlin();
		List<float> radiusList = new List<float> ();
        float maxRadius = 0f;

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
            float offsetLength = noise1 * m_centerNoiseScale * m_radius;

            // the noise center
            Vector3 noiseCenter = offsetDir * offsetLength;

            // get collective noise radius
            // 1D
            // 1. length
            float noise2 = perlin.Noise(noiseParameter * m_rowNoiseSpan);
            float rowNoiseRadius = noise2 * m_rowNoiseScale * m_radius;

            for (int i = 0; i < m_segment; ++i)
            {
                // get individual noise
                // 3D

                float noise3 = perlin.Noise(
                    Mathf.Cos(theta) * m_radius * m_individualNoiseSpan,
                    heightTheta,
                    Mathf.Sin(theta) * m_radius * m_individualNoiseSpan);

                float individualNoiseRadius = noise3 * m_individualNoiseScale * m_radius;
                float finalRadius = baseRadius + rowNoiseRadius + individualNoiseRadius;

                var noisePos = noiseCenter + new Vector3(finalRadius * Mathf.Cos(theta), 0f, finalRadius * Mathf.Sin(theta));
                float result = noisePos.magnitude;

                if (result > maxRadius)
                    maxRadius = result;

                // fill noise radius matrix
				radiusList.Add(result);

                theta += delta;
            }
            heightTheta += heightDelta;
        }

		ClayMesh cMesh = new ClayMesh(m_verticalSegment + 1, m_segment, radiusList, m_height, m_thicknessRatio);

        return cMesh;
    }
}
