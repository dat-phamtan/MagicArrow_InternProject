using Assets.Scripts.Data;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public interface IGamePlayUI
    {
        public void Init(IEventHandler eventHandler);
        public void JumpInAnimation(GameObject obj, Action onComplete = null);
        public void JumpOutAnimation(GameObject obj, Action onComplete = null);
        public void MoveInAnimation(GameObject obj, Vector2 to);
        public void MoveOutAnimation(GameObject obj, Vector2 to);
    }
}
