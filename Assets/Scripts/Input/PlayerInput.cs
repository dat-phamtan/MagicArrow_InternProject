using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Input
{
    public class PlayerInput : IInput
    {
        //private IController _controller;
        private float _absPlayZoneX;
        private float _absPlayZoneY;
        private readonly float _spacing;

        public event Action<Position> OnInteractAtPosition;

        public PlayerInput(float spacing)
        {
            _spacing = spacing;
        }

        public void InitInput(int width, int height)
        {
            _absPlayZoneX = (width - 1) * _spacing / 2;
            _absPlayZoneY = (height - 1) * _spacing / 2;
        }

        public void HandleInput(UnityEngine.Vector3 pos)
        {
            if (!IsInsideThePlayZone(pos))
                return;

            var boardPos = ConvertWorldPointToPosition(pos);
            OnInteractAtPosition?.Invoke(boardPos);
        }














        // HELPER FUNCS
        private bool IsInsideThePlayZone(Vector3 pos)
        {
            if (pos.x > _absPlayZoneX || pos.x < -_absPlayZoneX)
                return false;
            if (pos.y > _absPlayZoneY || pos.y < -_absPlayZoneY)
                return false;
            return true;
        }

        private Position ConvertWorldPointToPosition(Vector3 worldPoint)
        {
            int x = (int)System.Math.Round((worldPoint.x + _absPlayZoneX) / _spacing);
            int y = (int)System.Math.Round((worldPoint.y + _absPlayZoneY) / _spacing);
            return new Position(x, y);
        }
    }
}
