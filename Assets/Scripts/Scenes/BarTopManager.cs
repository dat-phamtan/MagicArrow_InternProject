using Assets.Scripts.CoreLogic;
using Assets.Scripts.Utility;
using System;
using UnityEngine;
using UnityEngine.UI;

public class BarTopManager : MonoBehaviour
{
    public GameObject[] stars;
    public int numHeart = 3;
    public GameObject levelNum;
    public Button pauseButton;
    private IController _controller;
    

    public void Start()
    {
        _controller = Locator.Get<IController>();
        _controller.OnLoseHeart += HandleLoseHeart;
        _controller.OnReset += HandleReset;
    }

    private void HandleLoseHeart()
    {
        stars[numHeart - 1].SetActive(false);
        numHeart--;
    }

    private void HandleReset()
    {
        numHeart = 3;
        for (int i = 0; i < numHeart; i++)
        {
            stars[i].SetActive(true);
        }
    }
}
