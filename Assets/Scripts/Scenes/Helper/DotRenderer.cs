using Assets.Scripts.CoreLogic;
using Assets.Scripts.Ultility;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DotRenderer : MonoBehaviour
{
    public float spacing = 1f;
    public Tilemap targetTilemap;
    public Color baseColor = Color.gray;
    public Color activeColor = Color.lightBlue;
    
    public float baseScale = 0.1f;
    public float activeScale = 0.2f;
    public float animationDuration = 0.5f;
    public float chainStepDelay = 0.11f;

    private IController _controller;
    private Tile _dotTile;
    private readonly Dictionary<Vector3Int, float> _currentScale = new();
    private readonly Dictionary<Vector3Int, Color> _currentColor = new();
    //private 

    private void Start()
    {
        _dotTile = CreateCircleTile(32);
        _controller = Locator.Get<IController>();        

        _controller.OnMoveArrowSuccess += HandleSpawnDots;
        _controller.OnRerenderBoard += ResetTileMap;
    }

    private void ResetTileMap()
    {
        targetTilemap.ClearAllTiles();
    }

    public void TriggerEffect(Vector3Int pos, Vector3 offset)
    {
        StartCoroutine(AnimateDot( pos, offset));
    }

    private IEnumerator AnimateDot(Vector3Int pos, Vector3 offset)
    {
        targetTilemap.SetTileFlags(pos, TileFlags.None);

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {

            float t = elapsedTime / animationDuration;

            Color currentColor = Color.Lerp(activeColor, baseColor, t);
            float currentScale = Mathf.Lerp(activeScale, baseScale, t);

            targetTilemap.SetColor(pos, currentColor);

            Matrix4x4 matrix = Matrix4x4.Scale(new Vector3(currentScale, currentScale, 1f));
            //Matrix4x4 matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(currentScale, currentScale, 1f));
            targetTilemap.SetTransformMatrix(pos, matrix);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        SetupBaseTile(pos, offset);
    }

    private void OnDisable()
    {
        _controller.OnMoveArrowSuccess -= HandleSpawnDots;
    }

    private void HandleSpawnDots(int interactedConfigIndex)
    {
        int width = _controller.GetConfigData().BoardWidth;
        int height = _controller.GetConfigData().BoardHeight;
        var interactdArrow = _controller.GetConfigData().Arrows[interactedConfigIndex];

        var chain = new List<(Vector3Int pos, Vector3 offset, bool needsTile)>();

        // tail -> head
        for (int i = interactdArrow.ArrowIndices.Length - 1; i >= 0; i--)
        {
            var worldPos = PositionConverter.IndexToWorldPos(interactdArrow.ArrowIndices[i], width, height, spacing);
            var intWorldPos = new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.z));
            var offset = worldPos - intWorldPos;
            chain.Add((intWorldPos, offset, true));
        }

        // head -> off board
        var cellList = _controller.GetNextCells(interactdArrow.YArrowHead, interactdArrow.XArrowHead, _controller.GetDirectionAtBoardIndex(interactdArrow.ArrowIndices[0]));
        for (int i = 0; i < cellList.Count; i++)
        {
            var worldPos = PositionConverter.IndexToWorldPos(cellList[i], width, height, spacing);
            var intWorldPos = new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.z));
            var offset = worldPos - intWorldPos;
            chain.Add((intWorldPos, offset, false));
        }

        StartCoroutine(PlayDotChain(chain));
    }

    private IEnumerator PlayDotChain(List<(Vector3Int pos, Vector3 offset, bool needsTile)> chain)
    {
        foreach (var step in chain)
        {
            if (step.needsTile)
                SetupBaseTile(step.pos, step.offset);

            TriggerEffect(step.pos, step.offset);
            yield return new WaitForSeconds(chainStepDelay);
        }
    }

    private void SetupBaseTile(Vector3Int pos, Vector3 offset)
    {

        targetTilemap.SetTile(pos, _dotTile);
        targetTilemap.SetTileFlags(pos, TileFlags.None);

        var matrix = Matrix4x4.TRS(offset, Quaternion.identity, new Vector3(1f, 1f, 1f));
        targetTilemap.SetTransformMatrix(pos, matrix);
        ApplyTileVisual(pos, baseScale, baseColor);
    }

    private void ApplyTileVisual(Vector3Int pos, float scale, Color color)
    {
        targetTilemap.SetColor(pos, color);
        targetTilemap.SetTransformMatrix(pos, Matrix4x4.Scale(Vector3.one * scale));
        _currentScale[pos] = scale;
        _currentColor[pos] = color;
    }

    private Tile CreateCircleTile(int resolution)
    {
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        //tex.filterMode = FilterMode.Bilinear;

        float radius = resolution / 2f;
        var center = new Vector2(radius, radius);

        for (int x = 0; x < resolution; x++)
        {
            for (int y = 0; y < resolution; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius - distance);
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();

        Sprite circleSprite = Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = circleSprite;
        return tile;
    }

    
}
