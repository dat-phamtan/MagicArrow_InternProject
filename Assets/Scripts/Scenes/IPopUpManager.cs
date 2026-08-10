using Assets.Scripts.CoreLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Scenes
{
    public interface IPopUpManager
    {
        public event Action OnPlayAgain;
        public void Init(IController controller);
    }
}
