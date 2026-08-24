using Assets.Scripts.CoreLogic;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Boosters
{
    public class Eraser : IBooster
    {
        private IGamePlayUI _uiManager;
        public Eraser()
        {
            _uiManager = Locator.Get<IGamePlayUI>();
        }

        public void OnClick(IController controller, Action onComplete)
        {
            //hide topbar and boosters bar
            if (controller.IsEraserModeTrue())
            {
                onComplete?.Invoke();
                return;
            }
            controller.EnterEraserMode();
            onComplete?.Invoke();
        }

        public void Dispose() { }
    }
}
