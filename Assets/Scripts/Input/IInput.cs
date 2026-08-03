using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Assets.Scripts.Input
{
    public interface IInput
    {
        public event Action<Position> OnInteractAtPosition;
        public void InitInput(int width, int height);
        public void HandleInput(UnityEngine.Vector3 pos);
    }
}
