using Assets.Scripts.CoreLogic;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace Assets.Scripts.BoostersGroup.Boosters
{
    public class Wand : IBooster
    {
        private float _animationDuration = 1f;
        private int _numMovedArrow = 3;
        private IController _controller;
        private IGamePlayUI _uiManager;
        private GameObject _wandAnimation;
        private Image _wandImage;
        private RectTransform _rectTransform;
        private Vector3 _vecPunchRotate;
        private Vector3 _vecLocalRotate;

        public Wand(GameObject wandAnimation)
        {
            _controller = Locator.Get<IController>();
            _uiManager = Locator.Get<IGamePlayUI>();
            _wandAnimation = wandAnimation;
            _rectTransform = _wandAnimation.GetComponent<RectTransform>();
            _vecPunchRotate = new Vector3(0, 0, 5f);
            _vecLocalRotate = new Vector3(0, 0, 30f);
        }

        public void OnClick(IController controller, Action onComplete)
        {
            HandleAnimation(onComplete);
        }

        public void Dispose() { }

        public void HandleAnimation(Action onComplete)
        {
            _uiManager.JumpInAnimation(_wandAnimation);
            Sequence uiSequence = DOTween.Sequence();

            uiSequence.Append(_rectTransform.DOPunchRotation(_vecPunchRotate, 1.5f, 10, 1f));
            uiSequence.Append(_rectTransform.DOLocalRotate(_vecLocalRotate, 0.3f).SetEase(Ease.InOutBack)).OnComplete(() =>
            {
                _controller.MoveSomeArrow(_numMovedArrow);
                onComplete?.Invoke();
                _uiManager.JumpOutAnimation(_wandAnimation);
            });
            uiSequence.Append(_rectTransform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.OutCubic));
        }
    }
}
