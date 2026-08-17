using Assets.Scripts.CoreLogic;
using Assets.Scripts.Ultility;
using Assets.Scripts.Utility;
using DG.Tweening;
using System;
using System.Drawing;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Assets.Scripts.Boosters
{ 
    public class Magnifier : IBooster
    {
        private UnityEngine.Color _lineColor = UnityEngine.Color.green;
        private int _pixelsPerUnit = 100;
        private int _lineWidth = 15;
        private int _lineLength = 5000;
        private bool _isAnimationCompleted = false;
        
        private Tile _lineTile;
        private Tilemap _tilemap;
        private IController _controller;

        private RectTransform _rectTransform;
        private Vector2 originalPosition;
        private Vector3 originalRotation;


        public Magnifier(Tilemap tilemap, Image image)
        {
            _tilemap = tilemap;
            ClickedAnimationInit(image);
            _lineTile = CreateLineTile(_lineLength, _lineWidth);
            _controller = Locator.Get<IController>();
            _controller.OnArrowClicked += HandleDisableLine;
            _controller.OnReset += OnReset;
        }

        private void ClickedAnimationInit(Image image)
        {
            _rectTransform = image.GetComponent<RectTransform>();
            originalPosition = _rectTransform.anchoredPosition;
            originalRotation = _rectTransform.localEulerAngles;
        }

        private void HandleAnimation(Action onComplete, Vector3Int intWorldPos, Vector3 offset, Direction direction)
        {
            var uiSequence = DOTween.Sequence();
            uiSequence.Append(_rectTransform.DOAnchorPosY(originalPosition.y + 50f, 1f).SetEase(Ease.OutQuad));
            uiSequence.Join(_rectTransform.DOLocalRotate(new Vector3(0, 0, 20f), 0.5f).SetEase(Ease.OutQuad));

            uiSequence.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, 40f), 0.5f).SetEase(Ease.OutQuad));
            uiSequence.Append(_rectTransform.DOLocalRotate(new Vector3(0, 0, 20f), 0.2f).SetEase(Ease.OutQuad));

            uiSequence.Append(_rectTransform.DOAnchorPos(originalPosition, 0.3f).SetEase(Ease.InOutQuad));
            uiSequence.Join(_rectTransform.DOLocalRotate(originalRotation, 0.3f).SetEase(Ease.InOutQuad));

            SetupBaseTile(intWorldPos, offset, direction);
            uiSequence.OnComplete(() => onComplete?.Invoke());
        }

        private void HandleDisableLine(int boardIndex)
        {
            var width = _controller.GetConfigData().BoardWidth;
            var height = _controller.GetConfigData().BoardHeight;
            var worldPos = PositionConverter.IndexToWorldPos(boardIndex, width, height, _controller.GetSpacing());
            var intWorldPos = new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.z));

            _tilemap.SetTile(intWorldPos, null);
        }

        public void OnClick(IController controller, Action onComplete)
        {
            var index = _controller.GetMovableArrowPosAndDir(out Direction direction);
            if (index == -1)
            {
                onComplete?.Invoke();
                return;
            }

            var width = _controller.GetConfigData().BoardWidth;
            var height = _controller.GetConfigData().BoardHeight;
            
            var worldPos = PositionConverter.IndexToWorldPos(index, width, height, _controller.GetSpacing());
            var intWorldPos = new Vector3Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y), Mathf.FloorToInt(worldPos.z));
            var offset = worldPos - intWorldPos;

            //SET CAM FOCUS
            //PLAY ANIMATION
            HandleAnimation(onComplete, intWorldPos, offset, direction);

            //SetupBaseTile(intWorldPos, offset, direction);
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
            var tex = new Texture2D(width, length, TextureFormat.RGBA32, false);
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
