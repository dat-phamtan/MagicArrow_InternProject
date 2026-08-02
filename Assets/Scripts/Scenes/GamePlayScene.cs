using Assets.Scripts.Config;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.IO;
using Assets.Scripts.UI;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePlayScene : MonoBehaviour, IUIGameScene
{
    public new Camera camera;
    //public GameObject dotPrefab;
    public GameObject headPrefab;
    public GameObject bodyPrefab;
    public GameObject tailPrefab;

    public Material arrowMaterial;

    public CameraModifier cameraModifier;
    private InputSystem_Actions _inputs;
    private float _spacing = 0.5f;
    private Dictionary<Direction, GameObject> _arrowPrefab;


    private void Awake()
    {
        _inputs = new InputSystem_Actions();
        _arrowPrefab = new Dictionary<Direction, GameObject>();
        //_spacing = bodyPrefab.GetComponent<Renderer>().bounds.size.x / 2f;
    }

    private void OnEnable()
    {
        _inputs.Enable();
        _inputs.UI.ClickAtPos.performed += HandleInput;
    }

    private void OnDisable()
    {
        _inputs.UI.ClickAtPos.performed -= HandleInput;
        _inputs.Disable();
    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        var screenPos = context.ReadValue<Vector2>();

        Debug.Log($"Screen Pos: {screenPos}");
        Debug.Log($"World Pos: {camera.ScreenToWorldPoint(screenPos)}");
    }

    public void DrawGridInit(List<Verticle> grid)
    {
        for (int i = 0; i < grid.Count; i++)
        {
            Vector3 spawnPosition = new(grid[i].XVerticle, grid[i].YVerticle, 0);
            if (grid[i].Type == VerticleType.HEAD)
                Instantiate(headPrefab, spawnPosition, Quaternion.identity);
            else if (grid[i].Type == VerticleType.TAIL)
                Instantiate(tailPrefab, spawnPosition, Quaternion.identity);
            else if (grid[i].Type == VerticleType.BODY)
                Instantiate(bodyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void Start()
    {
        IStorage storage = new LocalStorage();
        IConfig config = new ConfigManager(storage);
        //temp for test
        var controller = new ArrowController(config);
        controller.LoadData();
        var uiManager = new UIManager(controller);
        DrawGridInit(uiManager.InitBoard(_spacing));
        cameraModifier.FitCamera(controller.GetConfigData().BoardWidth, controller.GetConfigData().BoardHeight, _spacing);

        var configData = controller.GetConfigData();

        foreach (var arrow in configData.Arrows)
        {
            var go = new GameObject("Arrow");
            var builder = go.AddComponent<ArrowMeshBuilder>();
            builder.width = 0.3f;
            builder.arrowMaterial = arrowMaterial;

            var points = builder.BuildPathPoints(arrow.ArrowIndices, configData.BoardWidth, _spacing);
            builder.BuildArrowMesh(points, builder.width);
        }
    }

    void Update()
    {
        
    }


}
