using Assets.Scripts.Boosters;
using Assets.Scripts.Config;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using Assets.Scripts.IO;
using Assets.Scripts.Scenes.Helper;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
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
        private PlayerData _playerData;
        private BoardData _configData;
        private readonly IConfig _config;
        private readonly IStorage _storage;
        private readonly IInput _input;
        private readonly IGamePlayUI _uiManager;
        private IEventHandler _eventHandler;
        private IPopUpManager _popupManager;
        //private IBoostersManager _boostersManager = Locator.Get<IBoostersManager>();

        private List<int> _boardMatrix; //index: boardIndex, value: configIndex
        private List<bool> _boardMatrixCheck; //index: boardIndex, value: true/false   <-- stupid name
        private List<Direction> _directions;
        private List<bool> _isAnimated;
        private List<bool> _isFirstMoveFail;
        private float _spacing;
        private bool _isBgInteractBlock = false;
        private bool _isWaitingForEraserBooster = false;
        private int _heart = 3;
        private int _numAnimationSuccess = 0;
        private int _numAnimationFail = 0;
        private int _currentLvl;


        public event Action<int> OnMoveArrowSuccess;
        public event Action<int, int, int> OnMoveArrowFail;
        public event Action<int> OnEraseArrowAt;
        public event Action OnTurnPopupOff;
        public event Action<bool> OnTurnPopupOn;
        public event Action OnRerenderBoard;
        public event Action<int> OnArrowClicked;
        public event Action OnReset;
        public event Action OnLoseHeart;

        public event Action OnHideBarTop;
        public event Action OnShowBarTop;
        public event Action OnHideBoosters;
        public event Action OnShowBoosters;
        public event Action OnHideEraserPopup;
        public event Action OnVictory;
        public event Action OnHeartRestored;

        // implement interface
        public ArrowController(IConfig config, IStorage storage, IInput input, float spacing)
        {
            _config = config;
            _storage = storage;
            _input = input;
            _spacing = spacing;
        }
        public List<int> GetArrowMatrix()
        {
            return _boardMatrix;
        }
        public BoardData GetConfigData()
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
        public float GetSpacing()
        {
            return _spacing;
        }
        public bool IsOccupiedCell(int boardIndex)
        {
            return _boardMatrix[boardIndex] != -1 && _boardMatrixCheck[boardIndex];
        }
        public bool IsBgInteractionBlocked()
        {
            return _isBgInteractBlock;
        }
        public bool IsArrowExisted(int boardIndex)
        {
            return _boardMatrixCheck[boardIndex];
        }
        public List<int> GetNextCells(int yArrowHead, int xArrowHead, Direction direction)
        {
            int minBoardIndex, maxBoardIndex, step;
            int headBoardIndex = IntPositionToIndex(new Position(xArrowHead, yArrowHead));
            switch (direction)
            {
                case Direction.RIGHT: //right
                    minBoardIndex = _configData.BoardWidth * yArrowHead;
                    maxBoardIndex = _configData.BoardWidth * (yArrowHead + 1) - 1;
                    step = 1;
                    return GenerateListCell(headBoardIndex, minBoardIndex, maxBoardIndex, step);
                case Direction.UP: //up
                    minBoardIndex = xArrowHead;
                    maxBoardIndex = (_configData.BoardHeight - 1) * _configData.BoardWidth + xArrowHead;
                    step = _configData.BoardWidth;
                    return GenerateListCell(headBoardIndex, minBoardIndex, maxBoardIndex, step);
                case Direction.LEFT: //left
                    minBoardIndex = _configData.BoardWidth * yArrowHead;
                    maxBoardIndex = _configData.BoardWidth * (yArrowHead + 1) - 1;
                    step = -1;
                    return GenerateListCell(headBoardIndex, minBoardIndex, maxBoardIndex, step);
                case Direction.DOWN: //down
                    minBoardIndex = xArrowHead;
                    maxBoardIndex = (_configData.BoardHeight - 1) * _configData.BoardWidth + xArrowHead;
                    step = -_configData.BoardWidth;
                    return GenerateListCell(headBoardIndex, minBoardIndex, maxBoardIndex, step);
                default:
                    return new List<int>();
            }
        }
        public int GetMovableArrowPosAndDir(out Direction direction)
        {
            var index = FindAMoveableArrow();
            if (index == -1)
            {
                direction = Direction.LEFT;
                return index;
            }
            direction = _directions[index];
            return index;
        }
        public void MoveSomeArrow(int numArrows)
        {
            for (int i = 0; i < numArrows; i++)
            {
                var index = GetMovableArrowPosAndDir(out _);
                if (index == -1)
                    return;

                MoveArrowAtIndex(index);
            }
        }
        public void BlockInteraction()
        {
            _isBgInteractBlock = true;
        }
        public void UnblockInteraction()
        {
            _isBgInteractBlock = false;
        }
        public void HideGameSceneUI()
        {
            OnHideBarTop?.Invoke();
            OnHideBoosters?.Invoke();
        }
        public void ShowGameSceneUI()
        {
            OnShowBarTop?.Invoke();
            OnShowBoosters?.Invoke();
        }
        public void EnterEraserMode()
        {
            if (_isWaitingForEraserBooster)
                return;

            _isWaitingForEraserBooster = true;
            HideGameSceneUI();
        }
        public void ExitEraserMode()
        {
            if (!_isWaitingForEraserBooster)
                return;

            _isWaitingForEraserBooster = false;
            ShowGameSceneUI();
        }
        public bool IsEraserModeTrue()
        {
            return _isWaitingForEraserBooster;
        }
        public void LoadBoardData(BoardData boardData)
        {
            _configData = boardData;
        }
        public PlayerData GetPlayerData()
        {
            return _playerData;
        }
        public void LoadPlayerData()
        {
            _playerData = _storage.Load<PlayerData>("PlayerData");
        }
        public void SetCurrentLevelIndex(int currentLvlIndex)
        {
            _currentLvl = currentLvlIndex;
        }
        public int GetCurrentLevelIndex()
        {
            return _currentLvl;
        }
        public void RestoreHeart(int amount = 1)
        {
            _heart = Mathf.Min(_heart + amount, 3);
            _isBgInteractBlock = false;
            OnTurnPopupOff?.Invoke();
            OnHeartRestored?.Invoke();
        }



        // logic
        public void Init(IEventHandler eventHandler, IPopUpManager popupManager)
        {
            //LoadBoardData();
            InputInit();
            EventHandlerInit(eventHandler, popupManager);
            MatrixesInit();
            LoadMatrixes();
            CurveCorrection();
            RegisterAction();
            ResetRuntimeState();
        }

        private void ResetRuntimeState()
        {
            _isBgInteractBlock = false;
            _isWaitingForEraserBooster = false;
            _heart = 3;
            _numAnimationSuccess = 0;
            _numAnimationFail = 0;
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
            //_isWaitingForEraserBooster = false;
            _isBgInteractBlock = false;

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
            OnReset?.Invoke();
        }

        private void EventHandlerInit(IEventHandler eventHandler, IPopUpManager popupManager)
        {
            _eventHandler = eventHandler;
            _popupManager = popupManager;
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
            //_boostersManager.OnBoosterBusyChanged += HandleBlockInput;
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

            int width = _configData.BoardWidth;
            int height = _configData.BoardHeight;
            if (pos.X >= width || pos.Y >= height) return false;

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
            var movedArrow = _configData.Arrows[_boardMatrix[boardIndex]];

            var headPos = IndexToPosition(movedArrow.ArrowIndices[0]);
            var neckPos = IndexToPosition(movedArrow.ArrowIndices[1]);

            var direction = GetDirection(headPos, neckPos);
            GetZoneWithDirection(movedArrow, direction, out int minBoardIndex, out int maxBoardIndex, out int step);
            HandleMove(_boardMatrix[boardIndex], movedArrow.ArrowIndices[0], minBoardIndex, maxBoardIndex, step);
        }

        private void GetZoneWithDirection(Arrow movedArrow, Direction direction, out int min, out int max, out int step)
        {
            int width = _configData.BoardWidth;
            int height = _configData.BoardHeight;
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

        private void HandleMove(int iConfigIndex, int headBoardIndex, int minBoardIndex, int maxBoardIndex, int step) //need to be renamed --> stupid name
        {
            if (IsABlockedPath(headBoardIndex, iConfigIndex, minBoardIndex, maxBoardIndex, step, out int currentBoardIndex))
            {
                var deltaBoardIndex = (currentBoardIndex - headBoardIndex) / step;
                var cConfigIndex = _boardMatrix[currentBoardIndex];

                BlockInteractWithArrow(iConfigIndex);
                OnMoveArrowFail?.Invoke(iConfigIndex, cConfigIndex, deltaBoardIndex);

                if (_isFirstMoveFail[iConfigIndex])
                {
                    _heart--;
                    OnLoseHeart?.Invoke();
                }

                _numAnimationFail++;
                if (AllHeartAreLost())
                    HandleLose();
            }
            else
            {
                DiableArrow(iConfigIndex);
                OnMoveArrowSuccess?.Invoke(iConfigIndex);
                _numAnimationSuccess++;
                OnArrowClicked?.Invoke(headBoardIndex);
                if (AllArrowsAreCleared())
                    HandleWin();
            }
        }

        private bool IsBlockedCell(int currentBoardIndex, int configIndex)
        {
            return _boardMatrix[currentBoardIndex] != -1
                && _boardMatrix[currentBoardIndex] != configIndex
                && _boardMatrixCheck[currentBoardIndex];
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
            _isBgInteractBlock = true;
            SaveWinResult();
            OnTurnPopupOn?.Invoke(true);
        }

        private void HandleLose()
        {
            Debug.Log("DEFEAT");
            _isBgInteractBlock = true;
            OnTurnPopupOn?.Invoke(false);
        }

        private void SaveWinResult()
        {
            if (_playerData == null || _playerData.CurrentLevelsData == null)
                return;

            var completedLevel = Array.Find(_playerData.CurrentLevelsData, l => l.LevelId == _currentLvl);
            if (completedLevel == null)
                return;

            completedLevel.LevelState = (_heart < 3) ? LevelState.NOTCOMLETED : LevelState.COMPLETED;

            int starsEarned = Mathf.Clamp(_heart, 0, 3);
            if (starsEarned > completedLevel.Star)
                completedLevel.Star = starsEarned;

            if (_playerData.CurrentLevelId == _currentLvl)
                _playerData.CurrentLevelId = _currentLvl + 1;

            _storage.Save("PlayerData", _playerData);
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
            OnHideEraserPopup?.Invoke();
            ExitEraserMode();
        }

        private int FindAMoveableArrow()
        {
            for (int i = 0; i < _configData.Arrows.Length; i++)
            {
                var arrow = _configData.Arrows[i];
                var direction = GetDirectionAtBoardIndex(arrow.ArrowIndices[0]);
                GetZoneWithDirection(arrow, direction, out int min, out int max, out int step);
                if (!IsABlockedPath(arrow.ArrowIndices[0], i, min, max, step, out _) && _boardMatrixCheck[arrow.ArrowIndices[0]]){
                    return arrow.ArrowIndices[0];
                }
            }
            return -1; 
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
            if (!_isBgInteractBlock)
                MoveArrowAtIndex(boardIndex);
        }

        
    }
}
