using Assets.Scripts.Boosters;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BarTopManager : MonoBehaviour
{
    public GameObject[] stars;
    public int numHeart = 3;
    public int yPosShow = -200;
    public int yPosHide = 300;
    public GameObject levelNum;
    public Button pauseBtn;

    //pause pannel
    public GameObject pausePopup;
    public Button resumeBtn;
    public Button playAgainBtn;
    public Button returnHomeBtn;
    public Button soundEffectBtn;
    public Button musicBtn;
    public Button vibrateBtn;
    public Button lightModeBtn;
    public GameObject blackBg;
    public GameObject topBar;

    private Vector2 _basePos;
    private Vector2 _hidePos;
    private IController _controller;
    private IGamePlayUI _uiManager;
    private IBoostersManager _boostersManager;

    private void OnEnable()
    {
        pauseBtn.onClick.AddListener(() => { HandlePauseClicked(); });
        resumeBtn.onClick.AddListener(() => { HandleResume(); });
    }

    public void Start()
    {
        _controller = Locator.Get<IController>();
        _uiManager = Locator.Get<IGamePlayUI>();
        _boostersManager = Locator.Get<IBoostersManager>();
        _controller.OnLoseHeart += HandleLoseHeart;
        _controller.OnReset += HandleReset;
        _controller.OnShowBarTop += HandleShowBarTop;
        _controller.OnHideBarTop += HandleHideBarTop;
        _boostersManager.OnBoosterBusyChanged += HandleBusyChanged;
        PositionInit();
        HandleShowBarTop();
        
    }

    private void HandlePauseClicked()
    {
        _uiManager.JumpInAnimation(pausePopup);
        _controller.BlockInteraction();
        blackBg.SetActive(true);
    }

    private void HandleResume()
    {
        _uiManager.JumpOutAnimation(pausePopup);
        _controller.UnblockInteraction();
        blackBg.SetActive(false);
    }

    private void HandleBusyChanged(bool isBusy)
    {
        pauseBtn.interactable = !isBusy;
    }

    private void PositionInit() 
    {
        _basePos = new Vector2(0, yPosShow);
        _hidePos = new Vector2(0, yPosHide);
    }

    private void HandleHideBarTop()
    {
        _uiManager.MoveOutAnimation(topBar, _hidePos);
    }

    private void HandleShowBarTop()
    {
        _uiManager.MoveInAnimation(topBar, _basePos);
    }

    private void HandleLoseHeart()
    {
        stars[numHeart - 1].SetActive(false);
        numHeart--;
    }

    private void HandleReset()
    {
        numHeart = 3;
        for (int i = 0; i < numHeart; i++)
        {
            stars[i].SetActive(true);
        }
    }
}
