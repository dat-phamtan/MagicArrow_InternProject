using Assets.Scripts.Boosters;
using Assets.Scripts.Config;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.Input;
using Assets.Scripts.IO;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.SceneManagement;
using Assets.Scripts.Ultility;
using Assets.Scripts.Sound;

public class LoadingScene : MonoBehaviour
{
    public float boardSpacing = 1f;
    public float maximumFakeLoading = 0.9f;
    public float textDuration = 1f;
    public float loadingPerFrame = 0.05f;
    public float loadingBoost = 0.01f;
    public int completedDelayDurartion = 100;
    public Slider slider;
    public TextMeshProUGUI loadingText;

    private int _numDots = 0;
    private bool _isDone = false;
    private bool _isLoadCompleted = false;
    private int _maximumNumDots = 3;

    public void Awake()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        slider.value = 0f;
    }

    private async UniTaskVoid Start()
    {
        var token = this.GetCancellationTokenOnDestroy();
        _ = FakeLoading(token);

        await DataHelper.CheckFileExist();
        ServicesInit();
        await UniTask.SwitchToThreadPool();
        await LoadPlayerData();
        await UniTask.SwitchToMainThread();
        SoundDataInit();

        var op = SceneManager.LoadSceneAsync("Home");
        op.allowSceneActivation = false;
        while (op.progress < maximumFakeLoading)
            await UniTask.Yield(PlayerLoopTiming.Update, token);


        //await UniTask.Delay(2000, cancellationToken: token);
        _isLoadCompleted = true;
        while (slider.value < maximumFakeLoading)
            await UniTask.Yield(PlayerLoopTiming.Update, token);

        _isDone = true;
        slider.value = 1f;
        loadingText.text = "Completed";
        await UniTask.Delay(completedDelayDurartion, cancellationToken: token);
        op.allowSceneActivation = true;
    }

    private async UniTask FakeLoading(CancellationToken token)
    {
        float duration = 0f;
        while (!_isDone)
        {
            token.ThrowIfCancellationRequested();
            if (_isLoadCompleted)
                loadingPerFrame += loadingBoost;
            duration += Time.deltaTime;
            if (slider.value < maximumFakeLoading)
                slider.value += loadingPerFrame * Time.deltaTime;

            if (_numDots > _maximumNumDots)
            {
                loadingText.text = "Loading";
                _numDots = 0;
            }
            if (duration > textDuration)
            {
                loadingText.text += ".";
                _numDots++;
                duration = 0f;
            }
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    private async UniTask LoadPlayerData()
    {
        var controller = Locator.Get<IController>();
        controller.LoadPlayerData();
    }

    private void SoundDataInit()
    {
        var controller = Locator.Get<IController>();
        var soundManager = Locator.Get<ISoundManager>();
        var storage = Locator.Get<IStorage>();
        var settingData = controller.GetPlayerData().Setting;
        soundManager.Init(storage, settingData);
        soundManager.BindingEvents(controller);
    }

    private void ServicesInit()
    {
        IStorage storage = new LocalStorage();
        IConfig config = new ConfigManager(storage);
        IInput input = new PlayerInput(boardSpacing);
        IController controller = new ArrowController(config, storage, input, boardSpacing);
        IGamePlayUI uiManager = new GamePlayUI(controller, input, boardSpacing);
        IBoostersManager boosterManager = new BoostersManager(controller);
        IHomeUI homeUI = new HomeUI();
        //ICamera camera = new Cam

        Locator.Register(storage);
        Locator.Register(config);
        Locator.Register(input);
        Locator.Register(controller);
        Locator.Register(uiManager);
        Locator.Register(boosterManager);
        Locator.Register(homeUI);
    }
}
