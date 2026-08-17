using Assets.Scripts.Boosters;
using Assets.Scripts.Utility;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class UIBooster : MonoBehaviour, IBoosterAction
{
    //temp data
    public int magnifierNums = 2;
    public int eraserNums = 2;
    public int wandNums = 2;
    public int rulerNum = 2;

    public Button magnifier;
    public Button eraser;
    public Button wand;
    public Button ruler;

    public Image magnifierImage;

    public Tilemap magnifierTilemap;
    public Tilemap rulerTilemap;
    private IBooster _booster;
    private IBoostersManager _boostersManager;
    public event Action<IBooster> OnBoosterClicked;

    private void OnEnable()
    {
        //_boostersManager.OnBoosterBusyChanged += HandleBusyChanged;
        magnifier.onClick.AddListener(() => { OnBoosterClicked(new Magnifier(magnifierTilemap, magnifierImage)); });
        eraser.onClick.AddListener(() => { OnBoosterClicked(new Eraser()); });
        wand.onClick.AddListener(() => { OnBoosterClicked(new Wand()); });
        ruler.onClick.AddListener(() => { OnBoosterClicked(new Ruler(rulerTilemap)); });
    }

    private void Awake()
    {
        _boostersManager = Locator.Get<IBoostersManager>();
    }

    private void Start()
    {
        _boostersManager.Init(this);
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
