using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Ultility
{
    public static class PositionConverter
    {
        public static Vector3 IndexToWorldPos(int index, int boardWidth, int boardHeight, float spacing)
        {
            int x = index % boardWidth;
            int y = index / boardWidth;
            float offsetX = -(boardWidth - 1) * spacing / 2f;
            float offsetY = -(boardHeight - 1) * spacing / 2f;
            return new Vector3(offsetX + x * spacing, offsetY + y * spacing, 0);
        }
    }
}
