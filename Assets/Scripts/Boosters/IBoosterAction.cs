using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Boosters
{
    public interface IBoosterAction
    {
        public event Action<IBooster> OnBoosterClicked;
    }
}
