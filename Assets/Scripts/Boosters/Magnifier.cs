using Assets.Scripts.CoreLogic;
using Assets.Scripts.Ultility;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Boosters
{ 
    public class Magnifier : IBooster
    {
        private int _pixelsPerUnit = 100;
        private int _lineWidth = 15;
        private int _lineLength = 1000;
        private Color _lineColor = Color.green;
        private Tile _lineTile;
        private Tilemap _tilemap;
        private IController _controller;

        public Magnifier(Tilemap tilemap)
        {
            _tilemap = tilemap;
            _lineTile = CreateLineTile(_lineLength, _lineWidth);
            _controller = Locator.Get<IController>();
        }

        public void OnClick(IController controller)
        {
            var width = _controller.GetConfigData().BoardWidth;
            var height = _controller.GetConfigData().BoardHeight;
            var index = _controller.GetMovableArrowPosAndDir(out Direction direction);
            var worldPos = PositionConverter.IndexToWorldPos(index, width, height, _controller.GetSpacing());
            //var intWorldPos = new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.z));
            //var offset = worldPos - intWorldPos;

            var intWorldPos = new Vector3Int(0, 0, 0);
            var offset = Vector3.zero;

            SetupBaseTile(intWorldPos, offset, Direction.RIGHT);
        }

        private void SetupBaseTile(Vector3Int pos, Vector3 offset, Direction direction)
        {
            _tilemap.SetTile(pos, _lineTile);
            _tilemap.SetTileFlags(pos, TileFlags.None);
            var quaternion = Quaternion.Euler(GetDirectionVector(direction));
            var matrix = Matrix4x4.TRS(offset, quaternion, new Vector3(1f, 1f, 1f));
            _tilemap.SetTransformMatrix(pos, matrix);
            ApplyTileVisual(pos, 1, _lineColor);
        }

        private Vector3 GetDirectionVector(Direction direction)
        {
            switch (direction)
            {
                case Direction.LEFT:
                    return Vector3.left;
                case Direction.RIGHT:
                    return Vector3.right;
                case Direction.UP:
                    return Vector3.up;
                case Direction.DOWN:
                    return Vector3.down;
                default:
                    return Vector3.up;
            }
        }

        private void ApplyTileVisual(Vector3Int pos, float scale, Color color)
        {
            _tilemap.SetColor(pos, color);
            _tilemap.SetTransformMatrix(pos, Matrix4x4.Scale(Vector3.one * scale));
        }

        private Tile CreateLineTile(int length, int width)
        {
            Texture2D tex = new Texture2D(width, length, TextureFormat.RGBA32, false);
            for (int x = 0; x < width; x++)
                for (int y = 0; y < length; y++)
                    tex.SetPixel(x, y, _lineColor);

            tex.Apply();

            Sprite lineSprite = Sprite.Create(tex, new Rect(0, 0, width, length), new Vector2(0.5f, 0), _pixelsPerUnit);
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = lineSprite;
            return tile;
        }
    }
}
