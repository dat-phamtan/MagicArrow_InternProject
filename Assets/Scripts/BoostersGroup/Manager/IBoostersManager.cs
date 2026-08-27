using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Boosters.BoostersManager
{
    public interface IBoostersManager
    {
        public event Action<bool> OnBoosterBusyChanged;
        public void Init(IBoosterAction boosterAction);
    }
}
