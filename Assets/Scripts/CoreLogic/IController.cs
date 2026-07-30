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
        public PartType GetArrowTypeAtPosition(int x, int y);
        public ConfigData GetConfigData();
    }
}
