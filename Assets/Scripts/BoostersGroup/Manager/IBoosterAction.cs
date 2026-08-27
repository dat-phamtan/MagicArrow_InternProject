using Assets.Scripts.BoostersGroup.Boosters;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Boosters.BoostersManager
{
    public interface IBoosterAction
    {
        public event Action<IBooster> OnBoosterClicked;
    }
}
