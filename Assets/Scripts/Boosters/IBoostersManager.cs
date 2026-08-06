using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Boosters
{
    public interface IBoostersManager
    {
        public void Init(IBoosterAction boosterAction);
    }
}
