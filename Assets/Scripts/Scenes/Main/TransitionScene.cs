using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System.Threading;

public class TransitionScene : MonoBehaviour
{
    public GameObject logo;
    public string nextSceneName = "GamePlay";
    public float minLogoHoldDuration = 0.5f; 

    private IGamePlayUI _uiManager;

    private async UniTaskVoid Start()
    {
        var token = this.GetCancellationTokenOnDestroy();
        _uiManager = Locator.Get<IGamePlayUI>();

        var op = SceneManager.LoadSceneAsync(nextSceneName);
        op.allowSceneActivation = false;

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
        var tcs = new UniTaskCompletionSource();
        _uiManager.JumpInAnimation(logo, () => tcs.TrySetResult());
        return tcs.Task.AttachExternalCancellation(token);
    }

    private UniTask JumpOut(CancellationToken token)
    {
        var tcs = new UniTaskCompletionSource();
        _uiManager.JumpOutAnimation(logo, () => tcs.TrySetResult());
        return tcs.Task.AttachExternalCancellation(token);
    }
}