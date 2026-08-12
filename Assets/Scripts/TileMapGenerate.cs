using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapGenerate : MonoBehaviour
{
    public Tilemap targetTilemap;
    //public TileBase dotTile;

    public int width = 10;
    public int height = 10;

    public Color baseColor = Color.gray;
    public Color activeColor = Color.blue;
    public float baseScale = 0.15f;
    public float activeScale = 0.5f;
    public float animationDuration = 0.5f;

    //private Tile generatedCircleTile;
    private Tile _dotTile;
    private readonly Dictionary<Vector3Int, float> _currentScale = new();
    private readonly Dictionary<Vector3Int, Color> _currentColor = new();

    private void Start()
    {
        _dotTile = CreateCircleTile(32);
        GenerateMap();
    }

    private void GenerateMap()
    {
        targetTilemap.ClearAllTiles();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var cellPos = new Vector3Int(i, j, 0);
                SetupBaseTile(cellPos);
            }
        }
    }

    private void SetupBaseTile(Vector3Int pos)
    {
        targetTilemap.SetTile(pos, _dotTile);
        targetTilemap.SetTileFlags(pos, TileFlags.None);
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
