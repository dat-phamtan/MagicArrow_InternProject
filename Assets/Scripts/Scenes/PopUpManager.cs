using Assets.Scripts.CoreLogic;
using Assets.Scripts.Scenes;
using Assets.Scripts.UI;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour, IPopUpManager
{
    public GameObject popupPannel;
    public GameObject blackBg;
    //public GameObject
    public Button button1;
    public TextMeshProUGUI result;
    private IController _controller;
    public IEventHandler _eventHandler;
    public event Action OnPlayAgain;

    public void Init(IController controller, IEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
        _controller = controller;
        RegisterAction();
    }

    private void RegisterAction()
    {
        button1.onClick.AddListener(() => { OnPlayAgain?.Invoke(); });
        _eventHandler.OnTurnPopupOn += HandleTurnPopupOn;
        _controller.OnTurnPopupOff += HandleTurnPopupOff;
    }

    private void HandleTurnPopupOff()
    {
        popupPannel.SetActive(false);
        blackBg.SetActive(false);
    }

    private void HandleTurnPopupOn(string text)
    {
        popupPannel.SetActive(true);
        blackBg.SetActive(true);
        result.text = text;
    }
}
