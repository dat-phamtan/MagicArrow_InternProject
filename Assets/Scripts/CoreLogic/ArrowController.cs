using Assets.Scripts.Config;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Assets.Scripts.CoreLogic
{
    public enum PartType { HEAD, BODY, TAIL }
    public enum Direction { LEFT, RIGHT, UP, DOWN, LEFTUP, LEFTDOWN, RIGHTUP, RIGHTDOWN }
    public class ArrowController : IController
    {
        private ConfigData _configData;
        private IConfig _config;
        private IInput _input;

        private List<int> _boardMatrix;
        private List<bool> _boardMatrixCheck; //<-- stupid name
        private List<Direction> _directions;

        public event Action OnGridInit;
        public event Action<int> OnMoveArrowAway;


        public ArrowController(IConfig config, IInput input)
        {
            _config = config;
            _input = input;
        }

        public List<int> GetArrowMatrix()
        {
            return _boardMatrix;
        }

        public ConfigData GetConfigData()
        {
            return _configData;
        }

        public PartType GetArrowTypeAtPosition(Position pos)
        {
            int boardIndex = IntPositionToIndex(pos);
            var arrow = _configData.Arrows[_boardMatrix[boardIndex]];
            if (pos.X == arrow.XArrowHead && pos.Y == arrow.YArrowHead)
                return PartType.HEAD;

            var tailPosition = IndexToPosition(arrow.ArrowIndices[^1]);
            if ((pos.X == tailPosition.X) && (pos.Y == tailPosition.Y))
                return PartType.TAIL;

            return PartType.BODY;
        }

        public Direction GetDirectionAtPosition(Position pos)
        {
            var index = IntPositionToIndex(pos);
            return _directions[index];
        }

        public void Init()
        {
            LoadConfig();
            InputInit();
            MatrixesInit();
            LoadMatrixes();
            CurveCorrection();
            RegisterAction();
        }

        private void LoadConfig()
        {
            _configData = _config.Load();
        }
        
        private void InputInit()
        {
            _input.InitInput(_configData.BoardWidth, _configData.BoardHeight);
        }

        private void RegisterAction()
        {
            _input.OnInteractAtPosition += HandleUserInput;
            //tobecontinued
        }

        private void LoadMatrixes()
        {
            for (int arrowIndex = 0; arrowIndex < _configData.Arrows.Length; arrowIndex++)
            {
                var arrow = _configData.Arrows[arrowIndex];
                for (int j = 0; j < arrow.ArrowIndices.Length; j++)
                {
                    int cellIndex = arrow.ArrowIndices[j];
                    AddMatrixes(arrowIndex, cellIndex);
                    DirectionInit(arrow.ArrowIndices);
                }
            }
        }

        private void CurveCorrection()
        {
            for (int i = 0; i < _configData.Arrows.Length; i++)
            {
                var arrowIndices = _configData.Arrows[i].ArrowIndices;
                if (arrowIndices.Length < 3)
                    return;

               //if (IsCurveExist())
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

        private void AssignCurve(int index, Direction pre, Direction pos)
        {
            if (pre == Direction.RIGHT)
            {
                if (pos == Direction.UP)
                    _directions[index] = Direction.RIGHTUP; // _|
                else
                    _directions[index] = Direction.RIGHTDOWN;//  -|
            }
            else if (pre == Direction.LEFT)
            {
                if (pos == Direction.UP)
                    _directions[index] = Direction.LEFTUP;//  |_
                else
                    _directions[index] = Direction.LEFTDOWN;//  

            }
            else if (pre == Direction.UP)
            {
                if (pos == Direction.LEFT)
                    _directions[index] = Direction.LEFTUP;
                else
                    _directions[index] = Direction.RIGHTUP;
            }
            else
            {
                if (pos == Direction.LEFT)
                    _directions[index] = Direction.LEFTDOWN;
                else
                    _directions[index] = Direction.RIGHTDOWN;
            }
        }

        private void MatrixesInit()
        {
            int boardSize = _configData.BoardWidth * _configData.BoardHeight;
            _boardMatrix = Enumerable.Repeat(-1, boardSize).ToList();
            _boardMatrixCheck = Enumerable.Repeat(false, boardSize).ToList();
            _directions = Enumerable.Repeat(Direction.LEFT, boardSize).ToList();
        }

        private void AddMatrixes(int configIndex, int cellIndex)
        {
            _boardMatrix[cellIndex] = configIndex;
            _boardMatrixCheck[cellIndex] = true;
        }

        private void DirectionInit(int[] indices)
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
                _directions[indices[0]] = directon;
            }
        }

        private bool IsInsidePlayZone(Position pos)
        {
            if (_configData == null) return false;
            if (pos.X < 0 || pos.Y < 0) return false;
            if (pos.X >= _configData.BoardWidth || pos.Y >= _configData.BoardHeight) return false;
            return true;
        }

        private bool IsPartOfExistArrow(int index)
        {
            return _boardMatrix[index] != -1 && _boardMatrixCheck[index];
        }


        private int IntPositionToIndex(Position pos)
        {
            return pos.X + pos.Y * _configData.BoardWidth;
        }

        private Position IndexToPosition(int index)
        {
            int boardWidth = _configData.BoardWidth;
            return new Position(index % boardWidth, index / boardWidth);
        }

        private void MoveArrowAtIndex(int index)
        {
            var movedArrow = _configData.Arrows[_boardMatrix[index]];
            var headPos = new Position(movedArrow.XArrowHead, movedArrow.YArrowHead);
            var neckPos = IndexToPosition(movedArrow.ArrowIndices[1]);
            var direction = GetDirection(headPos, neckPos);
            int from, to, delta;
            switch (direction)
            {
                case Direction.RIGHT: //right
                    from = _configData.BoardWidth * movedArrow.YArrowHead;
                    to = _configData.BoardWidth * (movedArrow.YArrowHead + 1) - 1;
                    delta = 1;
                    HandleMove(_boardMatrix[index], index, from, to, delta);
                    break;
                case Direction.UP: //up
                    from = movedArrow.XArrowHead;
                    to = (_configData.BoardHeight - 1) * _configData.BoardWidth + movedArrow.XArrowHead;
                    delta = _configData.BoardWidth;
                    HandleMove(_boardMatrix[index], index, from, to, delta);
                    break;
                case Direction.LEFT: //left
                    from = _configData.BoardWidth * movedArrow.YArrowHead;
                    to = _configData.BoardWidth * (movedArrow.YArrowHead + 1) - 1;
                    delta = -1;
                    HandleMove(_boardMatrix[index], index, from, to, delta);
                    break;
                case Direction.DOWN: //down
                    from = movedArrow.XArrowHead;
                    to = (_configData.BoardHeight - 1) * _configData.BoardWidth + movedArrow.XArrowHead;
                    delta = -_configData.BoardWidth;
                    HandleMove(_boardMatrix[index], index, from, to, delta);
                    break;
            }
        }

        private void HandleMove(int indexInConfig, int headIndexInMatrix, int from, int to, int delta) //need to be renamed --> stupid name
        {
            int tempIndex = headIndexInMatrix + delta;
            var IsCollided = false;
            while (tempIndex >= from && tempIndex <= to)
            {
                if (_boardMatrix[tempIndex] != -1 && _boardMatrixCheck[tempIndex])
                {
                    //collision detected --> invoke animaton
                    Debug.LogWarning("Can not move awway!!!");
                    IsCollided = true;
                    break;
                }
                tempIndex += delta;
            }
            if (!IsCollided)
            {
                //no collision --> invoke animation
                DiableArrow(indexInConfig);
                OnMoveArrowAway?.Invoke(indexInConfig);
            }
        }

        private void DiableArrow(int index)
        {
            var arrowIndices = _configData.Arrows[index].ArrowIndices;
            for (int i = 0; i < arrowIndices.Length; i++)
            {
                _boardMatrixCheck[arrowIndices[i]] = false;
            }
        }

        private Direction GetDirection(Position prePos, Position currentPos)
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


        //Input handler
        private void HandleUserInput(Position pos)
        {
            int index = IntPositionToIndex(pos);
            if (!IsPartOfExistArrow(index))
                return;

            MoveArrowAtIndex(index);
        }
    }
}
