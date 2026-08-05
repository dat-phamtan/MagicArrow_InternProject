using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace Assets.Scripts.UI
{
    public interface IEventHandler
    {
        public event Action<Vector3> OnInteractAt;
        public event Action<int> OnUnblockInteractWidthArrow;
    }
}
