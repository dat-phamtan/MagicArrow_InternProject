using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ProceduralTileAnimator : MonoBehaviour
{
    public Tilemap myTilemap;

    [Header("Cài đặt Animation")]
    public Color baseColor = Color.gray;     
    public Color activeColor = Color.blue;  
    public float baseScale = 0.5f;            
    public float activeScale = 1.8f;          
    public float animationDuration = 0.5f;     

    private Tile generatedCircleTile;

    void Start()
    {
        generatedCircleTile = CreateCircleTile(32);

        // Ví dụ: Đặt hình tròn này vào ô (0,0) làm mặc định (màu xám, size 1)
        Vector3Int testPos = new Vector3Int(0, 0, 0);
        SetupBaseTile(testPos);
    }

    private void Update()
    {
        //TriggerEffect(new Vector3Int(0, 0, 0));
    }
















    public void TriggerEffect(Vector3Int cellPosition)
    {
        StartCoroutine(AnimateDot(cellPosition));
    }

    private IEnumerator AnimateDot(Vector3Int pos)
    {
        myTilemap.SetTileFlags(pos, TileFlags.None);

        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {

            float t = elapsedTime / animationDuration;

            Color currentColor = Color.Lerp(activeColor, baseColor, t);
            float currentScale = Mathf.Lerp(activeScale, baseScale, t);

            myTilemap.SetColor(pos, currentColor);

            Matrix4x4 matrix = Matrix4x4.Scale(new Vector3(currentScale, currentScale, 1f));
            myTilemap.SetTransformMatrix(pos, matrix);

            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        SetupBaseTile(pos);
    }

    private void SetupBaseTile(Vector3Int pos)
    {
        myTilemap.SetTile(pos, generatedCircleTile);
        myTilemap.SetTileFlags(pos, TileFlags.None); 
        myTilemap.SetColor(pos, baseColor);          

        Matrix4x4 baseMatrix = Matrix4x4.Scale(new Vector3(baseScale, baseScale, 1f));
        myTilemap.SetTransformMatrix(pos, baseMatrix);
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