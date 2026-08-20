using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Data
{
    public class BoardData
    {  
        public int BoardWidth { get; set; } = 5;
        public int BoardHeight { get; set; } = 4;
        public Arrow[] Arrows { get; set; }

        public BoardData(int boardWidth, int boardHeight, Arrow[] arrows)
        {
            BoardWidth = boardWidth;
            BoardHeight = boardHeight;
            Arrows = arrows;
        }
    }
}
