using UnityEngine;

public class Example : MonoBehaviour
{
    [SerializeField] Vector3[] newVertices;
    [SerializeField] Vector2[] newUV;
    [SerializeField] int[] newTriangles;

    void Start()
    {
        // Create a new Mesh and set its data properties (vertices, UV coordinates, and triangles).
        Mesh mesh = new Mesh
        {
            vertices = newVertices,
            uv = newUV,
            triangles = newTriangles
        };

        // After updating the Mesh data, recalculate the normals. 
        // If the Mesh uses shaders with normal maps, also call RecalculateTangents for proper lighting.
        mesh.RecalculateNormals();

        // This assignment is temporary and will reset to the initial Mesh when exiting Play mode.
        GetComponent<MeshFilter>().mesh = mesh;
    }
}