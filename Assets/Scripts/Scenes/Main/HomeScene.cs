using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Sound;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System;
using TMPro;
using Unity.VisualScripting;
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

    //setting
    public Button settingBtn;
    public GameObject blackBg;
    public GameObject settingsPopup;
    public GameObject languagePopup;

    public Button languageBtn;
    public Button musicBtn;
    public Button soundEffectBtn;
    public Button[] exitBtns;


    public float levelSpacing = 50f;
    public GameObject levels;
    public RectTransform snapTarget;
    
    private InputSystem_Actions _inputActions;
    private IController _controller;
    private IHomeUI _homeUI;
    private IGamePlayUI _uiManager;
    private ISoundManager _soundManager;
    private PlayerData _playerData;
    private int _currentIndex = -1;
    private bool _isLangugePopup = false;


    public void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }

    public void OnEnable()
    {
        _inputActions.Enable();
        settingBtn.onClick.AddListener(HandleShowSettings);
        languageBtn.onClick.AddListener(HandleShowLanguage);
        musicBtn.onClick.AddListener(HandleMusicBtnClicked);
        soundEffectBtn.onClick.AddListener(HandleSfxBtnClicked);
        foreach (var exit in exitBtns)
            exit.onClick.AddListener(HandleExit);
    }

    public void OnDisable()
    {
        _inputActions.Disable();
        settingBtn.onClick.RemoveListener(HandleShowSettings);
        languageBtn.onClick.RemoveListener(HandleShowLanguage);
        musicBtn.onClick.RemoveListener(HandleMusicBtnClicked);
        soundEffectBtn.onClick.RemoveListener(HandleSfxBtnClicked);
        foreach (var exit in exitBtns)
            exit.onClick.RemoveListener(HandleExit);
        if (_homeUI != null)
            _homeUI.OnSnappedAt -= HandleSnapped;
    }

    public void Start()
    {
        _controller = Locator.Get<IController>();
        _homeUI = Locator.Get<IHomeUI>();
        _uiManager = Locator.Get<IGamePlayUI>();
        _soundManager = Locator.Get<ISoundManager>();
        _playerData = _controller.GetPlayerData();

        //sound
        Locator.Get<ISoundManager>().PlayMusic(MusicId.HomeTheme);

        _homeUI.OnSnappedAt += HandleSnapped;
        greenBtn.GetComponentInChildren<Button>().onClick.AddListener(HandleHomeBtnClicked);
        orangeBtn.GetComponentInChildren<Button>().onClick.AddListener(HandleHomeBtnClicked);

        coinValue.text = _playerData.Gold.ToString();
        heartValue.text = _playerData.Heart.ToString();
        starValue.text = _playerData.Star.ToString();
        heartRegenTime.text = _playerData.RegenHour.ToString() + ":" + _playerData.RegenMinute.ToString();

        HandleSnapped(_playerData.CurrentLevelId);
    }

    private void HandleMusicBtnClicked()
    {
        _soundManager.PlaySfx(SfxId.ButtonClick);
        var disable = musicBtn.transform.Find("Diable").gameObject;
        disable.SetActive(!disable.activeInHierarchy);
        _soundManager.SetMusicMuted(!_soundManager.IsMuteMusic);
    }

    private void HandleSfxBtnClicked()
    {
        _soundManager.PlaySfx(SfxId.ButtonClick);
        var disable = soundEffectBtn.transform.Find("Diable").gameObject;
        disable.SetActive(!disable.activeInHierarchy);
        _soundManager.SetSfxMuted(!_soundManager.IsMuteSoundEffect);
    }

    private void HandleExit()
    {
        _soundManager.PlaySfx(SfxId.ButtonClick);
        if (_isLangugePopup)
        {
            _uiManager.JumpOutAnimation(languagePopup);
            _isLangugePopup = false;
            return;
        }
            
        if (settingsPopup.activeInHierarchy)
            _uiManager.JumpOutAnimation(settingsPopup);
        blackBg.SetActive(false);
    }

    private void HandleShowSettings()
    {
        _soundManager.PlaySfx(SfxId.ButtonClick);
        blackBg.SetActive(true);
        _uiManager.JumpInAnimation(settingsPopup);
    }

    private void HandleShowLanguage()
    {
        //_uiManager.JumpOutAnimation(settingsPopup);
        _soundManager.PlaySfx(SfxId.ButtonClick);
        _uiManager.JumpInAnimation(languagePopup);
        _isLangugePopup = true;
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
            //greenBtn.GetComponentInChildren<Button>().onClick.AddListener(HandleHomeBtnClicked);
            orangeBtn.SetActive(false);
            var texts = greenBtn.GetComponentsInChildren<TextMeshProUGUI>();
            texts[0].text = "Replay";
            texts[1].text = "Level " + snappedLevelData.LevelId.ToString();
        }
        else
        {
            orangeBtn.SetActive(true);
            //orangeBtn.GetComponentInChildren<Button>().onClick.AddListener(HandleHomeBtnClicked);
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
        var btn = greenBtn.GetComponentInChildren<Button>();
        btn.onClick.RemoveListener(HandleHomeBtnClicked);
        btn.onClick.AddListener(HandleHomeBtnClicked);
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
        _soundManager.PlaySfx(SfxId.ButtonClick);
        var levelData = GetLevelDataAt(_currentIndex);
        _controller.LoadBoardData(levelData.BoardData);
        DG.Tweening.DOTween.KillAll();
        Locator.Get<ISoundManager>().StopMusic();
        SceneManager.LoadSceneAsync("Transition");
    }
    
}
