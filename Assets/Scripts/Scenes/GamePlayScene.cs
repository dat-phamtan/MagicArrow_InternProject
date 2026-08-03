using Assets.Scripts.Config;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using Assets.Scripts.IO;
using Assets.Scripts.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = Assets.Scripts.Input.PlayerInput;

public class GamePlayScene : MonoBehaviour
{
    public new Camera camera;
    //public GameObject dotPrefab;
    public GameObject headPrefab;
    public GameObject bodyPrefab;
    public GameObject tailPrefab;
    public ArrowAssembler arrowAssembler;

    public Material arrowMaterial;
    public CameraModifier cameraModifier;
    public float spacing = 1f;


    private IConfig _config;
    private IStorage _storage;
    private IInput _input;
    private IController _controller;
    private IUIManager _uiManager;
    private InputSystem_Actions _inputActions;
    private ConfigData _configData;
    private List<GameObject> _partsList;



    private void Awake()
    {
        _storage = new LocalStorage();
        _config = new ConfigManager(_storage);
        _input = new PlayerInput(spacing);
        _controller = new ArrowController(_config, _input);
        _inputActions = new InputSystem_Actions();
        _uiManager = new UIManager(_controller);

    }

    void Start()
    {
        _controller.Init();
        _configData = _controller.GetConfigData();

        //DrawGridInit(_uiManager.InitBoard(spacing));
        cameraModifier.FitCamera(_configData.BoardWidth, _configData.BoardHeight, spacing);

        foreach (var arrow in _configData.Arrows)
        {
            arrowAssembler.Build(arrow, _configData.BoardWidth, _configData.BoardHeight, spacing);
        }
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.UI.ClickAtPos.performed += HandleInput;
        _controller.OnMoveArrowAway += MoveArrowAway;

    }

    private void OnDisable()
    {
        _inputActions.UI.ClickAtPos.performed -= HandleInput;
        _controller.OnMoveArrowAway -= MoveArrowAway;
        _inputActions.Disable();
    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        var screenPos = context.ReadValue<Vector2>();
        _input.HandleInput(camera.ScreenToWorldPoint(screenPos));
    }

    public void DrawGridInit(List<Verticle> grid)
    {
        _partsList = new List<GameObject>();
        for (int i = 0; i < grid.Count; i++)
        {
            Vector3 spawnPosition = new(grid[i].XVerticle, grid[i].YVerticle, 0);
            //Debug.Log($"{grid[i].XVerticle }/{ grid[i].YVerticle}");
            if (grid[i].Type == VerticleType.HEAD)
                _partsList.Add(Instantiate(headPrefab, spawnPosition, Quaternion.identity));
            else if (grid[i].Type == VerticleType.TAIL)
                _partsList.Add(Instantiate(tailPrefab, spawnPosition, Quaternion.identity));
            else if (grid[i].Type == VerticleType.BODY)
                _partsList.Add(Instantiate(bodyPrefab, spawnPosition, Quaternion.identity));
        }
    }

    private void MoveArrowAway(int boardIndex)
    {
        var arrowIndices = _configData.Arrows[boardIndex].ArrowIndices;
        for (int i = 0; i < arrowIndices.Length; i++)
        {
            Destroy(_partsList[arrowIndices[i]]);
        }
    }
}
