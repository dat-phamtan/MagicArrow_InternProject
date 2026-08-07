using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.UI;
using JetBrains.Annotations;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ArrowAssembler : MonoBehaviour
{
    public Material redHeadMaterial;
    public Material redBodyMaterial;
    public Material redTailMaterial;
    public Material blackHeadMaterial;
    public Material blackBodyMaterial;
    public Material blackTailMaterial;

    public Sprite head;
    public Sprite body;
    public Sprite tail;

    public GameObject arrowRayHint;
    private IEventHandler _eventHandler;


    public void Init(IEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
    }

    public GameObject Build(Arrow arrow, int boardWidth, int boardHeight, float spacing, out Vector3[] points, out ArrowMeshBuilder builder)
    {
        var root = new GameObject("Arrow");
        root.transform.SetParent(transform, false);

        points = BuildPathPoints(arrow.ArrowIndices, boardWidth, boardHeight, spacing);
        builder = null;

        if (points.Length < 2)
            return root;

        builder = root.AddComponent<ArrowMeshBuilder>();
        ChangeArrowColor(0, builder);

        builder.bodyThickness = body.rect.height / body.pixelsPerUnit;
        builder.bodyLength = body.rect.width / body.pixelsPerUnit;

        builder.headThickness = head.rect.height / head.pixelsPerUnit;
        builder.headLength = head.rect.width / head.pixelsPerUnit;

        builder.tailThickness = tail.rect.height / tail.pixelsPerUnit;
        builder.tailLength = tail.rect.width / tail.pixelsPerUnit;

        var arrowIndices = arrow.ArrowIndices;
        builder.BuildArrow(points, spacing);


        //boosters 
        //var rayHintInstance = Instantiate(arrowRayHint, root.transform);
        //var rect = rayHintInstance.GetComponent<RectTransform>();
        //rect.sizeDelta = new Vector2(body.rect.width, Mathf.Max(boardWidth, boardHeight));
        //rayHintInstance.transform.position = IndexToWorldPos(arrow.ArrowIndices[0], boardWidth, boardHeight, spacing);
        ////rayHintInstance.transform.
        //rayHintInstance.SetActive(false);

        return root;
    }

    public void ChangeArrowColor(int colorIndex, ArrowMeshBuilder builder)
    {
        if (colorIndex == 0)
        {
            builder.bodyMaterial = redBodyMaterial;
            builder.headMaterial = redHeadMaterial;
            builder.tailMaterial = redTailMaterial;
        }
        else if (colorIndex == 1)
        {
            builder.bodyMaterial = blackBodyMaterial;
            builder.headMaterial = blackHeadMaterial;
            builder.tailMaterial = blackTailMaterial;
        }
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