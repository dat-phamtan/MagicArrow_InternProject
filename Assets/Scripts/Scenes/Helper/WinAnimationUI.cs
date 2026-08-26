using Assets.Scripts.CoreLogic;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WinAnimationUI : MonoBehaviour
{
    public int arrowDecPos = 200;
    public GameObject dataLabel;
    public GameObject[] arrows;
    public TextMeshProUGUI level;

    private List<Vector2> arrowPos;
    private IController _controller;
    private IGamePlayUI _gamePlayUI;

    public void Start()
    {
        _controller = Locator.Get<IController>();
        _gamePlayUI = Locator.Get<IGamePlayUI>();
        ArrowDecorationInit();
        LoadLevelData();
    }


    private void ArrowDecorationInit()
    {
        arrowPos.Add(new Vector2(0, -arrowDecPos));
        arrowPos.Add(new Vector2(arrowDecPos, arrowDecPos));
        arrowPos.Add(new Vector2(0, -arrowDecPos));
        arrowPos.Add(new Vector2(-arrowDecPos, arrowDecPos));
    }

    private void LoadLevelData()
    {
        var levelData = _controller.GetCurrentLevelIndex();
        level.text = "Level " + levelData.ToString();
    }

    private void PlayArrowDecoAnimation()
    {
        for (int i = 0; i < arrows.Length; i++)
        {
            _gamePlayUI.MoveInAnimation(arrows[i], arrowPos[i]);
        }
    }

    private void PlayDataLabelAnimation()
    {

    }

}
