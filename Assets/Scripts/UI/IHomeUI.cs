using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public interface IHomeUI
    {
        public void ScrollSnapInit(GameObject levels, RectTransform snapTarget, float snapDuration = 0.3f, float velocityThreshold = 20f);
        public void RegisterItem(RectTransform item);
        public void ClearItem();
        public IEnumerator Snap();
    }
}
