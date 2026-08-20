using Assets.Scripts.Boosters;
using Assets.Scripts.Config;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using Assets.Scripts.IO;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.Controls;

public class LoadingScene : MonoBehaviour
{
    public float boardSpacing = 1f;
    public float maximumFakeLoading = 0.9f;
    public float textDuration = 1f;
    public float loadingPerFrame = 0.05f;
    public float loadingBoost = 0.01f;
    public Slider slider;
    public TextMeshProUGUI loadingText;
    private int _numdots = 0;
    private bool _isDone = false;
    private bool _isLoadCompleted = false;

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
        ServicesInit();

        await UniTask.SwitchToThreadPool();
        await LoadPlayerData();
        await UniTask.SwitchToMainThread();

        var op = SceneManager.LoadSceneAsync("Home");
        op.allowSceneActivation = false;
        while (op.progress < 0.9f)
            await UniTask.Yield(PlayerLoopTiming.Update, token);


        await UniTask.Delay(2000, cancellationToken: token);
        _isLoadCompleted = true;
        while (slider.value < maximumFakeLoading)
        {
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        _isDone = true;
        slider.value = 1f;
        loadingText.text = "Completed";
        await UniTask.Delay(100, cancellationToken: token);
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

            if (_numdots > 3)
            {
                loadingText.text = "Loading";
                _numdots = 0;
            }
            if (duration > textDuration)
            {
                loadingText.text += ".";
                _numdots++;
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

    private void ServicesInit()
    {
        IStorage storage = new LocalStorage();
        IConfig config = new ConfigManager(storage);
        IInput input = new PlayerInput(boardSpacing);
        IController controller = new ArrowController(config, storage, input, boardSpacing);
        IUIManager uiManager = new UIManager(controller, input, boardSpacing);
        IBoostersManager boosterManager = new BoostersManager(controller);

        Locator.Register(storage);
        Locator.Register(config);
        Locator.Register(input);
        Locator.Register(controller);
        Locator.Register(uiManager);
        Locator.Register(boosterManager);
    }
}
