using Assets.Scripts.CoreLogic;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Scenes.Helper
{
    public interface IPopUpManager
    {
        public event Action OnPlayAgain;
        public void Init(IController controller, IEventHandler eventHandler);
    }
}
