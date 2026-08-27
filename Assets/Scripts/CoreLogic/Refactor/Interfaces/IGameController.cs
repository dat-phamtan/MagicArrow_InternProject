using Assets.Scripts.Data;
using Assets.Scripts.Scenes.Helper;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Animations;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.CoreLogic.Refactor.Interfaces
{
    public interface IGameController
    {
        public event Action<int> OnMoveArrowSuccess;
        public event Action<int, int, int> OnMoveArrowFail;
        public event Action<int> OnEraseArrowAt;
        public event Action<bool> OnTurnPopupOn;
        public event Action OnTurnPopupOff;
        public event Action OnRendererBoard;
        public event Action<int> OnArrowClicked;
        public event Action OnReset;
        public event Action OnLoseHeart;
        public event Action OnHeartRestored;

        public IBoardState Board { get; }
        public IGameRules Rules { get; }
        public IPlayerProgress Progress { get; }

        public bool IsInteractionBlocked {  get; }
        public bool IsEraserMode {  get; }

        public void Init(IEventHandler eventHandler, IPopUpManager popupManager);
        public void BlockInteraction();
        public void UnblockInteraction();
        public void EnterEraserMode();
        public void ExitEraserMode();
        public void MoveSomeArrows(int count);
        public void LoadBoard(BoardData data);
    }
}
