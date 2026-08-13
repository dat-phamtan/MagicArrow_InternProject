using Assets.Scripts.Boosters;
using Assets.Scripts.Config;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.Input;
using Assets.Scripts.IO;
using Assets.Scripts.Scenes;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    public float spacing = 1f;

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
        IController controller = new ArrowController(config, input, spacing);
        IUIManager uiManager = new UIManager(controller, input, spacing);
        IBoostersManager boosterManager = new BoostersManager(controller);
        //IPopUpManager popupManager = new PopUpManager();

        Locator.Register(storage);
        Locator.Register(config);
        Locator.Register(input);
        Locator.Register(controller);
        Locator.Register(uiManager);
        Locator.Register(boosterManager);
        //Locator.Register(popupManager);

        SceneManager.LoadScene("GamePlay");
    }
}
