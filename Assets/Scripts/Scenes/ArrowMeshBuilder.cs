using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ArrowMeshBuilder : MonoBehaviour
{
    public Material arrowMaterial;
    public float bodyTileLength = 0.24f;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private const float CORNER_MULTIPLIER = 1.41421356f;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;
        GetComponent<MeshRenderer>().material = arrowMaterial;
    }

    private Vector3 IndexToWorldPos(int index, int boardWidth, int boardHeight, float spacing)
    {
        int x = index % boardWidth;
        int y = index / boardWidth;
        float offsetX = -(boardWidth - 1) * spacing / 2f;
        float offsetY = -(boardHeight - 1) * spacing / 2f;
        return new Vector3(offsetX + x * spacing, offsetY + y * spacing, 0);
    }

    public Vector3[] BuildPathPoints(int[] arrowIndices, int boardWidth, int boardHeight, float spacing)
    {
        var points = new Vector3[arrowIndices.Length];
        for (int i = 0; i < arrowIndices.Length; i++)
            points[i] = IndexToWorldPos(arrowIndices[i], boardWidth, boardHeight, spacing);
        return points;
    }

    public void BuildBodyMesh(Vector3[] path, float width)
    {
        Debug.Log(path);
        int n = path.Length;
        if (n < 2)
        {
            _mesh.Clear();
            return;
        }

        var vertices = new Vector3[n * 2];
        var uvs = new Vector2[n * 2];
        var triangles = new List<int>();

        float halfWidth = width / 2f;
        float accumulatedLength = 0f;

        for (int i = 0; i < n; i++)
        {
            Vector3 dirPrev = (i > 0) ? (path[i] - path[i - 1]).normalized : (path[1] - path[0]).normalized;
            Vector3 dirNext = (i < n - 1) ? (path[i + 1] - path[i]).normalized : dirPrev;

            bool isCorner = Vector3.Dot(dirPrev, dirNext) < 0.99f;
            Vector3 dirAvg = (dirPrev + dirNext).normalized;
            if (dirAvg == Vector3.zero) dirAvg = dirPrev;

            Vector3 normal = new Vector3(-dirAvg.y, dirAvg.x, 0f);

            float miterLength = isCorner ? halfWidth * CORNER_MULTIPLIER : halfWidth;

            vertices[i * 2] = path[i] + normal * miterLength;
            vertices[i * 2 + 1] = path[i] - normal * miterLength;

            if (i > 0) accumulatedLength += Vector3.Distance(path[i], path[i - 1]);

            float u = accumulatedLength / Mathf.Max(bodyTileLength, 0.001f);
            uvs[i * 2] = new Vector2(u, 0f);
            uvs[i * 2 + 1] = new Vector2(u, 1f);
        }

        for (int i = 0; i < n - 1; i++)
        {
            int topLeft = i * 2;
            int topRight = i * 2 + 1;
            int bottomLeft = (i + 1) * 2;
            int bottomRight = (i + 1) * 2 + 1;

            triangles.Add(topLeft);
            triangles.Add(bottomLeft);
            triangles.Add(topRight);

            triangles.Add(topRight);
            triangles.Add(bottomLeft);
            triangles.Add(bottomRight);
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.uv = uvs;
        _mesh.triangles = triangles.ToArray();
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }
}