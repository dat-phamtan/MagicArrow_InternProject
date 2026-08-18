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
        public void ShowUI(GameObject obj);
        public void HideUI(GameObject obj);
        public void ShowTopBar(GameObject obj, Vector2 to);
        public void HideTopBar(GameObject obj, Vector2 to);
    }
}
