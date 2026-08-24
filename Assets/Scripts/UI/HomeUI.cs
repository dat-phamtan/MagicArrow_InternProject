using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly List<RectTransform> _items = new();
        private readonly List<int> _itemsIndices = new();

        private float _snapDuration = 1f;
        private float _velocityThreshold = 20f;

        public event Action<int> OnSnappedAt;

        public HomeUI(){}

        public void ScrollSnapInit( ScrollRect levels, 
                                    RectTransform snapTarget,
                                    float snapDuration = 0.3f, 
                                    float velocityThreshold = 20f)
        {
            _content?.DOKill();
            //_levels = levels;
            _snapTarget = snapTarget;
            _snapDuration = snapDuration;
            _velocityThreshold = velocityThreshold;

            _scrollRect = levels;
            _content = _scrollRect.content;
            ClearItem();
        }

        public void RegisterItem(int index, RectTransform item)
        {
            _items.Add(item);
            _itemsIndices.Add(index);
        }

        public void ClearItem()
        {
            _items.Clear();
        }

        public IEnumerator Snap()
        {
            while (_scrollRect.velocity.magnitude > _velocityThreshold)
            {
                //Debug.Log($"{ _scrollRect.velocity.magnitude}");
                yield return null; 
            }
            SnapToNearest();
        }

        //prepare for init snap 
        public void MoveContentToPosition(float target)
        {
            _content.DOAnchorPosY(target, _snapDuration).SetEase(Ease.OutCubic);
        }

        private void SnapToNearest()
        {
            if (_items.Count == 0)
                return;

            RectTransform nearest = null;
            int nearestIndex = -1;
            float minDist = float.MaxValue;
            float targetLocalY = GetRectTransformLocalY(_snapTarget);

            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i] == null)
                {
                    _items.RemoveAt(i);
                    _itemsIndices.RemoveAt(i);
                    continue;
                }

                float itemLocalY = GetRectTransformLocalY(_items[i]);
                float dist = Mathf.Abs(itemLocalY - targetLocalY);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = _items[i];
                    nearestIndex = _itemsIndices[i];
                }
            }

            if (nearest == null)
                return;

            OnSnappedAt?.Invoke(nearestIndex);

            float nearestLocalY = GetRectTransformLocalY(nearest);
            float deltaY = targetLocalY - nearestLocalY;

            _scrollRect.velocity = Vector2.zero;
            _scrollRect.StopMovement();
            float targetY = _content.anchoredPosition.y + deltaY;
            _content.DOKill();
            MoveContentToPosition(targetY);
        }

        private float GetRectTransformLocalY(RectTransform item)
        {
            return _scrollRect.viewport.InverseTransformPoint(item.position).y;
        }
    }
}
