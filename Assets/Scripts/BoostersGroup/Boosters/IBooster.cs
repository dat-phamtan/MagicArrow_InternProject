using Assets.Scripts.CoreLogic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.BoostersGroup.Boosters
{
    public interface IBooster
    {
        public void OnClick(IController controller, Action onComplete);
        public void Dispose();
    }
}
