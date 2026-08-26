using Assets.Scripts.CoreLogic;
using Assets.Scripts.Scenes.Helper;
using Assets.Scripts.UI;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PopUpManager : MonoBehaviour, IPopUpManager
{
    public GameObject popupPannel;
    public GameObject blackBg;
    public Button button1;
    public TextMeshProUGUI result;

    public int arrowDecPos = 200;
    public GameObject dataLabel;
    public GameObject[] arrows;
    public TextMeshProUGUI level;

    
    public GameObject winUI2;


    private IGamePlayUI _gamePlayUI;
    private IController _controller;
    private IEventHandler _eventHandler;
    private List<Vector2> arrowPos;

    public event Action OnPlayAgain;

    private void OnDisable()
    {
        button1.onClick.RemoveListener(HandlePlayAgainClicked);

        if (_eventHandler != null)
            _eventHandler.OnTurnPopupOn -= HandleTurnPopupOn;

        if (_controller != null)
            _controller.OnTurnPopupOff -= HandleTurnPopupOff;
    }

    public void Init(IController controller, IEventHandler eventHandler)
    {
        _eventHandler = eventHandler;
        _controller = controller;
        RegisterAction();
    }

    private void RegisterAction()
    {
        button1.onClick.AddListener(HandlePlayAgainClicked);
        _eventHandler.OnTurnPopupOn += HandleTurnPopupOn;
        _controller.OnTurnPopupOff += HandleTurnPopupOff;
    }

    private void HandlePlayAgainClicked()
    {
        OnPlayAgain?.Invoke();
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
