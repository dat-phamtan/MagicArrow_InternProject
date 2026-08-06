using Assets.Scripts.CoreLogic;
using Assets.Scripts.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ArrowMeshBuilder : MonoBehaviour
{
    public Material headMaterial;
    public Material bodyMaterial;
    public Material tailMaterial;
    public Image arrowRayHint;

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
    private IController _controller;

    private void Awake()
    { 
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _mesh = new Mesh { name = "Arrow" };
        _meshFilter.mesh = _mesh;
        
    }

    public void BuildArrow(IController controller, int[] arrowIndices, Vector3[] path, float spacing)
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
        var rows = new List<(int a, int b)>();

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
            Vector3 dirPrev = SnapToAxis((i > 0) ? (path[i] - path[i - 1]) : (path[1] - path[0]));
            Vector3 dirNext = SnapToAxis((i < n - 1) ? (path[i + 1] - path[i]) : dirPrev);
            bool isCorner = dirPrev != dirNext;

            if (isCorner)
            {
                var cornerCenter = GetCornerCenter(path[i - 1], path[i], path[i + 1], spacing);
                float innerRadius = spacing - bodyWidth / 2f;
                float startAngle = GetStartAngle(controller.GetDirectionAtBoardIndex(arrowIndices[i]));
                
                var cornerVerticles = GenerateCornerVerticle(cornerCenter, innerRadius, bodyWidth, startAngle, 10);
                for (int j = 0; j < cornerVerticles.Count; j += 2)
                {
                    int idx = vertices.Count;
                    vertices.Add(cornerVerticles[j]);
                    vertices.Add(cornerVerticles[j + 1]);
                    rows.Add((idx, idx + 1));

                    if (i > 0) accumulatedLength += Vector3.Distance(path[i], path[i - 1]);

                    float u = accumulatedLength / Mathf.Max(bodyTileLength, 0.001f);
                    uvs.Add(new Vector2(u, 1f));
                    uvs.Add(new Vector2(u, 0f));
                }
            }
            else
            {
                Vector3 dirAvg = (dirPrev + dirNext).normalized;
                if (dirAvg == Vector3.zero) dirAvg = dirPrev;
                Vector3 normal = new(-dirAvg.y, dirAvg.x, 0f);

                int idx = vertices.Count;
                vertices.Add(path[i] + normal * halfWidth);
                vertices.Add(path[i] - normal * halfWidth);
                rows.Add((idx, idx + 1));

                if (i > 0) accumulatedLength += Vector3.Distance(path[i], path[i - 1]);

                float u = accumulatedLength / Mathf.Max(bodyTileLength, 0.001f);
                uvs.Add(new Vector2(u, 1f));
                uvs.Add(new Vector2(u, 0f));
            }
        }


        for (int r = 0; r < rows.Count - 1; r++)
        {
            var (topLeft, topRight) = rows[r];
            var (bottomLeft, bottomRight) = rows[r + 1];

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

    private float GetStartAngle(Direction direction)
    {
        switch (direction)
        {
            case Direction.LEFTDOWN:
                return 270;
            case Direction.RIGHTDOWN:
                return 180;
            case Direction.LEFTUP:
                return 0;
            case Direction.RIGHTUP:
                return 90;
            default:
                return 0;
        }
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

    private Vector3 SnapToAxis(Vector3 dir)
    {
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            return new Vector3(Mathf.Sign(dir.x), 0, 0);
        return new Vector3(0, Mathf.Sign(dir.y), 0);
    }

    private Vector3 GetCornerCenter(Vector3 prePos, Vector3 cornerPos, Vector3 postPos, float radius)
    {
        var dir1 = (prePos - cornerPos).normalized;
        var dir2 = (postPos - cornerPos).normalized;

        var direction = (dir1 + dir2).normalized;
        var distanceToBaseCorner = radius * Mathf.Sqrt(2);
        
        var cornerCenterPos = cornerPos + direction * distanceToBaseCorner;
        return cornerCenterPos;
    }

    private List<Vector3> GenerateCornerVerticle(Vector3 cornerCenter, float innerRadius, float thickness, float startAngle, int segments)
    {
        float outerRadius = innerRadius + thickness;
        float endAngle = startAngle + 90f;

        var result = new List<Vector3>();
        for (int i = 0; i < segments; i++)
        {
            float ratio = (float)i / segments;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, ratio) * Mathf.Deg2Rad;

            float cos = Mathf.Cos(currentAngle);
            float sin = Mathf.Sin(currentAngle);

            float innerX = cornerCenter.x + innerRadius * cos;
            float innerY = cornerCenter.y + innerRadius * sin;
            result.Add(new Vector3(innerX, innerY, 0f));

            float outerX = cornerCenter.x + outerRadius * cos;
            float outerY = cornerCenter.y + outerRadius * sin;
            result.Add(new Vector3(outerX, outerY, 0f));
        }
        return result;
    }
}