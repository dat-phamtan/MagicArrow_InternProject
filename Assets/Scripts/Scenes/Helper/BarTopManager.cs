using Assets.Scripts.Boosters;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public GameObject restartPopup;
    public GameObject restartConfirmPopup;
    public GameObject quitConfirmPopup;
    public GameObject quitPopup;
    public GameObject blackBg;
    public GameObject topBar;

    public Button[] popupTurnOffBtn;
    public Button[] resumeBtn;
    public Button restartBtn1;
    public Button restartBtn2;
    public Button restartBtn3;
    public Button quitBtn1;
    public Button quitBtn2;
    public Button quitBtn3;

    public Button soundEffectBtn;
    public Button musicBtn;
    public Button vibrateBtn;
    public Button lightModeBtn;
    

    private Vector2 _basePos;
    private Vector2 _hidePos;
    private IController _controller;
    private IGamePlayUI _uiManager;
    private IBoostersManager _boostersManager;
    private bool _isRestartConfirmed = false;
    private bool _isQuitConfirmed; 

    private void OnEnable()
    {
        pauseBtn.onClick.AddListener(HandlePauseClicked);
        restartBtn1.onClick.AddListener(HandlePlayAgainRequest);
        restartBtn2.onClick.AddListener(HandlePlayAgainConfirm);
        restartBtn3.onClick.AddListener(HandlePlayAgain);
        quitBtn1.onClick.AddListener(HandleQuitRequest);
        quitBtn2.onClick.AddListener(HandleQuitConfirm);
        quitBtn3.onClick.AddListener(HandleQuit);
        foreach (var resume in resumeBtn)
            resume.onClick.AddListener(HandleResume);
        foreach (var exit in popupTurnOffBtn)
            exit.onClick.AddListener(HandleResume);
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

    private void OnDisable()
    {
        pauseBtn.onClick.RemoveListener(HandlePauseClicked);
        restartBtn1.onClick.RemoveListener(HandlePlayAgainRequest);
        restartBtn2.onClick.RemoveListener(HandlePlayAgainConfirm);
        restartBtn3.onClick.RemoveListener(HandlePlayAgain);
        quitBtn1.onClick.RemoveListener(HandleQuitRequest);
        quitBtn2.onClick.RemoveListener(HandleQuitConfirm);
        quitBtn3.onClick.RemoveListener(HandleQuit);
        foreach (var resume in resumeBtn)
            resume.onClick.RemoveListener(HandleResume);
        foreach (var exit in popupTurnOffBtn)
            exit.onClick.RemoveListener(HandleResume);

        if (_controller != null)
        {
            _controller.OnLoseHeart -= HandleLoseHeart;
            _controller.OnReset -= HandleReset;
            _controller.OnShowBarTop -= HandleShowBarTop;
            _controller.OnHideBarTop -= HandleHideBarTop;
        }

        if (_boostersManager != null)
            _boostersManager.OnBoosterBusyChanged -= HandleBusyChanged;
    }

    private void HandlePauseClicked()
    {
        _uiManager.JumpInAnimation(pausePopup);
        _controller.BlockInteraction();
        blackBg.SetActive(true);
    }

    private void HandleResume()
    {
        if (pausePopup.activeInHierarchy)
            _uiManager.JumpOutAnimation(pausePopup);
        if (restartPopup.activeInHierarchy)
            _uiManager.JumpOutAnimation(restartPopup);
        if (restartConfirmPopup.activeInHierarchy)
            _uiManager.JumpOutAnimation(restartConfirmPopup);
        if (quitPopup.activeInHierarchy)
            _uiManager.JumpOutAnimation(quitPopup);
        if (quitConfirmPopup.activeInHierarchy)
            _uiManager.JumpOutAnimation(quitConfirmPopup);
        _controller.UnblockInteraction();
        blackBg.SetActive(false);
    }

    private void HandlePlayAgainRequest()
    {
        _uiManager.JumpOutAnimation(pausePopup);
        _uiManager.JumpInAnimation(restartConfirmPopup);
    }

    private void HandlePlayAgainConfirm()
    {
        //restartPopup.SetActive(true);
        //restartConfirmPopup.SetActive(false);
        _uiManager.JumpOutAnimation(restartConfirmPopup);
        _uiManager.JumpInAnimation(restartPopup);
        
    }

    private void HandlePlayAgain()
    {
        _uiManager.JumpOutAnimation(restartPopup);
        SceneManager.LoadSceneAsync("Transition");
    }

    private void HandleQuitRequest()
    {
        _uiManager.JumpOutAnimation(pausePopup);
        _uiManager.JumpInAnimation(quitConfirmPopup);
    }

    private void HandleQuitConfirm()
    {
        //quitPopup.SetActive(true);
        //quitConfirmPopup.SetActive(false);
        _uiManager.JumpOutAnimation(quitConfirmPopup);
        _uiManager.JumpInAnimation(quitPopup);
    }

    private void HandleQuit()
    {
        _uiManager.JumpOutAnimation(quitPopup);
        SceneManager.LoadSceneAsync("Home");
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
