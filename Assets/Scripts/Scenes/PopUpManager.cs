using Assets.Scripts.CoreLogic;
using Assets.Scripts.Scenes;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour, IPopUpManager
{
    public GameObject popupPannel;
    public Button button1;
    public TextMeshProUGUI result;
    private IController _controller;
    public event Action OnPlayAgain;

    public void Init(IController controller)
    {
        _controller = controller;
        RegisterAction();
    }

    private void RegisterAction()
    {
        button1.onClick.AddListener(() => { OnPlayAgain?.Invoke(); });
        _controller.OnTurnPopupOn += HandleTurnPopupOn;
        _controller.OnTurnPopupOff += HandleTurnPopupOff;
    }

    private void HandleTurnPopupOff()
    {
        popupPannel.SetActive(false);
    }

    private void HandleTurnPopupOn(string text)
    {
        popupPannel.SetActive(true);
        result.text = text;
    }
}
