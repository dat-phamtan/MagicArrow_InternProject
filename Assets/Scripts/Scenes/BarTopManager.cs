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
    public Button pauseButton;
    public GameObject pausePopup;
    public GameObject blackBg;
    public GameObject topBar;

    private Vector2 _basePos;
    private Vector2 _hidePos;
    private IController _controller;
    private IUIManager _uiManager;
    private IBoostersManager _boostersManager;

    private void OnEnable()
    {
        pauseButton.onClick.AddListener(() => { HandlePauseClicked(); });
    }

    public void Start()
    {
        _controller = Locator.Get<IController>();
        _uiManager = Locator.Get<IUIManager>();
        _boostersManager = Locator.Get<IBoostersManager>();
        _controller.OnLoseHeart += HandleLoseHeart;
        _controller.OnReset += HandleReset;
        _controller.OnShowBarTop += HandleShowBarTop;
        _controller.OnHideBarTop += HandleHideBarTop;
        _boostersManager.OnBoosterBusyChanged += HandleBusyChanged;
        PositionInit();
        HandleShowBarTop();
        
    }

    private void HandleBusyChanged(bool isBusy)
    {
        pauseButton.interactable = !isBusy;
    }

    private void PositionInit() 
    {
        _basePos = new Vector2(0, yPosShow);
        _hidePos = new Vector2(0, yPosHide);
    }

    private void HandleHideBarTop()
    {
        _uiManager.PlaySlideOutAnimation(topBar, _hidePos);
    }

    private void HandleShowBarTop()
    {
        _uiManager.PlaySlideInAnimation(topBar, _basePos);
    }

    private void HandlePauseClicked()
    {
        _controller.BlockInteraction();
        pausePopup.SetActive(true);
        blackBg.SetActive(true);
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
