using Assets.Scripts.CoreLogic;
using Assets.Scripts.Ultility;
using Assets.Scripts.Utility;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Assets.Scripts.Boosters
{
    public class Ruler : IBooster
    {
        private int _pixelsPerUnit = 100;
        private int _lineWidth = 15;
        private int _lineLength = 5000;
        private UnityEngine.Color _lineColor = UnityEngine.Color.lightSkyBlue;
        private Tile _lineTile;
        private Tilemap _tilemap;
        private IController _controller;

        private RectTransform _rectTransform;
        private Vector2 originalPosition;
        private Vector3 originalRotation;


        public Ruler(Tilemap tilemap, Image image)
        {
            _tilemap = tilemap;
            ClickedAnimationInit(image);
            _lineTile = CreateLineTile(_lineLength, _lineWidth);
            _controller = Locator.Get<IController>();
            _controller.OnArrowClicked += HandleDisableLines;
            _controller.OnReset += OnReset;
        }

        public void Dispose()
        {
            _controller.OnArrowClicked -= HandleDisableLines;
            _controller.OnReset -= OnReset;
        }

        private void ClickedAnimationInit(Image image)
        {
            _rectTransform = image.GetComponent<RectTransform>();
            originalPosition = _rectTransform.anchoredPosition;
            originalRotation = _rectTransform.localEulerAngles;
            //_particle.SetPositionAndRotation(originalPosition, Quaternion.identity);
        }

        private void HandleAnimation(Action onComplete, Vector3 worldPos, Vector3Int intWorldPos, Vector3 offset, Direction direction)
        {
            var uiSequence = DOTween.Sequence();
            uiSequence.Append(_rectTransform.DOAnchorPosY(originalPosition.y + 50f, 1f).SetEase(Ease.OutQuad));

            uiSequence.Append(_rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.InOutQuad));

            uiSequence.OnComplete(() =>
            {
                SetupBaseTile(intWorldPos, offset, direction);
                onComplete?.Invoke();
            });
        }

        private void HandleDisableLines(int boardIndex)
        {
            _tilemap.ClearAllTiles();
        }

        public void OnClick(IController controller, Action onComplete)
        {
            var arrows = controller.GetConfigData().Arrows;
            var width = _controller.GetConfigData().BoardWidth;
            var height = _controller.GetConfigData().BoardHeight;
            for (int i = 0; i < arrows.Length; i++)
            {
                var arrowIndices = arrows[i].ArrowIndices;
                if (!controller.IsArrowExisted(arrowIndices[0]))
                    continue;
                Debug.Log(i);
                var direction = controller.GetDirectionAtBoardIndex(arrowIndices[0]);
                var worldPos = PositionConverter.IndexToWorldPos(arrowIndices[0], width, height, _controller.GetSpacing());
                var intWorldPos = new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.z));
                var offset = worldPos - intWorldPos;
                //SetupBaseTile(intWorldPos, offset, direction);
                HandleAnimation(onComplete, worldPos, intWorldPos, offset, direction);
            }
        }

        public void OnReset()
        {
            _tilemap.ClearAllTiles();
        }

        private void SetupBaseTile(Vector3Int pos, Vector3 offset, Direction direction)
        {
            _tilemap.SetTile(pos, _lineTile);
            _tilemap.SetTileFlags(pos, TileFlags.None);

            float angle = GetDirectionAngle(direction);
            var quaternion = Quaternion.Euler(new Vector3(0f, 0f, angle));
            var matrix = Matrix4x4.TRS(offset, quaternion, Vector3.one);
            _tilemap.SetTransformMatrix(pos, matrix);
            _tilemap.SetColor(pos, _lineColor);
        }

        private float GetDirectionAngle(Direction direction)
        {
            switch (direction)
            {
                case Direction.RIGHT:
                    return 270f;
                case Direction.UP:
                    return 0f;
                case Direction.LEFT:
                    return 90f;
                case Direction.DOWN:
                    return 180f;
                default:
                    return 0f;
            }
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
