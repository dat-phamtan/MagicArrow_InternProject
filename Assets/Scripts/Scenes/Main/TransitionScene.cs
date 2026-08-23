using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;

public class TransitionScene : MonoBehaviour
{
    public GameObject logo;
    private IGamePlayUI _uiManager;


    private void Start()
    {
        //var token = this.GetCancellationTokenOnDestroy();
        _uiManager = Locator.Get<IGamePlayUI>();
        _uiManager.JumpInAnimation(logo);
        var op = SceneManager.LoadSceneAsync("GamePlay");

        if (op.isDone)
        {
            _uiManager.JumpOutAnimation(logo);
        }
    }
}
