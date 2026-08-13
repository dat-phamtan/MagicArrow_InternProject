using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Tilemaps;

public class TileMapGenerate : MonoBehaviour
{
    public Tilemap targetTilemap;
    //public TileBase dotTile;

    public int width = 10;
    public int height = 10;

    public Color baseColor = Color.gray;
    public Color activeColor = Color.blue;
    public Color lineColor = Color.green;
    public float baseScale = 1f;
    public float activeScale = 2f;
    public float animationDuration = 0.5f;

    //private Tile generatedCircleTile;
    private Tile _lineTile;
    private readonly Dictionary<Vector3Int, float> _currentScale = new();
    private readonly Dictionary<Vector3Int, Color> _currentColor = new();

    private void Start()
    {
        _lineTile = CreateLineTile(1000, 10);
        GenerateMap();
    }

    private void GenerateMap()
    {
        targetTilemap.ClearAllTiles();
        SetupBaseTile(new Vector3Int(0, 0, 0));
        //for (int i = 0; i < width; i++)
        //{
        //    for (int j = 0; j < height; j++)
        //    {
        //        var cellPos = new Vector3Int(i, j, 0);
        //        SetupBaseTile(cellPos);
        //    }
        //}
    }

    private void SetupBaseTile(Vector3Int pos)
    {
        
        targetTilemap.SetTile(pos, _lineTile);
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

    private Tile CreateLineTile(int length, int width)
    {
        Texture2D tex = new Texture2D(width, length, TextureFormat.RGBA32, false);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < length; y++)
            {
                tex.SetPixel(x, y, lineColor);
            }
        }
        tex.Apply();

        Sprite lineSprite = Sprite.Create(tex, new Rect(0, 0, width, length), new Vector2(0.5f, 0), 100);
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        //tile.GetComponent<Tilemap>().tileAnchor = new Vector3(0, 0, 0);
        tile.sprite = lineSprite;
        return tile;
    }
}
