using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ArrowMeshBuilder : MonoBehaviour
{
    public Material headMaterial;
    public Material bodyMaterial;
    public Material tailMaterial;

    public float bodyWidth = 0.3f;
    public float bodyTileLength = 0.24f;
    public float headWidth = 0.5f;
    public float headLength = 0.5f;
    public float tailWidth = 0.5f;
    public float tailLength = 0.3f;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private const float CORNER_MULTIPLIER = 1.41421356f;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _mesh = new Mesh { name = "Arrow" };
        _meshFilter.mesh = _mesh;
    }

    public void BuildArrow(Vector3[] path)
    {
        int n = path.Length;
        if (n < 2) 
        { 
            _mesh.Clear(); 
            return; 
        }

        var vertices = new List<Vector3>();
        var uvs = new List<Vector2>();
        var headTris = new List<int>();
        var bodyTris = new List<int>();
        var tailTris = new List<int>();

        // head 
        Vector3 headDir = (path[1] - path[0]).normalized;
        Vector3 headNormal = new(-headDir.y, headDir.x, 0f);
        AddQuad(vertices, uvs, headTris,
            back: path[0] - 0.5f * headLength * headDir,
            front: path[0] + 0.5f * headLength * headDir,
            normal: headNormal, halfWidth: headWidth * 0.5f);

        // tail
        int last = n - 1;
        Vector3 tailDir = (path[last] - path[last - 1]).normalized;
        Vector3 tailNormal = new(-tailDir.y, tailDir.x, 0f);
        AddQuad(vertices, uvs, tailTris,
            back: path[last] - 0.5f * tailLength * tailDir,
            front: path[last] + 0.5f * tailLength * tailDir,
            normal: tailNormal, halfWidth: tailWidth * 0.5f);

        // body
        int bodyBase = vertices.Count;
        float halfWidth = bodyWidth / 2f;
        float accumulatedLength = 0f;

        for (int i = 0; i < n; i++)
        {
            Vector3 dirPrev = (i > 0) ? (path[i] - path[i - 1]).normalized : (path[1] - path[0]).normalized;
            Vector3 dirNext = (i < n - 1) ? (path[i + 1] - path[i]).normalized : dirPrev;
            bool isCorner = Vector3.Dot(dirPrev, dirNext) < 0.99f;
            Vector3 dirAvg = (dirPrev + dirNext).normalized;
            if (dirAvg == Vector3.zero) dirAvg = dirPrev;
            Vector3 normal = new(-dirAvg.y, dirAvg.x, 0f);
            float miterLength = isCorner ? halfWidth * CORNER_MULTIPLIER : halfWidth;

            vertices.Add(path[i] + normal * miterLength);
            vertices.Add(path[i] - normal * miterLength);

            if (i > 0) accumulatedLength += Vector3.Distance(path[i], path[i - 1]);
            float u = accumulatedLength / Mathf.Max(bodyTileLength, 0.001f);
            uvs.Add(new Vector2(u, 1f));
            uvs.Add(new Vector2(u, 0f));
        }


        for (int i = 0; i < n - 1; i++)
        {
            int topLeft = bodyBase + i * 2;
            int topRight = bodyBase + i * 2 + 1;
            int bottomLeft = bodyBase + (i + 1) * 2;
            int bottomRight = bodyBase + (i + 1) * 2 + 1;

            bodyTris.Add(topLeft); 
            bodyTris.Add(bottomLeft); 
            bodyTris.Add(topRight);
            bodyTris.Add(topRight); 
            bodyTris.Add(bottomLeft); 
            bodyTris.Add(bottomRight);
        }

        // mesh (3 submesh)
        _mesh.Clear();
        _mesh.SetVertices(vertices);
        _mesh.SetUVs(0, uvs);
        _mesh.subMeshCount = 3;
        
        _mesh.SetTriangles(bodyTris, 1);
        _mesh.SetTriangles(tailTris, 2);
        _mesh.SetTriangles(headTris, 0);
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshRenderer.materials = new[] { headMaterial, bodyMaterial, tailMaterial };
    }

    private void AddQuad(List<Vector3> vertices, List<Vector2> uvs, List<int> tris, Vector3 back, Vector3 front, Vector3 normal, float halfWidth)
    {
        int b = vertices.Count;
        vertices.Add(back + normal * halfWidth); 
        uvs.Add(new Vector2(0, 1));
        vertices.Add(back - normal * halfWidth); 
        uvs.Add(new Vector2(0, 0));
        vertices.Add(front - normal * halfWidth); 
        uvs.Add(new Vector2(1, 0));
        vertices.Add(front + normal * halfWidth); 
        uvs.Add(new Vector2(1, 1));

        tris.Add(b); 
        tris.Add(b + 2); 
        tris.Add(b + 1);

        tris.Add(b); 
        tris.Add(b + 3); 
        tris.Add(b + 2);
    }
}