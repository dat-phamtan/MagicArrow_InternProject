using Assets.Scripts.Config;
using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Assets.Scripts.CoreLogic
{
    public enum PartType { HEAD, BODY, TAIL}
    public class ArrowController : IController
    {
        private ConfigData _configData;
        private IConfig _config;

        private List<int> _boardMatrix;
        private List<bool> _boardMatrixCheck; //<-- stupid name

        public event Action OnGridInit;


        public ArrowController(IConfig config)
        {
            _config = config;
        }

        public List<int> GetArrowMatrix()
        {
            return _boardMatrix;
        }

        public ConfigData GetConfigData()
        {
            return _configData;
        }

        public void LoadData()
        {
            _configData = _config.Load();
            BoardMatrixsInit();
        }

        public PartType GetArrowTypeAtPosition(int x, int y)
        {
            var arrow = _configData.Arrows[_boardMatrix[x + y * _configData.BoardWidth]];
            if (x == arrow.XArrowHead && y == arrow.YArrowHead)
                return PartType.HEAD;
            else if ((x, y) == IndexToPosition(arrow.ArrowIndices[^1]))
                return PartType.TAIL;
            else
                return PartType.BODY;
        }

        private void BoardMatrixsInit()
        {
            _boardMatrix = Enumerable.Repeat(-1, _configData.BoardWidth * _configData.BoardHeight).ToList();
            _boardMatrixCheck = Enumerable.Repeat(false, _configData.BoardWidth * _configData.BoardHeight).ToList();
            for (int i = 0; i < _configData.Arrows.Length; i++)
            {
                var arrow = _configData.Arrows[i];
                for (int j = 0; j < arrow.ArrowIndices.Length; j++)
                {
                    _boardMatrix[j] = i;
                    _boardMatrixCheck[j] = true;
                }
            }
        }

        private bool IsInsidePlayZone(int x, int y)
        {
            if (_configData == null) return false;
            if (x < 0 || y < 0) return false;
            if (x >= _configData.BoardWidth || y >= _configData.BoardHeight) return false;
            return true;
        }

        private bool IsPartOfExistArrow(int index)
        {
            return _boardMatrix[index] != -1 && _boardMatrixCheck[index];
        }


        private int IntPositionToIndex(int x, int y)
        {
            return x + y * _configData.BoardWidth;
        }

        private (int, int) IndexToPosition(int index)
        {
            int boardWidth = _configData.BoardWidth;
            return (index % boardWidth, index / boardWidth);
        }

        private void MoveArrowAtIndex(int index)
        {
            var movedArrow = _configData.Arrows[_boardMatrix[index]];
            var direction = GetArrowDirection(movedArrow);
            int from, to, delta;
            switch (direction)
            {
                case (1, 0): //right
                    from = _configData.BoardWidth * movedArrow.YArrowHead;
                    to = _configData.BoardWidth * (movedArrow.YArrowHead + 1) - 1;
                    delta = 1;
                    HandleMove(_boardMatrix[index], index, from, to, delta);
                    break;
                case (0, 1): //up
                    from = movedArrow.XArrowHead;
                    to = (_configData.BoardHeight - 1) * _configData.BoardWidth + movedArrow.XArrowHead;
                    delta = _configData.BoardWidth;
                    HandleMove(_boardMatrix[index], index, from, to, delta);
                    break;
                case (-1, 0): //left
                    from = _configData.BoardWidth * movedArrow.YArrowHead;
                    to = _configData.BoardWidth * (movedArrow.YArrowHead + 1) - 1;
                    delta = -1;
                    HandleMove(_boardMatrix[index], index, from, to, delta);
                    break;
                case (0, -1): //down
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
                    IsCollided = true;
                    break;
                }
                tempIndex += delta;
            }
            if (!IsCollided)
            {
                //no collision --> invoke animation
                DiableArrow(indexInConfig);
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

        private (int, int) GetArrowDirection(Arrow arrow)
        {
            var (xArrowNeck, yArrowNeck) = IndexToPosition(arrow.ArrowIndices[1]);
            return (arrow.XArrowHead - xArrowNeck, arrow.YArrowHead - yArrowNeck);
        } 
        

        //Input handler
        private void HandleUserInput(int x, int y)
        {
            if (!IsInsidePlayZone(x, y))
                return;

            int index = x + y * _configData.BoardWidth;
            if (!IsPartOfExistArrow(index))
                return;

            MoveArrowAtIndex(index);




        }
    }
}
