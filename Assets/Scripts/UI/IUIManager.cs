using Assets.Scripts.Data;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public interface IUIManager
    {
        public void Init(IEventHandler eventHandler);
        public void PlayJumpInAnimation(GameObject obj);
        public void PlayJumpOutAnimation(GameObject obj);
        public void PlaySlideInAnimation(GameObject obj, Vector2 to);
        public void PlaySlideOutAnimation(GameObject obj, Vector2 to);
    }
}
