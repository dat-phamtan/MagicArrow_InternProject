using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Data
{
    public class Position
    {
        public int X {  get; set; }
        public float Xf { get; set; }
        public int Y { get; set; }
        public float Yf { get; set; }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Position(float  x, float y)
        {
            Xf = x;
            Yf = y;
        }
    }
}
