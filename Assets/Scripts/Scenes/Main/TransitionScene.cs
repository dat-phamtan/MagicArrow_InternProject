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
    public float animationTimeout = 3f; // safety net so a lost/killed tween can never hang the transition forever

    private IGamePlayUI _uiManager;

    private async UniTaskVoid Start()
    {
        var token = this.GetCancellationTokenOnDestroy();
        var op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false;

        try
        {
            _uiManager = Locator.Get<IGamePlayUI>();

            await JumpIn(token);

            float elapsed = 0f;
            while (op.progress < 0.9f || elapsed < minLogoHoldDuration)
            {
                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            await JumpOut(token);
        }
        catch (OperationCanceledException)
        {
            // scene got destroyed / transition cancelled mid-flight, nothing else to do
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        finally
        {
            // guarantees we NEVER get stuck on this scene, even if the animation
            // callback is lost (tween killed without completing, missing components, etc.)
            op.allowSceneActivation = true;
        }
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
        {
            Debug.LogError($"TransitionScene: '{name}' skipped, logo is missing or missing CanvasGroup/RectTransform.");
            return;
        }

        var tcs = new UniTaskCompletionSource();
        play(() => tcs.TrySetResult());

        var animationTask = tcs.Task.AttachExternalCancellation(token);
        var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(animationTimeout), cancellationToken: token);

        int winnerIndex = await UniTask.WhenAny(animationTask, timeoutTask);
        if (winnerIndex == 1)
            Debug.LogWarning($"TransitionScene: '{name}' animation timed out after {animationTimeout}s, continuing anyway.");
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