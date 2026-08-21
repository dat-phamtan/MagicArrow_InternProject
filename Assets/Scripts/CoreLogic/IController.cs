using Assets.Scripts.Config;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using Assets.Scripts.Scenes;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;

namespace Assets.Scripts.CoreLogic
{
    public interface IController
    {
        public event Action<int> OnMoveArrowSuccess;
        public event Action<int, int, int> OnMoveArrowFail;
        public event Action<int> OnEraseArrowAt;
        public event Action<bool> OnTurnPopupOn;
        public event Action OnTurnPopupOff;
        public event Action OnRerenderBoard;
        public event Action<int> OnArrowClicked;
        public event Action OnReset;
        public event Action OnLoseHeart;

        public event Action OnHideBarTop;
        public event Action OnShowBarTop;
        public event Action OnHideBoosters;
        public event Action OnShowBoosters;
        public event Action OnHideEraserPopup;
        

        public void Init(IEventHandler eventHandler, IPopUpManager popupManager);
        public List<int> GetArrowMatrix();
        public PartType GetArrowTypeAtPosition(Position pos);
        public BoardData GetConfigData();
        public Direction GetDirectionAtPosition(Position pos);
        public Direction GetDirectionAtBoardIndex(int boardIndex);
        public void DiableArrow(int configIndex);
        public bool IsFirstMoveFail(int configIndex);
        public int GetConfigIndexAt(int boardIndex);
        public int GetSuccessAnimationNum();
        public int GetFailAnimationNum();
        public int GetHeart();
        public float GetSpacing(); 
        public bool IsOccupiedCell(int boardIndex);
        public bool IsBgInteractionBlocked();
        public bool IsArrowExisted(int boardIndex);
        public List<int> GetNextCells(int yArrowHead, int xArrowHead, Direction direction);
        public int GetMovableArrowPosAndDir(out Direction direction);
        public void MoveSomeArrow(int numArrows);
        public void BlockInteraction();
        public void UnblockInteraction();
        public void HideGameSceneUI();
        public void ShowGameSceneUI();
        public void EnterEraserMode();
        public void ExitEraserMode();
        public bool IsEraserModeTrue();
        public void LoadBoardData(BoardData boardData);
        public PlayerData GetPlayerData();
        public void LoadPlayerData();
    }
}
