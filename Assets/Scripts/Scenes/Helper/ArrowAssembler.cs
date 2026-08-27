using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.UI;
using JetBrains.Annotations;
using System;
using System.Collections;
using Unity.Mathematics;
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
    public Material purpleHeadMaterial;
    public Material purpleBodyMaterial;
    public Material purpleTailMaterial;
    public Material brownHeadMaterial;
    public Material brownBodyMaterial;
    public Material brownTailMaterial;


    public Sprite head;
    public Sprite body;
    public Sprite tail;
    public int numColor = 3;

    public GameObject arrowRayHint;
    private int _currentIndex = 0;
    private System.Random _random = new();
    private IEventHandler _eventHandler;


    public void Init(IEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
        _eventHandler.OnCollidedAnimation += HandleCollidedAnimation;
    }

    private void HandleCollidedAnimation(GameObject @object)
    {
        //StartCoroutine(PlayCollidedAnimation(@object));
    }


    public GameObject Build(Arrow arrow, Vector3[] points, float[] cumulativeLength, float spacing, out ArrowMeshBuilder builder)
    {
        var root = new GameObject("Arrow");
        root.transform.SetParent(transform, false);
        builder = null;

        if (points.Length < 2)
            return root;

        builder = root.AddComponent<ArrowMeshBuilder>();

        SetArrowColor(GetColorIndex(), builder);

        builder.bodyThickness = body.rect.height / body.pixelsPerUnit;
        builder.bodyLength = body.rect.width / body.pixelsPerUnit;

        builder.headThickness = head.rect.height / head.pixelsPerUnit;
        builder.headLength = head.rect.width / head.pixelsPerUnit;

        builder.tailThickness = tail.rect.height / tail.pixelsPerUnit;
        builder.tailLength = tail.rect.width / tail.pixelsPerUnit;

        var arrowIndices = arrow.ArrowIndices;
        builder.BuildArrow(points, cumulativeLength, spacing);


        //boosters 
        //var rayHintInstance = Instantiate(arrowRayHint, root.transform);
        //var rect = rayHintInstance.GetComponent<RectTransform>();
        //rect.sizeDelta = new Vector2(body.rect.width, Mathf.Max(boardWidth, boardHeight));
        //rayHintInstance.transform.position = IndexToWorldPos(arrow.ArrowIndices[0], boardWidth, boardHeight, spacing);
        ////rayHintInstance.transform.
        //rayHintInstance.SetActive(false);

        return root;
    }

    private int GetColorIndex()
    {
        if (_currentIndex > numColor - 1)
            _currentIndex = 0;
        return _currentIndex++;
    }

    private void SetArrowColor(int index, ArrowMeshBuilder builder)
    {
        switch (index)
        {
            case 0:
                builder.bodyMaterial = redBodyMaterial;
                builder.headMaterial = redHeadMaterial;
                builder.tailMaterial = redTailMaterial;
                break;
            case 1:
                builder.bodyMaterial = purpleBodyMaterial;
                builder.headMaterial = purpleHeadMaterial;
                builder.tailMaterial = purpleTailMaterial;
                break;
            case 2:
                builder.bodyMaterial = brownBodyMaterial;
                builder.headMaterial = brownHeadMaterial;
                builder.tailMaterial = brownTailMaterial;
                break;
        }
    }

    public void ChangeArrowColor(int colorIndex, ArrowMeshBuilder builder)
    {
        switch (colorIndex)
        {
            case 0:
                builder.bodyMaterial = redBodyMaterial;
                builder.headMaterial = redHeadMaterial;
                builder.tailMaterial = redTailMaterial;
                break;
            case 1:
                builder.bodyMaterial = blackBodyMaterial;
                builder.headMaterial = blackHeadMaterial;
                builder.tailMaterial = blackTailMaterial;
                break;   
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

    public Vector3[] BuildPathPoints(int[] arrowIndices, int boardWidth, int boardHeight, float spacing)
    {
        var points = new Vector3[arrowIndices.Length];
        for (int i = 0; i < arrowIndices.Length; i++)
            points[i] = IndexToWorldPos(arrowIndices[i], boardWidth, boardHeight, spacing);
        return points;
    }
}