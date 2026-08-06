using Assets.Scripts.CoreLogic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Boosters
{
    public class BoostersManager : IBoostersManager
    {
        private IBoosterAction _boosterAction;
        private IController _controller;

        public BoostersManager(IController controller)
        {
            _controller = controller;
        }
        
        public void Init(IBoosterAction boosterAction)
        {
            _boosterAction = boosterAction;
            _boosterAction.OnBoosterClicked += HandleBoosterClick;
        }

        private void HandleBoosterClick(IBooster booster)
        {
            booster.OnClick(_controller);
        }
        
    }
}
