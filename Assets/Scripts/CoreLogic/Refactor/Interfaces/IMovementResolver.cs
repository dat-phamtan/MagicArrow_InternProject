using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.CoreLogic.Refactor.Interfaces
{
    public interface IMovementResolver
    {
        public bool TryResolveMove(int boardIndex, out MoveResult result);
        public List<int> GetExitPathCells(int headX, int headY, Direction dir);
        public int FindAnyMoveableArrow(out Direction direction);
    }
}
