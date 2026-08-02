using Assets.Scripts.Data;
using System.Collections.Generic;
using UnityEngine;

public class ArrowMeshBuilder : MonoBehaviour
{
    public float width = 0.3f;
    public Material arrowMaterial;

    private Mesh _mesh;
    private MeshFilter _meshFilter;

    //private List<Verticle> _verticles;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;
        GetComponent<MeshRenderer>().material = arrowMaterial;
    }

    private Vector3 IndexToWorldPos(int index, int boardWidth, float spacing)
    {
        int x = index % boardWidth;
        int y = index / boardWidth;
        return new Vector3(x * spacing, y * spacing, 0);
    }

    public Vector3[] BuildPathPoints(int[] arrowIndices, int boardWidth, float spacing)
    {
        var points = new Vector3[arrowIndices.Length];
        for (int i = 0; i < arrowIndices.Length; i++)
        {
            points[i] = IndexToWorldPos(arrowIndices[i], boardWidth, spacing);
        }
        return points;
    }

    public void BuildArrowMesh(Vector3[] path, float width)
    {
        int n = path.Length;
        var verticles = new Vector3[n * 2];
        var uvs = new Vector2[n * 2];
        var triangles = new List<int>();

        float halfWidth = width / 2f;
        float totalLength = ComputePathLength(path);
        float accumulatedLength = 0f;

        for (int i = 0; i < n; i++)
        {
            Vector3 dirPrev = (i > 0) ? Vector3.Normalize(path[i] - path[i - 1]) : Vector3.Normalize(path[1] - path[0]);
            Vector3 dirNext = (i < n - 1) ? Vector3.Normalize(path[i + 1] - path[i]) : dirPrev;

            Vector3 dirAvg = Vector3.Normalize(dirPrev + dirNext);
            Vector3 normal = new(-dirAvg.y, dirAvg.x, 0f);

            float miterCos = Vector3.Dot(normal, new Vector3(-dirNext.y, dirNext.x, 0f));
            float miterLength = halfWidth / Mathf.Max(miterCos, 0.5f);

            verticles[i * 2] = path[i] + normal * miterLength;
            verticles[i * 2 + 1] = path[i] - normal * miterLength;

            if (i > 0) accumulatedLength += Vector3.Distance(path[i], path[i - 1]);
            float u = accumulatedLength / totalLength;
            uvs[i * 2] = new Vector2(u, 0f);
            uvs[i * 2 + 1] = new Vector2(u, 1f);
        }

        BuildTriangles(n, triangles);

        _mesh.Clear();
        _mesh.vertices = verticles;
        _mesh.uv = uvs;
        _mesh.triangles = triangles.ToArray();
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();
    }

    private float ComputePathLength(Vector3[] path)
    {
        float length = 0f;
        for (int i = 1; i < path.Length; i++)
        {
            length += Vector3.Distance(path[i], path[i - 1]);
        }
        return length;
    }

    private void BuildTriangles(int pointCount, List<int> triangles)
    {
        for (int i = 0; i < pointCount; i++)
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
    }
}
