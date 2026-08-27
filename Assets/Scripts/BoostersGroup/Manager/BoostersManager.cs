using Assets.Scripts.BoostersGroup.Boosters;
using Assets.Scripts.CoreLogic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Boosters.BoostersManager
{
    public class BoostersManager : IBoostersManager
    {
        private IBoosterAction _boosterAction;
        private IController _controller;
        private bool _isBusy = false;

        public event Action<bool> OnBoosterBusyChanged;

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
            if (_isBusy) 
                return;

            SetBusy(true);
            booster.OnClick(_controller, () => SetBusy(false));
        }
        
        private void SetBusy(bool isBusy)
        {
            _isBusy = isBusy;
            OnBoosterBusyChanged?.Invoke(isBusy);
            if (isBusy)
                _controller.BlockInteraction();
            else
                _controller.UnblockInteraction();
        }
    }
}
