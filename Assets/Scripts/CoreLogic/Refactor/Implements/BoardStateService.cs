using Assets.Scripts.CoreLogic.Refactor.Interfaces;
using Assets.Scripts.Data;
using Assets.Scripts.Ultility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.CoreLogic.Refactor.Implements
{
    public class BoardStateService : IBoardState
    {
        private BoardData _config;
        private List<int> _matrix;
        private List<bool> _active;
        private List<Direction> _directions;
        private List<bool> _animated;

        public BoardData ConfigData {  get { return _config; } }
        public List<int> ArrowMatrix {  get { return _matrix; } }

        
        //INTERFACE IMPLEMENT
        public void Load(BoardData data)
        {
            _config = data;
            int boardSize = data.BoardWidth * data.BoardHeight;
            _matrix = Enumerable.Repeat(-1, boardSize).ToList();
            _active = Enumerable.Repeat(false, boardSize).ToList();
            _directions = Enumerable.Repeat(Direction.LEFT, boardSize).ToList();
            _animated = Enumerable.Repeat(false, boardSize).ToList();

            for (int cfgIndex = 0; cfgIndex < data.Arrows.Length;  cfgIndex++)
            {
                var arrow = data.Arrows[cfgIndex];
                foreach (var boardIndex in arrow.ArrowIndices)
                {
                    _matrix[boardIndex] = cfgIndex;
                    _active[boardIndex] = true;
                }
                InitDirections(arrow.ArrowIndices);
            }
            CorrectCurves();
        }

        public void DisableArrow(int configIndex)
        {
            var arrowIndices = _config.Arrows[configIndex].ArrowIndices;
            for (int i = 0; i < arrowIndices.Length; i++)
            {
                _active[arrowIndices[i]] = false;
            }
        }

        public int GetConfigIndex(int boardIndex)
        {
            return _matrix[boardIndex];
        }

        public Direction GetDirection(int boardIndex)
        {
            return _directions[boardIndex];
        }

        public Direction GetDirection(Position pos)
        {
            var boardIndex = IntPositionToIndex(pos);
            return _directions[boardIndex];
        }

        public PartType GetPartType(Position pos)
        {
            int boardIndex = IntPositionToIndex(pos);
            var arrow = _config.Arrows[_matrix[boardIndex]];
            if (pos.X == arrow.XArrowHead && pos.Y == arrow.YArrowHead)
                return PartType.HEAD;

            var tailPosition = IndexToPosition(arrow.ArrowIndices[^1]);
            if ((pos.X == tailPosition.X) && (pos.Y == tailPosition.Y))
                return PartType.TAIL;

            return PartType.BODY;
        }

        public bool IsAnimated(int configIndex)
        {
            var indices = _config.Arrows[configIndex].ArrowIndices;
            return _animated[indices[0]];
        }

        public bool IsArrowActive(int boardIndex)
        {
            return _active[boardIndex];
        }

        public bool IsOccupied(int boardIndex)
        {
            return _matrix[boardIndex] != -1 && _active[boardIndex];
        }

        public void ResetBoard()
        {
            throw new NotImplementedException();
        }

        public void SetAnimated(int configIndex, bool animated)
        {
            throw new NotImplementedException();
        }

        public int IntPositionToIndex(Position pos)
        {
            return pos.X + pos.Y * _config.BoardWidth;
        }

        public Position IndexToPosition(int boardIndex)
        {
            int boardWidth = _config.BoardWidth;
            return new Position(boardIndex % boardWidth, boardIndex / boardWidth);
        }

        public Direction GetDirection(Position prePos, Position currentPos)
        {
            var direction = (prePos.X - currentPos.X, prePos.Y - currentPos.Y);
            switch (direction)
            {
                case (1, 0): //right
                    return Direction.RIGHT;
                case (0, 1): //up
                    return Direction.UP;
                case (-1, 0): //left
                    return Direction.LEFT;
                case (0, -1): //down
                    return Direction.DOWN;
                default:
                    break;
            }
            return Direction.RIGHT;
        }


        //HELPER
        //init
        private void InitDirections(int[] indices)
        {
            var headPos = IndexToPosition(indices[0]);
            var neckPos = IndexToPosition(indices[1]);
            var headDirection = GetDirection(headPos, neckPos);
            _directions[indices[0]] = headDirection;
            _directions[indices[1]] = headDirection;

            for (int i = 2; i < indices.Length; i++)
            {
                var prePos = IndexToPosition(indices[i - 1]);
                var currentPos = IndexToPosition(indices[i]);
                var directon = GetDirection(prePos, currentPos);
                _directions[indices[i]] = directon;
            }
        }

        //curve
        private void CorrectCurves()
        {
            for (int i = 0; i < _config.Arrows.Length; i++)
            {
                var arrowIndices = _config.Arrows[i].ArrowIndices;
                if (arrowIndices.Length < 3)
                    continue;

                for (int j = 1; j < arrowIndices.Length - 1; j++)
                    if (IsCurveExist(arrowIndices[j - 1], arrowIndices[j + 1]))
                        AssignCurve(arrowIndices[j], _directions[arrowIndices[j - 1]], _directions[arrowIndices[j + 1]]);
            }
        }

        private bool IsCurveExist(int preIndex, int posIndex)
        {

            if ((int)_directions[preIndex] > 3 || (int)_directions[posIndex] > 3)
                return true;
            return _directions[preIndex] != _directions[posIndex];
        }

        private void AssignCurve(int boardIndex, Direction pre, Direction pos)
        {
            if (pre == Direction.RIGHT)
            {
                if (pos == Direction.UP)
                    _directions[boardIndex] = Direction.RIGHTUP; // _|
                else
                    _directions[boardIndex] = Direction.RIGHTDOWN;//  -|
            }
            else if (pre == Direction.LEFT)
            {
                if (pos == Direction.UP)
                    _directions[boardIndex] = Direction.LEFTUP;//  |_
                else
                    _directions[boardIndex] = Direction.LEFTDOWN;//  

            }
            else if (pre == Direction.UP)
            {
                if (pos == Direction.LEFT)
                    _directions[boardIndex] = Direction.LEFTUP;
                else
                    _directions[boardIndex] = Direction.RIGHTUP;
            }
            else
            {
                if (pos == Direction.LEFT)
                    _directions[boardIndex] = Direction.LEFTDOWN;
                else
                    _directions[boardIndex] = Direction.RIGHTDOWN;
            }
        }
    }
}
