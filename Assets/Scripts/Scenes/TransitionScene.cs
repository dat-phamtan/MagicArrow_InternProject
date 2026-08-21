using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;

public class TransitionScene : MonoBehaviour
{
    public GameObject logo;
    private IUIManager _uiManager;

    //private async UniTaskVoid Start()
    //{
    //    var token = this.GetCancellationTokenOnDestroy();
    //    _uiManager = Locator.Get<IUIManager>();
    //    _uiManager.JumpInAnimation(logo);
    //    var op = SceneManager.LoadSceneAsync("GamePlay");
    //    op.allowSceneActivation = false;
    //    while (op.progress < 0.9f)
    //        await UniTask.Yield(PlayerLoopTiming.Update, token);

    //    _uiManager.JumpOutAnimation(logo);
    //    op.allowSceneActivation = true;
    //}

    private void Start()
    {
        var token = this.GetCancellationTokenOnDestroy();
        _uiManager = Locator.Get<IUIManager>();
        _uiManager.JumpInAnimation(logo);
        var op = SceneManager.LoadSceneAsync("GamePlay");

        if (op.isDone)
        {
            _uiManager.JumpOutAnimation(logo);
        }
        //op.allowSceneActivation = false;
        //while (op.progress < 0.9f)
        //    await UniTask.Yield(PlayerLoopTiming.Update, token);

        //_uiManager.JumpOutAnimation(logo);
        //op.allowSceneActivation = true;
    }
}
