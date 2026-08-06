using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class ArrowAssembler : MonoBehaviour
{
    public Material headMaterial;
    public Material bodyMaterial;
    public Material tailMaterial;

    public Sprite head;
    public Sprite body;
    public Sprite tail;

    public GameObject arrowRayHint;

    public GameObject Build(IController controller, Arrow arrow, int boardWidth, int boardHeight, float spacing, out Vector3[] points, out ArrowMeshBuilder builder)
    {
        var root = new GameObject("Arrow");
        root.transform.SetParent(transform, false);

        points = BuildPathPoints(arrow.ArrowIndices, boardWidth, boardHeight, spacing);
        builder = null;

        if (points.Length < 2)
            return root;

        builder = root.AddComponent<ArrowMeshBuilder>();
        builder.headMaterial = headMaterial;
        builder.bodyMaterial = bodyMaterial;
        builder.tailMaterial = tailMaterial;

        builder.bodyWidth = body.rect.height / body.pixelsPerUnit;
        builder.bodyTileLength = body.rect.width / body.pixelsPerUnit;

        builder.headWidth = head.rect.height / head.pixelsPerUnit;
        builder.headLength = head.rect.width / head.pixelsPerUnit;

        builder.tailWidth = tail.rect.height / tail.pixelsPerUnit;
        builder.tailLength = tail.rect.width / tail.pixelsPerUnit;

        var arrowIndices = arrow.ArrowIndices;
        builder.BuildArrow(controller, arrowIndices, points, spacing);


        //boosters 
        //var rayHintInstance = Instantiate(arrowRayHint, root.transform);
        //var rect = rayHintInstance.GetComponent<RectTransform>();
        //rect.sizeDelta = new Vector2(body.rect.width, Mathf.Max(boardWidth, boardHeight));
        //rayHintInstance.transform.position = IndexToWorldPos(arrow.ArrowIndices[0], boardWidth, boardHeight, spacing);
        ////rayHintInstance.transform.
        //rayHintInstance.SetActive(false);

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