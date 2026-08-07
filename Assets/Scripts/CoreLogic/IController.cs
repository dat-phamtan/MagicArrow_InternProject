using Assets.Scripts.Config;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
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

        public void Init(IEventHandler eventHandler);
        public List<int> GetArrowMatrix();
        public PartType GetArrowTypeAtPosition(Position pos);
        public ConfigData GetConfigData();
        public Direction GetDirectionAtPosition(Position pos);
        public Direction GetDirectionAtBoardIndex(int boardIndex);
        public void ChangeEraserUsedMode();
        public void DiableArrow(int configIndex);
        public bool IsFirstMoveFail(int configIndex);
        public int GetConfigIndexAt(int boardIndex);

    }
}
