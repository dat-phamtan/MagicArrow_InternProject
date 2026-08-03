using Assets.Scripts.Data;
using UnityEngine;

public class ArrowAssembler : MonoBehaviour
{
    [Header("Prefabs (sprite quad hoặc GameObject có SpriteRenderer)")]
    public GameObject headPrefab;
    public GameObject bodyAnchorPrefab; 
    public GameObject tailPrefab;

    [Header("Material & kích thước")]
    public Material bodyMaterial;
    public float width = 0.3f;
    public float bodyTileLength = 0.5f;

    public Vector3 baseFacing = Vector3.left;

    public GameObject Build(Arrow arrow, int boardWidth, int boardHeight, float spacing)
    {
        var root = new GameObject("Arrow");
        root.transform.SetParent(transform, false);

        var points = BuildPathPoints(arrow.ArrowIndices, boardWidth, boardHeight, spacing);
        if (points.Length < 2)
            return root;

        Vector3 headPos = points[0];
        Vector3 headDir = (points[0] - points[1]).normalized;
        var headGo = Instantiate(headPrefab, headPos, Quaternion.identity, root.transform);
        headGo.transform.rotation = Quaternion.FromToRotation(baseFacing, headDir);

        int last = points.Length - 1;
        Vector3 tailPos = points[last];
        Vector3 tailDir = (points[last] - points[last - 1]).normalized;
        var tailGo = Instantiate(tailPrefab, tailPos, Quaternion.identity, root.transform);
        tailGo.transform.rotation = Quaternion.FromToRotation(baseFacing, tailDir);

        GameObject bodyGo = (bodyAnchorPrefab != null) ? Instantiate(bodyAnchorPrefab, root.transform) : new GameObject("ArrowBody");

        if (bodyAnchorPrefab == null)
            bodyGo.transform.SetParent(root.transform, false);

        var builder = bodyGo.GetComponent<ArrowMeshBuilder>();
        if (builder == null)
            builder = bodyGo.AddComponent<ArrowMeshBuilder>();

        builder.arrowMaterial = bodyMaterial;
        builder.bodyTileLength = bodyTileLength;
        builder.BuildBodyMesh(points, width);

        return root;
    }

    private Vector3 IndexToWorldPos(int index, int boardWidth, int boardHeight, float spacing)
    {
        int x = index % boardWidth;
        int y = index / boardWidth;
        float offsetX = -(boardWidth - 1) * spacing / 2f;
        float offsetY = -(boardHeight - 1) * spacing / 2f;
        return new Vector3(offsetX + x * spacing, offsetY + y * spacing, 0);
    }

    private Vector3[] BuildPathPoints(int[] arrowIndices, int boardWidth, int boardHeight, float spacing)
    {
        var points = new Vector3[arrowIndices.Length];
        for (int i = 0; i < arrowIndices.Length; i++)
            points[i] = IndexToWorldPos(arrowIndices[i], boardWidth, boardHeight, spacing);
        return points;
    }
}