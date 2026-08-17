using System;
using System.Collections.Generic;

using System.Text;
using UnityEngine;

namespace Assets.Scripts.Ultility
{
    public interface ICamera
    {
        public void FocusOnPos(Vector3 pos);
    }
}
