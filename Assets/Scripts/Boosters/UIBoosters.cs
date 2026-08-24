using Assets.Scripts.Boosters;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class UIBooster : MonoBehaviour, IBoosterAction
{
    //temp data
    public int magnifierNums = 2;
    public int eraserNums = 2;
    public int wandNums = 2;
    public int rulerNum = 2;
    public int eraserPopupYPos = -400;

    public Button magnifier;
    public Button eraser;
    public Button wand;
    public Button ruler;

    public GameObject[] boosters;
    public GameObject eraserPopup;
    public Button eraserPopupExit;

    public GameObject wandAnimation;

    public Image magnifierImage;
    public Image rulerImage;

    //public GameObject particle;

    public Tilemap magnifierTilemap;
    public Tilemap rulerTilemap;

    private IBooster _booster;
    private IBoostersManager _boostersManager;
    private IGamePlayUI _uiManager;
    private IController _controller;
    private Vector2 _eraserPopupBasePos;
    private Vector2 _eraserPopupHidePos;

    private IBooster _magnifierBooster;
    private IBooster _eraserBooster;
    private IBooster _wandBooster;
    private IBooster _rulerBooster;

    public event Action<IBooster> OnBoosterClicked;

    private void OnEnable()
    {

        _magnifierBooster ??= new Magnifier(magnifierTilemap, magnifierImage);
        _rulerBooster ??= new Ruler(rulerTilemap, rulerImage);
        _wandBooster ??= new Wand(wandAnimation);
        _eraserBooster ??= new Eraser();

        magnifier.onClick.AddListener(() => OnBoosterClicked?.Invoke(_magnifierBooster));
        eraser.onClick.AddListener(HandleEraserOnClicked);
        wand.onClick.AddListener(() => OnBoosterClicked?.Invoke(_wandBooster));
        ruler.onClick.AddListener(() => OnBoosterClicked?.Invoke(_rulerBooster));
        eraserPopupExit.onClick.AddListener(HandleExit);
    }

    private void OnDisable()
    {
        _controller.OnHideBoosters -= HandleHideBoosters;
        _controller.OnShowBoosters -= HandleShowBoosters;
        _controller.OnHideEraserPopup -= HideEraserPopup;
        _boostersManager.OnBoosterBusyChanged -= HandleBusyChanged;
        _magnifierBooster?.Dispose();
        _rulerBooster?.Dispose();
        _wandBooster?.Dispose();
        _eraserBooster?.Dispose();
    }

    private void Start()
    {
        _boostersManager = Locator.Get<IBoostersManager>();
        _controller = Locator.Get<IController>();
        _uiManager = Locator.Get<IGamePlayUI>();
        _boostersManager.Init(this);
        _controller.OnHideBoosters += HandleHideBoosters;
        _controller.OnShowBoosters += HandleShowBoosters;
        _controller.OnHideEraserPopup += HideEraserPopup;
        _boostersManager.OnBoosterBusyChanged += HandleBusyChanged;
        _eraserPopupBasePos = new Vector2(0, eraserPopupYPos);
        _eraserPopupHidePos = new Vector2(0, -eraserPopupYPos);
        HandleShowBoosters();
    }
    
    private void HandleExit()
    {
        HideEraserPopup();
        _controller.ExitEraserMode();
    }

    private void HandleEraserOnClicked()
    {
        if (_controller.IsEraserModeTrue())
            return;
        OnBoosterClicked?.Invoke(_eraserBooster);
        ShowEraserPopup();
    }

    private void ShowEraserPopup()
    {
        _uiManager.MoveInAnimation(eraserPopup, _eraserPopupBasePos);
    }

    private void HideEraserPopup()
    {
        _uiManager.MoveOutAnimation(eraserPopup, _eraserPopupHidePos);
    }

    private void HandleShowBoosters()
    {
        StartCoroutine(ShowBoosters());
    }

    private void HandleHideBoosters()
    {
        StartCoroutine(HideBoosters());
    }

    private IEnumerator ShowBoosters()
    {
        for (int i = 0; i < boosters.Length; i++)
        {
            _uiManager.JumpInAnimation(boosters[i]);
            yield return null;
        }
    }
    
    private IEnumerator HideBoosters()
    {
        for (int i = boosters.Length - 1; i >= 0; i--)
        {
            _uiManager.JumpOutAnimation(boosters[i]);
            yield return null;
        }
    }

    private void HandleBusyChanged(bool isBusy)
    {
        magnifier.interactable = !isBusy;
        eraser.interactable = !isBusy;
        wand.interactable = !isBusy;
        ruler.interactable = !isBusy;
    }
}
