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

    public Image magnifierImage;
    public Image rulerImage;

    public GameObject particle;

    public Tilemap magnifierTilemap;
    public Tilemap rulerTilemap;

    private IBooster _booster;
    private IBoostersManager _boostersManager;
    private IUIManager _uiManager;
    private IController _controller;
    private Vector2 _eraserPopupBasePos;
    private Vector2 _eraserPopupHidePos;

    public event Action<IBooster> OnBoosterClicked;

    private void OnEnable()
    {
        //_boostersManager.OnBoosterBusyChanged += HandleBusyChanged;
        magnifier.onClick.AddListener(() => { OnBoosterClicked(new Magnifier(magnifierTilemap, magnifierImage, particle)); });
        eraser.onClick.AddListener(HandleEraserOnClicked);
        wand.onClick.AddListener(() => { OnBoosterClicked(new Wand()); });
        ruler.onClick.AddListener(() => { OnBoosterClicked(new Ruler(rulerTilemap, rulerImage)); });
        eraserPopupExit.onClick.AddListener(HandleExit);
    }

    private void Start()
    {
        _boostersManager = Locator.Get<IBoostersManager>();
        _controller = Locator.Get<IController>();
        _uiManager = Locator.Get<IUIManager>();
        _boostersManager.Init(this);
        _controller.OnHideBoosters += HandleHideBoosters;
        _controller.OnShowBoosters += HandleShowBoosters;
        _controller.OnHideEraserPopup += HideEraserPopup;
        _eraserPopupBasePos = new Vector2(0, eraserPopupYPos);
        _eraserPopupHidePos = new Vector2(0, -eraserPopupYPos);
        HandleShowBoosters();
    }
    
    private void HandleExit()
    {
        HideEraserPopup();
        _controller.ShowGameSceneUI();
    }

    private void HandleEraserOnClicked()
    {
        OnBoosterClicked?.Invoke(new Eraser());
        ShowEraserPopup();
    }

    private void ShowEraserPopup()
    {
        _uiManager.ShowTopBar(eraserPopup, _eraserPopupBasePos);
    }

    private void HideEraserPopup()
    {
        _uiManager.HideTopBar(eraserPopup, _eraserPopupHidePos);
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
            _uiManager.ShowUI(boosters[i]);
            yield return null;
        }
    }
    
    private IEnumerator HideBoosters()
    {
        for (int i = boosters.Length - 1; i >= 0; i--)
        {
            _uiManager.HideUI(boosters[i]);
            yield return null;
        }
    }

    private void OnDisable() 
    {
        //_boostersManager.OnBoosterBusyChanged -= HandleBusyChanged;
    }

    private void HandleBusyChanged(bool isBusy)
    {
        magnifier.interactable = !isBusy;
        eraser.interactable = !isBusy;
        wand.interactable = !isBusy;
        ruler.interactable = !isBusy;
    }
}
