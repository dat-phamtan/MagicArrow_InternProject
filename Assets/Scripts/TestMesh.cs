using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TestAnimateMesh : MonoBehaviour
{
    Mesh m_Mesh;

    readonly List<Vector3> m_InitialVertices = new List<Vector3>();
    readonly List<Vector3> m_InitialNormals = new List<Vector3>();
    readonly List<Vector3> m_AnimatedVertices = new List<Vector3>();

    [Header("Cấu hình Animation")]
    [Tooltip("Độ phồng tối đa theo hướng pháp tuyến")]
    public float amplitude = 0.5f;
    [Tooltip("Tốc độ chuyển động")]
    public float speed = 3f;

    void Start()
    {
        // 1. Khởi tạo Mesh mới và gán vào MeshFilter
        m_Mesh = new Mesh();
        m_Mesh.name = "Procedural Animated Cube";
        GetComponent<MeshFilter>().mesh = m_Mesh;

        // 2. Gán Material mặc định nếu GameObject chưa có Material
        var renderer = GetComponent<MeshRenderer>();
        if (renderer.sharedMaterial == null)
        {
            // Dùng shader mặc định của Unity để có thể nhìn thấy ngay
            renderer.sharedMaterial = new Material(Shader.Find("Standard"));
        }

        // 3. Tạo toàn bộ dữ liệu đỉnh, pháp tuyến và tam giác cho Cube
        GenerateCubeMesh();

        // 4. Cache lại dữ liệu ban đầu
        m_Mesh.GetVertices(m_InitialVertices);
        m_Mesh.GetNormals(m_InitialNormals);
        m_AnimatedVertices.AddRange(m_InitialVertices);
    }

    void Update()
    {
        // Animate các đỉnh di chuyển theo hướng của Vector Normal
        float offset = Mathf.Abs(Mathf.Sin(Time.time * speed)) * amplitude;

        for (var i = 0; i < m_InitialVertices.Count; i++)
        {
            m_AnimatedVertices[i] = m_InitialVertices[i] + m_InitialNormals[i] * offset;
        }

        // Cập nhật lại vị trí đỉnh vào Mesh
        m_Mesh.SetVertices(m_AnimatedVertices);
        m_Mesh.RecalculateBounds(); // Cập nhật bounds để không bị lỗi culling
    }

    /// <summary>
    /// Tạo dữ liệu 24 đỉnh của Cube (mỗi mặt 4 đỉnh riêng biệt để có Normal độc lập)
    /// </summary>
    void GenerateCubeMesh()
    {
        Vector3[] vertices = new Vector3[24]
        {
            // Mặt trước (Front)
            new Vector3(-0.5f, -0.5f,  0.5f), new Vector3( 0.5f, -0.5f,  0.5f),
            new Vector3( 0.5f,  0.5f,  0.5f), new Vector3(-0.5f,  0.5f,  0.5f),
            // Mặt sau (Back)
            new Vector3( 0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
            new Vector3(-0.5f,  0.5f, -0.5f), new Vector3( 0.5f,  0.5f, -0.5f),
            // Mặt trên (Top)
            new Vector3(-0.5f,  0.5f,  0.5f), new Vector3( 0.5f,  0.5f,  0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f), new Vector3(-0.5f,  0.5f, -0.5f),
            // Mặt dưới (Bottom)
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f, -0.5f,  0.5f), new Vector3(-0.5f, -0.5f,  0.5f),
            // Mặt trái (Left)
            new Vector3(-0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f,  0.5f),
            new Vector3(-0.5f,  0.5f,  0.5f), new Vector3(-0.5f,  0.5f, -0.5f),
            // Mặt phải (Right)
            new Vector3( 0.5f, -0.5f,  0.5f), new Vector3( 0.5f, -0.5f, -0.5f),
            new Vector3( 0.5f,  0.5f, -0.5f), new Vector3( 0.5f,  0.5f,  0.5f)
        };

        // Pháp tuyến chỉ hướng vuông góc ra ngoài cho từng mặt
        Vector3[] normals = new Vector3[24]
        {
            Vector3.forward, Vector3.forward, Vector3.forward, Vector3.forward, // Front
            Vector3.back,    Vector3.back,    Vector3.back,    Vector3.back,    // Back
            Vector3.up,      Vector3.up,      Vector3.up,      Vector3.up,      // Top
            Vector3.down,    Vector3.down,    Vector3.down,    Vector3.down,    // Bottom
            Vector3.left,    Vector3.left,    Vector3.left,    Vector3.left,    // Left
            Vector3.right,   Vector3.right,   Vector3.right,   Vector3.right    // Right
        };

        // Thứ tự các đỉnh để tạo thành 12 tam giác (6 hình chữ nhật)
        int[] triangles = new int[36]
        {
             0,  2,  1,  0,  3,  2, // Front
             4,  6,  5,  4,  7,  6, // Back
             8, 10,  9,  8, 11, 10, // Top
            12, 14, 13, 12, 15, 14, // Bottom
            16, 18, 17, 16, 19, 18, // Left
            20, 22, 21, 20, 23, 22  // Right
        };

        m_Mesh.vertices = vertices;
        m_Mesh.normals = normals;
        m_Mesh.triangles = triangles;
    }
}