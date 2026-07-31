using Assets.Scripts.Config;
using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;

namespace Assets.Scripts.CoreLogic
{
    public interface IController
    {
        public event Action OnGridInit;

        public List<int> GetArrowMatrix();
        public PartType GetArrowTypeAtPosition(Position pos);
        public ConfigData GetConfigData();
        public Direction GetDirectionAtPosition(Position pos);
    }
}
