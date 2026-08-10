using Assets.Scripts.CoreLogic;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ArrowMeshBuilder : MonoBehaviour
{
    public Material headMaterial;
    public Material bodyMaterial;
    public Material tailMaterial;
    public Image arrowRayHint;
    public float cornerRadius = 0.12f;

    public float bodyThickness = 0.3f;
    public float bodyLength = 0.24f;
    public float headThickness = 0.5f;
    public float headLength = 0.5f;
    public float tailThickness = 0.5f;
    public float tailLength = 0.3f;

    private Mesh _mesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private const float CORNER_MULTIPLIER = 1.41421356f;
    private IController _controller;
    private readonly List<Vector3> _verticles = new(64);
    private readonly List<Vector2> _uvs = new(64);
    private readonly List<int> _headTris = new(12);
    private readonly List<int> _bodyTris = new(128);
    private readonly List<int> _tailTris = new(12);
    private readonly List<(int a, int b)> _rows = new(32);
    private readonly List<Vector3> _cornerScratch = new(32);
    private Vector3[] _normalsBuffer = new Vector3[0];

   

    private void Awake()
    { 
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _mesh = new Mesh { name = "Arrow" };
        _meshFilter.mesh = _mesh;
        
    }
    //NEED REFACTOR NOWWW
    public void BuildArrow(Vector3[] path, float[] cumulativeLength, float spacing)
    {
        int n = path.Length;
        if (n < 2) 
        { 
            _mesh.Clear(); 
            return; 
        }

        _verticles.Clear();
        _uvs.Clear();
        _headTris.Clear();
        _bodyTris.Clear();
        _tailTris.Clear();
        _rows.Clear();

        //head mesh
        Vector3 headDir = (path[1] - path[0]).normalized;
        Vector3 headNormal = new(-headDir.y, headDir.x, 0f);
        AddQuad(_verticles, _uvs, _headTris,
            back: path[0] - 0.5f * headLength * headDir,
            front: path[0] + 0.5f * headLength * headDir,
            normal: headNormal, 
            halfWidth: headThickness * 0.5f);

        //tail mesh
        int last = n - 1;
        Vector3 tailDir = (path[last] - path[last - 1]).normalized;
        Vector3 tailNormal = new(-tailDir.y, tailDir.x, 0f);
        AddQuad(_verticles, _uvs, _tailTris,
            back: path[last] - 0.5f * tailLength * tailDir,
            front: path[last] + 0.5f * tailLength * tailDir,
            normal: tailNormal, 
            halfWidth: tailThickness * 0.5f);

        //body mesh
        float halfThickness = bodyThickness / 2f;
        float accumulatedLength = 0f;

        for (int i = 0; i < n; i++)
        {
            Vector3 dirPrev = ((i > 0) ? (path[i] - path[i - 1]) : (path[1] - path[0])).normalized;
            Vector3 dirNext = ((i < n - 1) ? (path[i + 1] - path[i]) : dirPrev).normalized;

            Vector3 dirAvg = (dirPrev + dirNext).normalized;
            Vector3 normal = new(-dirAvg.y, dirAvg.x, 0f);

            int idx = _verticles.Count;
            _verticles.Add(path[i] + normal * halfThickness);
            _verticles.Add(path[i] - normal * halfThickness);

            _rows.Add((idx, idx + 1));

            if (i > 0)
                accumulatedLength += Vector3.Distance(path[i], path[i - 1]);

            float u = accumulatedLength / bodyLength;

            _uvs.Add(new Vector2(u, 1f));
            _uvs.Add(new Vector2(u, 0f));
        }


        for (int i = 0; i < _rows.Count - 1; i++)
        {
            var (topLeft, topRight) = _rows[i];
            var (bottomLeft, bottomRight) = _rows[i + 1];

            _bodyTris.Add(topLeft);
            _bodyTris.Add(bottomLeft);
            _bodyTris.Add(topRight);
            _bodyTris.Add(topRight);
            _bodyTris.Add(bottomLeft);
            _bodyTris.Add(bottomRight);
        }

        // mesh (3 submesh)
        _mesh.Clear();
        _mesh.SetVertices(_verticles);
        _mesh.SetUVs(0, _uvs);
        _mesh.subMeshCount = 3;
        
        _mesh.SetTriangles(_bodyTris, 1);
        _mesh.SetTriangles(_tailTris, 2);
        _mesh.SetTriangles(_headTris, 0);
        //_mesh.RecalculateNormals();
        if (_normalsBuffer.Length != _verticles.Count)
            _normalsBuffer = new Vector3[_verticles.Count];

        for (int i = 0; i < _normalsBuffer.Length; i++)
            _normalsBuffer[i] = Vector3.back;

        _mesh.SetNormals(_normalsBuffer);
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

    private Vector3 GetCornerCenter(Vector3 prePos, Vector3 cornerPos, Vector3 postPos, float radius)
    {
        var dir1 = (prePos - cornerPos).normalized;
        var dir2 = (postPos - cornerPos).normalized;

        var direction = (dir1 + dir2).normalized;
        var distanceToBaseCorner = radius * Mathf.Sqrt(2);
        
        var cornerCenterPos = cornerPos + direction * distanceToBaseCorner;
        return cornerCenterPos;
    }

    private List<Vector3> GenerateCornerVerticle(Vector3 cornerCenter, float innerRadius, float thickness, float startAngle, int segments, float turnSign)
    {
        float outerRadius = innerRadius + thickness;
        float endAngle = startAngle + turnSign * 90f;

        //var _cornerScratch = new List<Vector3>();
        _cornerScratch.Clear();
        for (int i = 0; i <= segments; i++)
        {
            float ratio = (float)i / segments;
            float currentAngle = Mathf.Lerp(startAngle, endAngle, ratio) * Mathf.Deg2Rad;

            float cos = Mathf.Cos(currentAngle);
            float sin = Mathf.Sin(currentAngle);

            float innerX = cornerCenter.x + innerRadius * cos;
            float innerY = cornerCenter.y + innerRadius * sin;
            _cornerScratch.Add(new Vector3(innerX, innerY, 0f));

            float outerX = cornerCenter.x + outerRadius * cos;
            float outerY = cornerCenter.y + outerRadius * sin;
            _cornerScratch.Add(new Vector3(outerX, outerY, 0f));
        }
        return _cornerScratch;
    }
}