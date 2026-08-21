using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public interface IHomeUI
    {
        public event Action<int> OnSnappedAt;
        public void ScrollSnapInit(ScrollRect levels, RectTransform snapTarget, float snapDuration = 0.3f, float velocityThreshold = 20f);
        public void RegisterItem(int index, RectTransform item);
        public void ClearItem();
        public IEnumerator Snap();
    }
}
