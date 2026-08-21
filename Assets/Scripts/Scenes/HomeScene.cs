using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HomeScene : MonoBehaviour, IEndDragHandler
{
    public TextMeshProUGUI coinValue;
    public TextMeshProUGUI heartValue;
    public TextMeshProUGUI starValue;
    public TextMeshProUGUI heartRegenTime;
    public GameObject currentLevel;

    public Transform contentTransform;
    public GameObject grayPodium;
    public GameObject greenPodium;
    public GameObject orangePodium;

    public float levelSpacing = 50f;

    public GameObject levels;
    public RectTransform snapTarget;

    private int maxNumStar = 3;
    private int _numLevel;
    private InputSystem_Actions _inputActions;
    private IController _controller;
    private IHomeUI _homeUI;
    private PlayerData _playerData;
    private Coroutine _snapCoroutine;


    public void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }

    public void OnEnable()
    {
        _inputActions.Enable();
    }

    public void OnDisable()
    {
        _inputActions.Disable();
    }

    public void Start()
    {
        _controller = Locator.Get<IController>();
        _homeUI = Locator.Get<IHomeUI>();
        _playerData = _controller.GetPlayerData();
        _numLevel = _playerData.CurrentLevelsData.Length;

        coinValue.text = _playerData.Gold.ToString();
        heartValue.text = _playerData.Heart.ToString();
        starValue.text = _playerData.Star.ToString();
        heartRegenTime.text = _playerData.RegenHour.ToString() + ":" + _playerData.RegenMinute.ToString();

        _homeUI.ScrollSnapInit(levels, snapTarget);
        GenerateLevelList();

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_snapCoroutine != null)
            StopCoroutine(_snapCoroutine);
        _snapCoroutine = StartCoroutine(_homeUI.Snap());
    }

    void GenerateLevelList()
    {
        var levelData = _playerData.CurrentLevelsData;
        for (int i = _numLevel - 1; i >= 0; i--)
        {
            GameObject currentPrefab = levelData[i].LevelState switch
            {
                LevelState.UNPLAYED => grayPodium,
                LevelState.COMPLETED => greenPodium,
                LevelState.NOTCOMLETED => orangePodium,
                _ => greenPodium,
            };

            var newLevelItem = Instantiate(currentPrefab, contentTransform);
            newLevelItem.name = "Level_" + (i + 1);
            _homeUI.RegisterItem(newLevelItem.GetComponent<RectTransform>());

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


}
