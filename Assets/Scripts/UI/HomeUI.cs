using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class HomeUI : IHomeUI
    {
        private GameObject _levels;
        private RectTransform _snapTarget;
        private ScrollRect _scrollRect;
        private RectTransform _content;
        //private Coroutine _snapCoroutine;
        private List<RectTransform> _items = new();

        private float _snapDuration = 0.3f;
        private float _velocityThreshold = 20f;

        public HomeUI(){}

        public void ScrollSnapInit(GameObject levels, RectTransform snapTarget, float snapDuration = 0.3f, float velocityThreshold = 20f)
        {
            _levels = levels;
            _snapTarget = snapTarget;
            _snapDuration = snapDuration;
            _velocityThreshold = velocityThreshold;

            _scrollRect = _levels.GetComponent<ScrollRect>();
            _content = _scrollRect.content;
        }

        public void RegisterItem(RectTransform item)
        {
            _items.Add(item);
        }

        public void ClearItem()
        {
            _items.Clear();
        }

        public IEnumerator Snap()
        {
            while (_scrollRect.velocity.magnitude > _velocityThreshold)
                yield return null;
            SnapToNearest();
        }

        private void SnapToNearest()
        {
            if (_items.Count == 0)
                return;

            RectTransform nearest = null;
            float minDist = float.MaxValue;
            float targetLocalY = GetRectTransformLocalY(_snapTarget);

            foreach (var item in _items)
            {
                float itemLocalY = GetRectTransformLocalY(item);
                float dist = Mathf.Abs(itemLocalY - targetLocalY);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = item;
                }

                float nearestLocalY = GetRectTransformLocalY(nearest);
                float deltaY = targetLocalY - nearestLocalY;

                _scrollRect.velocity = Vector2.zero;
                _scrollRect.StopMovement();
                float targetY = _content.anchoredPosition.y + deltaY;
                _content.DOKill();
                _content.DOAnchorPosY(targetY, _snapDuration).SetEase(Ease.OutCubic);
            }
        }

        private float GetRectTransformLocalY(RectTransform item)
        {
            return _scrollRect.viewport.InverseTransformPoint(item.position).y;
        }
    }
}
