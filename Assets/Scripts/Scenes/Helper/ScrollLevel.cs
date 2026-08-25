using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class SnapHandler : MonoBehaviour, IEndDragHandler
{
    public Transform contentTransform;
    public GameObject grayPodium;
    public GameObject greenPodium;
    public GameObject orangePodium;
    public GameObject currentPodium;
    public RectTransform snapTarget;

    public float snapDuration = 0.3f;
    public float velocityThreshold = 20f;

    private const string SnapPointName = "SnapPoint";

    private Coroutine _snapCoroutine;
    private PlayerData _playerData;
    private IController _controller;
    private IHomeUI _homeUI;

    private readonly int maxNumStar = 3;
    private int _numLevel;

    void Start()
    {
        _homeUI = Locator.Get<IHomeUI>();
        _controller = Locator.Get<IController>();
        _playerData = _controller.GetPlayerData();
        _numLevel = _playerData.CurrentLevelsData.Length;
        _homeUI.ClearItem();
        _homeUI.ScrollSnapInit(GetComponent<ScrollRect>(), snapTarget, snapDuration, velocityThreshold);
        GenerateLevelList();
    }

    private void OnDestroy()
    {
        if (_snapCoroutine != null)
            StopCoroutine(_snapCoroutine);

        if (_homeUI != null)
            _homeUI.ClearItem();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_snapCoroutine != null)
            StopCoroutine(_snapCoroutine);
        _snapCoroutine = StartCoroutine(_homeUI.Snap());
    }

    public void GenerateLevelList()
    {
        var levelData = _playerData.CurrentLevelsData;
        for (int i = _numLevel - 1; i >= 0; i--) //<---- it reversed
        {
            GameObject currentPrefab = levelData[i].LevelState switch
            {
                LevelState.UNPLAYED => grayPodium,
                LevelState.COMPLETED => greenPodium,
                LevelState.NOTCOMLETED => orangePodium,
                _ => greenPodium,
            };

            if (levelData[i].LevelId == _playerData.CurrentLevelId)
                currentPrefab = currentPodium;

            var newLevelItem = Instantiate(currentPrefab, contentTransform);
            newLevelItem.name = "Level_" + (i + 1);
            newLevelItem.GetComponentInChildren<Button>();

            RectTransform snapAnchor = FindSnapPoint(newLevelItem.transform);
            _homeUI.RegisterItem(levelData[i].LevelId, snapAnchor);
            //_homeUI.RegisterItem(levelData[i].LevelId, newLevelItem.GetComponent<RectTransform>());

            var text = newLevelItem.GetComponentInChildren<TextMeshProUGUI>();
            text.text = levelData[i].LevelId.ToString();

            if (levelData[i].LevelState == LevelState.UNPLAYED)
                continue;

            var stars = newLevelItem.GetComponentsInChildren<Image>();
            List<Image> fillStars = new();
            for (int j = 0; j < stars.Length; j++)
                if (stars[j].CompareTag("FillStar"))
                    fillStars.Add(stars[j]);

            for (int k = 0; k < maxNumStar - levelData[i].Star; k++)
            {
                fillStars[k].enabled = false;
            }
        }
    }

    private RectTransform FindSnapPoint(Transform root)
    {
        var found = root.Find(SnapPointName) as RectTransform;
        if (found != null)
            return found;

        Debug.LogWarning($"[SnapHandler] Prefab '{root.name}' thiếu child '{SnapPointName}', tạm dùng root làm điểm snap (có thể bị lệch).");
        return root.GetComponent<RectTransform>();
    }
}