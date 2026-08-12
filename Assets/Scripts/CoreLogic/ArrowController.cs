using Assets.Scripts.Config;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using Assets.Scripts.Scenes;
using Assets.Scripts.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.CoreLogic
{
    public enum PartType { HEAD, BODY, TAIL }
    public enum Direction { LEFT, RIGHT, UP, DOWN, LEFTUP, LEFTDOWN, RIGHTUP, RIGHTDOWN }
    public class ArrowController : IController
    {
        private ConfigData _configData;
        private readonly IConfig _config;
        private readonly IInput _input;
        private readonly IUIManager _uiManager;
        private IEventHandler _eventHandler;
        private IPopUpManager _popupManager;

        private List<int> _boardMatrix; //index: boardIndex, value: configIndex
        private List<bool> _boardMatrixCheck; //index: boardIndex, value: true/false   <-- stupid name
        private List<Direction> _directions;
        private List<bool> _isAnimated;
        private List<bool> _isFirstMoveFail;
        private bool _isWinOrLose = false;
        private bool _isWaitingForEraserBooster = false;
        private int _heart = 3;
        private int _numAnimationSuccess = 0;
        private int _numAnimationFail = 0;
        

        public event Action<int> OnMoveArrowSuccess;
        public event Action<int, int, int> OnMoveArrowFail;
        public event Action<int> OnEraseArrowAt;
        public event Action OnTurnPopupOff;
        public event Action<bool> OnTurnPopupOn;
        public event Action OnRerenderBoard;

        // implement interface
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
            var boardIndex = IntPositionToIndex(pos);
            return _directions[boardIndex];
        }
        public Direction GetDirectionAtBoardIndex(int boardIndex)
        {
            return _directions[boardIndex];
        }
        public void ChangeEraserUsedMode()
        {
            _isWaitingForEraserBooster = !_isWaitingForEraserBooster;
        }
        public void DiableArrow(int configIndex)
        {
            var arrowIndices = _configData.Arrows[configIndex].ArrowIndices;
            for (int i = 0; i < arrowIndices.Length; i++)
            {
                _boardMatrixCheck[arrowIndices[i]] = false;
            }
        }
        public bool IsFirstMoveFail(int configIndex)
        {
            if (_isFirstMoveFail[configIndex])
            {
                _isFirstMoveFail[configIndex] = false;
                return true;
            }
            return false;
        }
        public int GetConfigIndexAt(int boardIndex)
        {
            return _boardMatrix[boardIndex];
        }
        public int GetSuccessAnimationNum()
        {
            return _numAnimationSuccess;
        }
        public int GetFailAnimationNum()
        {
            return _numAnimationFail;
        }
        public int GetHeart()
        {
            return _heart;
        }
        public bool IsOccupiedCell(int boardIndex)
        {
            return _boardMatrix[boardIndex] != -1 && _boardMatrixCheck[boardIndex];
        }
        public bool IsWinOrLose()
        {
            return _isWinOrLose;
        }


        // logic
        public void Init(IEventHandler eventHandler, IPopUpManager popupManager)
        {
            LoadConfig();
            InputInit();
            EventHandlerInit(eventHandler, popupManager);
            MatrixesInit();
            LoadMatrixes();
            CurveCorrection();
            RegisterAction();
        }

        //temp
        private void HandlePlayAgain()
        {
            int boardSize = _configData.BoardWidth * _configData.BoardHeight;
            _boardMatrixCheck = Enumerable.Repeat(false, boardSize).ToList();
            _isFirstMoveFail = Enumerable.Repeat(true, _configData.Arrows.Length).ToList();
            _isAnimated = Enumerable.Repeat(false, boardSize).ToList();
            _heart = 3;
            _numAnimationSuccess = 0;
            _numAnimationFail = 0;
            _isWaitingForEraserBooster = false;
            _isWinOrLose = false;

            for (int configIndex = 0; configIndex < _configData.Arrows.Length; configIndex++)
            {
                var arrow = _configData.Arrows[configIndex];
                for (int i = 0; i < arrow.ArrowIndices.Length; i++)
                {
                    int boardIndex = arrow.ArrowIndices[i];
                    _boardMatrixCheck[boardIndex] = true;
                }
            }

            OnTurnPopupOff?.Invoke();
            OnRerenderBoard?.Invoke();
        }

        private void EventHandlerInit(IEventHandler eventHandler, IPopUpManager popupManager)
        {
            _eventHandler = eventHandler;
            _popupManager = popupManager;
        }

        private void LoadConfig()
        {
            _configData = _config.Load();
            //_numArrow = _configData.Arrows.Length;
        }
        
        private void InputInit()
        {
            _input.InitInput(_configData.BoardWidth, _configData.BoardHeight);
        }

        private void RegisterAction()
        {
            _input.OnInteractAtPosition += HandleUserInput;
            _eventHandler.OnUnblockInteractWidthArrow += UnblockInteractWithArrow;
            _popupManager.OnPlayAgain += HandlePlayAgain;
            _eventHandler.OnAnimatedComplete += HandleArrowDestroyed;
            //tobecontinued
        }

        private void HandleArrowDestroyed(bool isSuccess)
        {
            if (isSuccess)
                _numAnimationSuccess--;
            else
                _numAnimationFail--;
        }

        private void LoadMatrixes()
        {
            for (int configIndex = 0; configIndex < _configData.Arrows.Length; configIndex++)
            {
                var arrow = _configData.Arrows[configIndex];
                for (int i = 0; i < arrow.ArrowIndices.Length; i++)
                {
                    int boardIndex = arrow.ArrowIndices[i];
                    AddMatrixes(configIndex, boardIndex);
                }
                DirectionInit(arrow.ArrowIndices);
            }
        }

        private void CurveCorrection()
        {
            for (int i = 0; i < _configData.Arrows.Length; i++)
            {
                var arrowIndices = _configData.Arrows[i].ArrowIndices;
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

        private void MatrixesInit()
        {
            int boardSize = _configData.BoardWidth * _configData.BoardHeight;
            _boardMatrix = Enumerable.Repeat(-1, boardSize).ToList();
            _boardMatrixCheck = Enumerable.Repeat(false, boardSize).ToList();
            _directions = Enumerable.Repeat(Direction.LEFT, boardSize).ToList();
            _isAnimated = Enumerable.Repeat(false, boardSize).ToList();
            _isFirstMoveFail = Enumerable.Repeat(true, _configData.Arrows.Length).ToList();
        }

        private void BlockInteractWithArrow(int configIndex)
        {
            var indices = _configData.Arrows[configIndex].ArrowIndices;
            for (int i = 0; i < indices.Length; i++)
            {
                _isAnimated[indices[i]] = true;
            }
        }

        private void UnblockInteractWithArrow(int configIndex)
        {
            var indices = _configData.Arrows[configIndex].ArrowIndices;
            for (int i = 0; i < indices.Length; i++)
            {
                _isAnimated[indices[i]] = false;
            }
        }

        private bool IsInteractBlocked(int matrixIndex)
        {
            return _isAnimated[matrixIndex];
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
                _directions[indices[i]] = directon;
            }
        }

        private bool IsInsidePlayZone(Position pos)
        {
            if (_configData == null) return false;
            if (pos.X < 0 || pos.Y < 0) return false;
            if (pos.X >= _configData.BoardWidth || pos.Y >= _configData.BoardHeight) return false;
            return true;
        }

        private bool IsPartOfExistArrow(int boardIndex)
        {
            return _boardMatrix[boardIndex] != -1 && _boardMatrixCheck[boardIndex];
        }


        private int IntPositionToIndex(Position pos)
        {
            return pos.X + pos.Y * _configData.BoardWidth;
        }

        private Position IndexToPosition(int boardIndex)
        {
            int boardWidth = _configData.BoardWidth;
            return new Position(boardIndex % boardWidth, boardIndex / boardWidth);
        }

        private void MoveArrowAtIndex(int boardIndex)
        {
            //Debug.Log(boardIndex);
            var movedArrow = _configData.Arrows[_boardMatrix[boardIndex]];
            var headIndexInMatrix = movedArrow.ArrowIndices[0];
            var headPos = new Position(movedArrow.XArrowHead, movedArrow.YArrowHead);
            var neckPos = IndexToPosition(movedArrow.ArrowIndices[1]);

            var direction = GetDirection(headPos, neckPos);
            int minBoardIndex, maxBoardIndex, step;
            switch (direction)
            {
                case Direction.RIGHT: //right
                    minBoardIndex = _configData.BoardWidth * movedArrow.YArrowHead;
                    maxBoardIndex = _configData.BoardWidth * (movedArrow.YArrowHead + 1) - 1;
                    step = 1;
                    HandleMove(_boardMatrix[boardIndex], headIndexInMatrix, minBoardIndex, maxBoardIndex, step);
                    break;
                case Direction.UP: //up
                    minBoardIndex = movedArrow.XArrowHead;
                    maxBoardIndex = (_configData.BoardHeight - 1) * _configData.BoardWidth + movedArrow.XArrowHead;
                    step = _configData.BoardWidth;
                    HandleMove(_boardMatrix[boardIndex], headIndexInMatrix, minBoardIndex, maxBoardIndex, step);
                    break;
                case Direction.LEFT: //left
                    minBoardIndex = _configData.BoardWidth * movedArrow.YArrowHead;
                    maxBoardIndex = _configData.BoardWidth * (movedArrow.YArrowHead + 1) - 1;
                    step = -1;
                    HandleMove(_boardMatrix[boardIndex], headIndexInMatrix, minBoardIndex, maxBoardIndex, step);
                    break;
                case Direction.DOWN: //down
                    minBoardIndex = movedArrow.XArrowHead;
                    maxBoardIndex = (_configData.BoardHeight - 1) * _configData.BoardWidth + movedArrow.XArrowHead;
                    step = -_configData.BoardWidth;
                    HandleMove(_boardMatrix[boardIndex], headIndexInMatrix, minBoardIndex, maxBoardIndex, step);
                    break;
            }
        }

        private void HandleMove(int interactedConfigIndex, int headBoardIndex, int minBoardIndex, int maxBoardIndex, int step) //need to be renamed --> stupid name
        {
            int currentBoardIndex = headBoardIndex + step;
            if (currentBoardIndex < minBoardIndex || currentBoardIndex > maxBoardIndex)
            {
                DiableArrow(interactedConfigIndex);
                OnMoveArrowSuccess?.Invoke(interactedConfigIndex);
                _numAnimationSuccess++;
                if (AllArrowsAreCleared())
                    HandleWin();
                return;
            }

            var IsCollided = false;
            while (currentBoardIndex >= minBoardIndex && currentBoardIndex <= maxBoardIndex)
            { 
                if (IsBlockedPath(currentBoardIndex, interactedConfigIndex))
                {
                    Debug.LogWarning("Fail");
                    var deltaBoardIndex = (currentBoardIndex - headBoardIndex) / step;
                    var collidedConfigIndex = _boardMatrix[currentBoardIndex];
                    BlockInteractWithArrow(interactedConfigIndex);
                    OnMoveArrowFail?.Invoke(interactedConfigIndex, collidedConfigIndex, deltaBoardIndex);

                    if (_isFirstMoveFail[interactedConfigIndex])
                        _heart--;

                    _numAnimationFail++;
                    if (AllHeartAreLost())
                        HandleLose();

                    IsCollided = true;
                    break;
                }
                currentBoardIndex += step;
            }
            if (!IsCollided)
            {
                //Debug.LogWarning("Correct");
                DiableArrow(interactedConfigIndex);
                OnMoveArrowSuccess?.Invoke(interactedConfigIndex);
                _numAnimationSuccess++;

                if (AllArrowsAreCleared())
                    HandleWin();
            }
        }

        private bool IsBlockedPath(int currentBoardIndex, int configIndex)
        {
            return _boardMatrix[currentBoardIndex] != -1 
                && _boardMatrix[currentBoardIndex] != configIndex 
                && _boardMatrixCheck[currentBoardIndex];
        }

        private bool AllArrowsAreCleared()
        {
            for (int i = 0; i < _boardMatrixCheck.Count(); i++)
                if (_boardMatrixCheck[i])
                    return false;
            return true;
        }

        private bool AllHeartAreLost()
        {
            return _heart == 0;
        }

        private void HandleWin()
        {
            Debug.Log("VICTORY!");
            _isWinOrLose = true;
            OnTurnPopupOn?.Invoke(true);
        }

        private void HandleLose()
        {
            Debug.Log("DEFEAT");
            _isWinOrLose = true;
            OnTurnPopupOn?.Invoke(false);
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

        private void EraseArrowAtPosition(int boardIndex)
        {
            DiableArrow(_boardMatrix[boardIndex]);
            OnEraseArrowAt?.Invoke(_boardMatrix[boardIndex]);
            ChangeEraserUsedMode();
        }

        private void FindMoveableArrow()
        {
            //for (int i = 0; i < _boardMatrix)
            //find a movable arrow
        }


        //Input handler
        private void HandleUserInput(Position pos)
        {
            int boardIndex = IntPositionToIndex(pos);
            //Debug.Log(index);
            if (!IsPartOfExistArrow(boardIndex))
                return;

            if (IsInteractBlocked(boardIndex))
                return;

            if (_isWaitingForEraserBooster)
            {
                EraseArrowAtPosition(boardIndex);
                return;
            }
            if (!_isWinOrLose)
                MoveArrowAtIndex(boardIndex);
        }

        
    }
}
