using Assets.Scripts.CoreLogic;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.Scenes.Helper
{
    public class DotGenerator
    {
        private IController _controller;
        private Color _baseColor = Color.grey;
        private Color _activeColor = Color.blue;
        private float _baseScale = 0.15f;
        private float _activeScale = 0.5f;
        private float _animationDuration = 0.5f;
        private Tile _dotTile;

        private readonly Dictionary<Vector3Int, float> _currentScale = new();
        private readonly Dictionary<Vector3Int, Color> _currentColor = new();

        
        public DotGenerator(IController controller)
        {
            _controller = controller;
            _controller.OnMoveArrowSuccess += HandleSpawnDots;
        }

        private void HandleSpawnDots(int interactedConfigIndex)
        {
            
        }

        private void GenerateDotsUnderArrow()
        {

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
}
