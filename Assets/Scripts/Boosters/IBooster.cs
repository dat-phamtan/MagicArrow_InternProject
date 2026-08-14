using Assets.Scripts.CoreLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Boosters
{
    public interface IBooster
    {
        //public event Action OnArrowClicked;
        public void OnClick(IController controller);
        //public void OnReset();
    }
}
