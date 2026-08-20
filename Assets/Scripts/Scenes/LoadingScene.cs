using Assets.Scripts.Boosters;
using Assets.Scripts.Config;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.Input;
using Assets.Scripts.IO;
using Assets.Scripts.Scenes;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System.Collections;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingScene : MonoBehaviour
{
    public float spacing = 1f;
    public Slider slider;
    public TextMeshPro loadingText;
    private float loadingPercent = 0f;
    private IController _controller;

    private void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
    }

    private void Start()
    {
        IStorage storage = new LocalStorage();
        IConfig config = new ConfigManager(storage);
        IInput input = new PlayerInput(spacing);
        _controller = new ArrowController(config, input, spacing);
        IUIManager uiManager = new UIManager(_controller, input, spacing);
        IBoostersManager boosterManager = new BoostersManager(_controller);
        //IPopUpManager popupManager = new PopUpManager();

        Locator.Register(storage);
        Locator.Register(config);
        Locator.Register(input);
        Locator.Register(_controller);
        Locator.Register(uiManager);
        Locator.Register(boosterManager);
        //Locator.Register(popupManager);

        SceneManager.LoadScene("GamePlay");
    }

    private IEnumerator LoadHomeScene() 
    {
        yield return null;
        _controller.LoadConfig();
    }
}
