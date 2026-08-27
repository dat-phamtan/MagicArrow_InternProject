using Assets.Scripts.CoreLogic.Refactor.Interfaces;
using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.CoreLogic.Refactor.Implements
{
    public class MovementResolver : IMovementResolver
    {
        private readonly IBoardState _board;

        public MovementResolver(IBoardState board)
        {
            _board = board;
        }

        public bool TryResolveMove(int boardIndex, out MoveResult result)
        {
            int cfg = _board.GetConfigIndex(boardIndex);
            var arrow = _board.ConfigData.Arrows[cfg];
            var headPos = _board.IndexToPosition(arrow.ArrowIndices[0]);
            var neckPos = _board.IndexToPosition(arrow.ArrowIndices[1]);
            var direction = _board.GetDirection(headPos, neckPos);

            GetZone(arrow, direction, out int min, out int max, out int step);
            if (IsABlockedPath(arrow.ArrowIndices[0], cfg, min, max, step, out int hitIndex))
            {
                int delta = (hitIndex - arrow.ArrowIndices[0]) / step;
                int hitConfigIndex = _board.GetConfigIndex(hitIndex);
                result = new MoveResult(false, cfg, hitConfigIndex, delta);
                return false;
            }
            result = new MoveResult(true, cfg, -1, 0);
            return true;
            
        }

        public int FindAnyMoveableArrow(out Direction direction)
        {
            var index = FindAMoveableArrow();
            if (index == -1)
            {
                direction = Direction.LEFT;
                return index;
            }
            direction = _board.GetDirection(index);
            return index;
        }

        public List<int> GetExitPathCells(int headX, int headY, Direction dir)
        {
            int minBoardIndex, maxBoardIndex, step;
            int headBoardIndex = _board.IntPositionToIndex(new Position(headX, headY));
            switch (dir)
            {
                case Direction.RIGHT: //right
                    minBoardIndex = _board.ConfigData.BoardWidth * headY;
                    maxBoardIndex = _board.ConfigData.BoardWidth * (headY + 1) - 1;
                    step = 1;
                    return GenerateListCell(headBoardIndex, minBoardIndex, maxBoardIndex, step);
                case Direction.UP: //up
                    minBoardIndex = headX;
                    maxBoardIndex = (_board.ConfigData.BoardHeight - 1) * _board.ConfigData.BoardWidth + headX;
                    step = _board.ConfigData.BoardWidth;
                    return GenerateListCell(headBoardIndex, minBoardIndex, maxBoardIndex, step);
                case Direction.LEFT: //left
                    minBoardIndex = _board.ConfigData.BoardWidth * headY;
                    maxBoardIndex = _board.ConfigData.BoardWidth * (headY + 1) - 1;
                    step = -1;
                    return GenerateListCell(headBoardIndex, minBoardIndex, maxBoardIndex, step);
                case Direction.DOWN: //down
                    minBoardIndex = headX;
                    maxBoardIndex = (_board.ConfigData.BoardHeight - 1) * _board.ConfigData.BoardWidth + headX;
                    step = -_board.ConfigData.BoardWidth;
                    return GenerateListCell(headBoardIndex, minBoardIndex, maxBoardIndex, step);
                default:
                    return new List<int>();
            }
        }


        //HELPER
        private void GetZone(Arrow movedArrow, Direction direction, out int min, out int max, out int step)
        {
            int width = _board.ConfigData.BoardWidth;
            int height = _board.ConfigData.BoardHeight;
            switch (direction)
            {
                case Direction.RIGHT:
                    min = width * movedArrow.YArrowHead;
                    max = width * (movedArrow.YArrowHead + 1) - 1;
                    step = 1;
                    break;
                case Direction.UP:
                    min = movedArrow.XArrowHead;
                    max = (height - 1) * width + movedArrow.XArrowHead;
                    step = width;
                    break;
                case Direction.LEFT:
                    min = width * movedArrow.YArrowHead;
                    max = width * (movedArrow.YArrowHead + 1) - 1;
                    step = -1;
                    break;
                case Direction.DOWN:
                    min = movedArrow.XArrowHead;
                    max = (height - 1) * width + movedArrow.XArrowHead;
                    step = -width;
                    break;
                default:
                    min = max = step = -1;
                    break;
            }
        }

        private bool IsABlockedPath(int headBoardIndex, int configIndex, int minBoardIndex, int maxBoardIndex, int step, out int currentBoardIndex)
        {
            int cBoardIndex = headBoardIndex + step;
            currentBoardIndex = -1;
            if (cBoardIndex < minBoardIndex || cBoardIndex > maxBoardIndex) //the head at border
                return false;

            while (cBoardIndex >= minBoardIndex && cBoardIndex <= maxBoardIndex)
            {
                if (IsBlockedCell(cBoardIndex, configIndex))
                {
                    currentBoardIndex = cBoardIndex;
                    return true;
                }
                cBoardIndex += step;
            }
            return false;
        }

        private bool IsBlockedCell(int boardIndex, int configIndex)
        {
            return _board.IsOccupied(boardIndex) 
                && _board.ArrowMatrix[boardIndex] != configIndex;

        }

        private int FindAMoveableArrow()
        {
            for (int i = 0; i < _board.ConfigData.Arrows.Length; i++)
            {
                var arrow = _board.ConfigData.Arrows[i];
                var direction = _board.GetDirection(arrow.ArrowIndices[0]);
                GetZone(arrow, direction, out int min, out int max, out int step);
                if (!IsABlockedPath(arrow.ArrowIndices[0], i, min, max, step, out _) && _board.IsArrowActive(arrow.ArrowIndices[0]))
                {
                    return arrow.ArrowIndices[0];
                }
            }
            return -1;
        }

        private List<int> GenerateListCell(int headBoardIndex, int min, int max, int step)
        {
            var cells = new List<int>();
            int currentBoardIndex = headBoardIndex + step;
            while (currentBoardIndex >= min && currentBoardIndex <= max)
            {
                cells.Add(currentBoardIndex);
                currentBoardIndex += step;
            }
            return cells;
        }
    }
}
