using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public interface IUIManager
    {
        public void Init(IEventHandler eventHandler);
    }
}
