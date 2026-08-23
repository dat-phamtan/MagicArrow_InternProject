using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HomeScene : MonoBehaviour
{
    public TextMeshProUGUI coinValue;
    public TextMeshProUGUI heartValue;
    public TextMeshProUGUI starValue;
    public TextMeshProUGUI heartRegenTime;


    public GameObject greenBtn;
    public GameObject orangeBtn;

    public GameObject redLabel;
    public GameObject blueLabel;
    public GameObject purpleLabel;
    public TextMeshProUGUI stateText;

    public float levelSpacing = 50f;

    public GameObject levels;
    public RectTransform snapTarget;
    
    private InputSystem_Actions _inputActions;
    private IController _controller;
    private IHomeUI _homeUI;
    private PlayerData _playerData;
    private int _currentIndex = -1;


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

        _homeUI.OnSnappedAt += HandleSnapped;

        coinValue.text = _playerData.Gold.ToString();
        heartValue.text = _playerData.Heart.ToString();
        starValue.text = _playerData.Star.ToString();
        heartRegenTime.text = _playerData.RegenHour.ToString() + ":" + _playerData.RegenMinute.ToString();

        HandleSnapped(_playerData.CurrentLevelId);
    }

    private void HandleSnapped(int index)
    {
        //Debug.Log(index);
        _currentIndex = index;
        var snappedLevelData = new LevelData();
       
        if (index >= _playerData.CurrentLevelId)
        {
            _currentIndex = _playerData.CurrentLevelId;
            snappedLevelData = GetLevelDataAt(_playerData.CurrentLevelId);
            HandleUnplayLevelSnapped();
            HandleLabel(snappedLevelData.Hardness, snappedLevelData.LevelState);
            return;
        }
        snappedLevelData = GetLevelDataAt(index);

        if (snappedLevelData.LevelState == LevelState.COMPLETED)
        {
            greenBtn.SetActive(true);
            greenBtn.GetComponentInChildren<Button>().onClick.AddListener(HandleHomeBtnClicked);
            orangeBtn.SetActive(false);
            var texts = greenBtn.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = "Replay";
            texts[1].text = "Level " + snappedLevelData.LevelId.ToString();
        }
        else
        {
            orangeBtn.SetActive(true);
            orangeBtn.GetComponentInChildren<Button>().onClick.AddListener(HandleHomeBtnClicked);
            greenBtn.SetActive(false);
            var texts = orangeBtn.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = "Replay";
            texts[1].text = "Level " + snappedLevelData.LevelId.ToString();
        }

        HandleLabel(snappedLevelData.Hardness, snappedLevelData.LevelState);
    }

    private LevelData GetLevelDataAt(int index)
    {
        for (int i = 0; i < _playerData.CurrentLevelsData.Length; i++)
            if (_playerData.CurrentLevelsData[i].LevelId == index)
                return _playerData.CurrentLevelsData[i];
        return _playerData.CurrentLevelsData[0];
    }

    private void HandleUnplayLevelSnapped()
    {
        greenBtn.SetActive(true);
        greenBtn.GetComponentInChildren<Button>().onClick.AddListener(HandleHomeBtnClicked);
        orangeBtn.SetActive(false);
        var texts = greenBtn.GetComponentsInChildren<TextMeshProUGUI>();
        texts[0].text = "Play";
        texts[1].text = "Level " + _playerData.CurrentLevelId.ToString();
    }

    private void HandleLabel(Hardness hardness, LevelState levelState)
    {
        HandleLabelBar(hardness, levelState);
        HandleLabelText(hardness, levelState);
    }

    private void HandleLabelBar(Hardness hardness, LevelState levelState)
    {
        switch (hardness)
        {
            case Hardness.SUPERHARD:
                purpleLabel.SetActive(true);
                redLabel.SetActive(false);
                blueLabel.SetActive(false);
                break;
            case Hardness.HARD:
                redLabel.SetActive(true);
                purpleLabel.SetActive(false);
                blueLabel.SetActive(false);
                break;
            case Hardness.NORMAL:
                if (levelState == LevelState.NOTCOMLETED)
                {
                    blueLabel.SetActive(false);
                    redLabel.SetActive(false);
                    purpleLabel.SetActive(false);
                    break;
                }
                blueLabel.SetActive(true);
                redLabel.SetActive(false);
                purpleLabel.SetActive(false);
                break;
        }
    }

    private void HandleLabelText(Hardness hardness, LevelState levelState)
    {
        switch (hardness)
        {
            case Hardness.SUPERHARD:
                stateText.text = "Super Hard";
                break;
            case Hardness.HARD:
                stateText.text = "Hard";
                break;
            case Hardness.NORMAL:
                if (levelState == LevelState.NOTCOMLETED)
                {
                    stateText.text = "";
                    break;
                }
                stateText.text = "Completed";
                break;
        }
    }

    private void HandleHomeBtnClicked()
    {
        var levelData = GetLevelDataAt(_currentIndex);
        _controller.LoadBoardData(levelData.BoardData);
        Debug.Log(levelData);
        SceneManager.LoadSceneAsync("Transition");
    }
    
}
