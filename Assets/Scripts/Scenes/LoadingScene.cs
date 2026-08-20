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

public class LoadingScene : MonoBehaviour
{
    public float spacing = 1f;
    public float maximumFakeLoading = 0.9f;
    public float textDuration = 1f;
    public float loadingPerFrame = 0.05f;
    public Slider slider;
    public TextMeshProUGUI loadingText;
    private int _numdots = 0;
    private bool _isDone = false;

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

        while (slider.value < maximumFakeLoading)
        {
            loadingPerFrame += 0.01f;
        }
        //await UniTask.Delay(2000, cancellationToken: token);
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
        IInput input = new PlayerInput(spacing);
        IController controller = new ArrowController(config, input, spacing);
        IUIManager uiManager = new UIManager(controller, input, spacing);
        IBoostersManager boosterManager = new BoostersManager(controller);

        Locator.Register(storage);
        Locator.Register(config);
        Locator.Register(input);
        Locator.Register(controller);
        Locator.Register(uiManager);
        Locator.Register(boosterManager);
    }
}
