using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System;
using System.Threading;

public class TransitionScene : MonoBehaviour
{
    public GameObject logo;
    public string nextSceneName = "GamePlay";
    public float minLogoHoldDuration = 0.5f;
    public float animationTimeout = 3f;

    private IGamePlayUI _uiManager;

    private async UniTaskVoid Start()
    {
        var token = this.GetCancellationTokenOnDestroy();
        var op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false;

        _uiManager = Locator.Get<IGamePlayUI>();

        await JumpIn(token);

        float elapsed = 0f;
        while (op.progress < 0.9f || elapsed < minLogoHoldDuration)
        {
            elapsed += Time.deltaTime;
            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        await JumpOut(token);
 
        op.allowSceneActivation = true;
    }

    private UniTask JumpIn(CancellationToken token)
    {
        return PlayAnimationSafe(
            name: "JumpIn",
            play: onComplete => _uiManager.JumpInAnimation(logo, onComplete),
            token: token);
    }

    private UniTask JumpOut(CancellationToken token)
    {
        return PlayAnimationSafe(
            name: "JumpOut",
            play: onComplete => _uiManager.JumpOutAnimation(logo, onComplete),
            token: token);
    }

    private async UniTask PlayAnimationSafe(string name, Action<Action> play, CancellationToken token)
    {
        if (!IsLogoValid())
            return;

        var tcs = new UniTaskCompletionSource();
        play(() => tcs.TrySetResult());

        var animationTask = tcs.Task.AttachExternalCancellation(token);
        var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(animationTimeout), cancellationToken: token);

        int winnerIndex = await UniTask.WhenAny(animationTask, timeoutTask);
    }

    private bool IsLogoValid()
    {
        if (logo == null)
            return false;
        if (logo.GetComponent<CanvasGroup>() == null)
            return false;
        if (logo.GetComponent<RectTransform>() == null)
            return false;
        return true;
    }
}