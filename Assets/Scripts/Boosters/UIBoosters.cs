using Assets.Scripts.Boosters;
using Assets.Scripts.Utility;
using System;
using UnityEngine;
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
    private IBooster _booster;
    public event Action<IBooster> OnBoosterClicked;


    private void OnEnable()
    {
        magnifier.onClick.AddListener(() => {OnBoosterClicked(new Eraser()); });
        eraser.onClick.AddListener(() => {OnBoosterClicked(new Eraser()); });
        wand.onClick.AddListener(() => {OnBoosterClicked(new Eraser()); });
        ruler.onClick.AddListener(() => {OnBoosterClicked(new Eraser()); });
    }

    private void Start()
    {
        var boosterManager = Locator.Get<IBoostersManager>();
        boosterManager.Init(this);
    }
}
