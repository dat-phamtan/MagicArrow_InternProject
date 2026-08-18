using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.UI
{
    public class UIManager : IUIManager
    {
        public float boostersAnimation = 0.3f;
        public float barsAnimation = 1f;

        private IInput _input;
        private IController _controller;
        private IEventHandler _eventHandler;

        private ConfigData _configData;
        private Dictionary<int, GameObject> _arrowRoots;
        private Dictionary<int, ArrowMeshBuilder> _arrowBuilders;
        private Dictionary<int, Vector3[]> _arrowPaths;
        private Dictionary<int, Vector3[]> _curvedPath;
        private Dictionary<int, float[]> _cumulativeLength;

        public UIManager(IController controller, IInput input, float spacing)
        {
            _input = input;
            _controller = controller;
            //_spacing = spacing;
        }

        public void Init(IEventHandler eventHandler)
        {
            _eventHandler = eventHandler;
            _eventHandler.OnInteractAt += HandleInteractAt;
        }

        private void HandleInteractAt(Vector3 pos)
        {
            _input.HandleInput(pos);
        }

        public void ShowUI(GameObject obj)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            var rectTransform = obj.GetComponent<RectTransform>();

            obj.SetActive(true);

            rectTransform.localScale = Vector3.zero;
            canvasGroup.alpha = 0f;

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            rectTransform.DOScale(Vector3.one, boostersAnimation).SetEase(Ease.OutBack);
            canvasGroup.DOFade(1f, boostersAnimation).SetEase(Ease.OutQuad);
        }
        
        public void HideUI(GameObject obj)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            var rectTransform = obj.GetComponent<RectTransform>();

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            
            rectTransform.DOScale(Vector3.zero, boostersAnimation).SetEase(Ease.InBack);
            canvasGroup.DOFade(0f, boostersAnimation).SetEase(Ease.InQuad).OnComplete(() =>
            {
                obj.SetActive(false);
            });
        }

        public void ShowTopBar(GameObject obj, Vector2 to)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            var rectTransform = obj.GetComponent<RectTransform>();

            obj.SetActive(true);

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            rectTransform.DOAnchorPos(to, barsAnimation).SetEase(Ease.OutBack);
        }

        public void HideTopBar(GameObject obj, Vector2 to)
        {
            var canvasGroup = obj.GetComponent<CanvasGroup>();
            var rectTransform = obj.GetComponent<RectTransform>();

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            rectTransform.DOAnchorPos(to, barsAnimation).SetEase(Ease.OutBack).OnComplete(() =>
            {
                obj.SetActive(false);
            });
        }
    }
}

