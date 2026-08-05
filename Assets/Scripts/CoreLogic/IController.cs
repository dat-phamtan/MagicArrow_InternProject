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
        //public event Action OnGridInit;
        public event Action<int> OnMoveArrowSuccess;
        public event Action<int, int> OnMoveArrowFail;

        public void Init(IEventHandler eventHandler);
        public List<int> GetArrowMatrix();
        public PartType GetArrowTypeAtPosition(Position pos);
        public ConfigData GetConfigData();
        public Direction GetDirectionAtPosition(Position pos);


        //temp for test
        public void UnblockInteractWithArrow(int matrixIndex);
    }
}
