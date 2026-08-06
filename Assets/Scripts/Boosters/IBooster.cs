using Assets.Scripts.CoreLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Boosters
{
    public interface IBooster
    {
        public void OnClick(IController controller);
    }
}
