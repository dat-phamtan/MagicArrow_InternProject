using Assets.Scripts.Boosters.BoostersManager;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Input
{
    public class PlayerInput : IInput
    {
        public int rangeCheck = 1; //3x3
        private float _absPlayZoneX;
        private float _absPlayZoneY;
        private readonly float _spacing;
        private readonly float _padding;
        private int _width;
        private int _height;
        private bool _isAnimationPlayed;
        private IBoostersManager _boostersManager;

        public event Action<Position> OnInteractAtPosition;

        public PlayerInput(float spacing)
        {
            _spacing = spacing;
            _padding = spacing / 2; 
        }

        public void InitInput(int width, int height)
        {
            _boostersManager = Locator.Get<IBoostersManager>();
            _boostersManager.OnBoosterBusyChanged += HandleAnimationBlock;
            _absPlayZoneX = ((width - 1) * _spacing) / 2;
            _absPlayZoneY = ((height - 1) * _spacing) / 2;
            _width = width;
            _height = height;
        }

        private void HandleAnimationBlock(bool isBusy)
        {
            _isAnimationPlayed = isBusy;
        }

        public void HandleInput(UnityEngine.Vector3 pos)
        {
            if (!IsInsideThePlayZone(pos) || _isAnimationPlayed)
                return;

            var boardPos = FindTheNearestCellPos(pos);
            //var boardPos = FindTheNearestCellPos(pos);
            //var boardPos2 = ConvertWorldPointToPosition2(pos);
            if (boardPos == null) return;
            OnInteractAtPosition?.Invoke(boardPos);
        }

        private Position FindTheNearestCellPos(Vector3 pos)
        {
            var cellPos = ConvertWorldPointToPosition(pos);
            var fCellPos = ConvertWorldPointToPositionF(pos);

            Position best = null;
            float bestDistance = float.MaxValue;

            var controller = Locator.Get<IController>();
            for (int i = -rangeCheck; i <= rangeCheck; i++)
            {
                for (int j = -rangeCheck; j <= rangeCheck; j++)
                {
                    int x = cellPos.X + i;
                    int y = cellPos.Y + j;

                    if (x < 0 || y < 0 || x >= _width || y >= _height)
                        continue;

                    int boardIndex = x + y * _width;
                    if (!controller.IsOccupiedCell(boardIndex))
                        continue;

                    float dx = fCellPos.Xf - x;
                    float dy = fCellPos.Yf - y;

                    float distance = dx * dx + dy * dy;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        best = new Position(x, y);
                    }
                }
            }
            return best;
        }





        // HELPER FUNCS
        private bool IsInsideThePlayZone(Vector3 pos)
        {
            if (pos.x > _absPlayZoneX + _padding || pos.x < -(_absPlayZoneX + _padding))
                return false;
            if (pos.y > _absPlayZoneY + _padding || pos.y < -(_absPlayZoneY + _padding))
                return false;
            return true;
        }

        //private Position ConvertWorldPointToPosition(Vector3 worldPoint)
        //{
        //    int x = (int)System.Math.Round((worldPoint.x + _absPlayZoneX) / _spacing);
        //    int y = (int)System.Math.Round((worldPoint.y + _absPlayZoneY) / _spacing);
        //    Debug.Log($"{x}/{y}");
        //    return new Position(x, y);
        //}

        private Position ConvertWorldPointToPosition(Vector3 worldPoint)
        {
            var fPos = ConvertWorldPointToPositionF(worldPoint);
            int x = Mathf.RoundToInt(fPos.Xf);
            int y = Mathf.RoundToInt(fPos.Yf);
            return new Position(x, y);
        }

        private Position ConvertWorldPointToPositionF(Vector3 worldPoint)
        {
            float x = (worldPoint.x + _absPlayZoneX) / _spacing;
            float y = (worldPoint.y + _absPlayZoneY) / _spacing;

            return new Position(x, y);
        }
    }
}
