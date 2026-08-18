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
        private IUIManager _uiManager;
        public Eraser()
        {
            _uiManager = Locator.Get<IUIManager>();
        }

        public void OnClick(IController controller, Action onComplete)
        {
            //hide topbar and boosters bar
            controller.ChangeEraserUsedMode();
            controller.HideGameSceneUI();
            onComplete?.Invoke();
            //_uiManager.HideUI()
            //show notification bar
            //disable arrow in boardmatrix
            //play delete animation <-- just simple for now ;))) --> disapear
        }
    }
}
