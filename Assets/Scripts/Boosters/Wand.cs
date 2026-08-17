using Assets.Scripts.CoreLogic;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Boosters
{
    public class Wand :  IBooster
    {
        private int _numMovedArrow = 3;
        private IController _controller;

        public Wand()
        {
            _controller = Locator.Get<IController>();

        }

        public void OnClick(IController controller, Action onComplete)
        {
            _controller.MoveSomeArrow(_numMovedArrow);
        }
    }
}
