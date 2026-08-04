using Assets.Scripts.Config;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using Assets.Scripts.IO;
using Assets.Scripts.UI;
using NUnit.Framework;
using System;
using System.Collections;
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

    //public Material arrowMaterial;
    public CameraModifier cameraModifier;
    public float spacing = 1f;
    public float speed = 10f;
    public float exitPadding = 10f;


    private IConfig _config;
    private IStorage _storage;
    private IInput _input;
    private IController _controller;
    private IUIManager _uiManager;
    private InputSystem_Actions _inputActions;
    private ConfigData _configData;
    private List<GameObject> _partsList;
    private Dictionary<int, GameObject> _arrowRoots;
    private Dictionary<int, ArrowMeshBuilder> _arrowBuilders;
    private Dictionary<int, Vector3[]> _arrowPaths;



    private void Awake()
    {
        _storage = new LocalStorage();
        _config = new ConfigManager(_storage);
        _input = new PlayerInput(spacing);
        _controller = new ArrowController(_config, _input);
        _inputActions = new InputSystem_Actions();
        _uiManager = new UIManager(_controller);

        _arrowRoots = new Dictionary<int, GameObject>();
        _arrowBuilders = new Dictionary<int, ArrowMeshBuilder>();
        _arrowPaths = new Dictionary<int, Vector3[]>();
    }

    void Start()
    {
        _controller.Init();
        _configData = _controller.GetConfigData();

        for (int i = 0; i < _configData.Arrows.Length; i++)
        {
            var root = arrowAssembler.Build(_configData.Arrows[i], _configData.BoardWidth, _configData.BoardHeight, spacing, out var points, out var builder);
            _arrowRoots[i] = root;
            _arrowBuilders[i] = builder;
            _arrowPaths[i] = points;  
        }
        cameraModifier.FitCamera(_configData.BoardWidth, _configData.BoardHeight, spacing);
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.UI.ClickAtPos.performed += HandleInput;
        _controller.OnMoveArrowSuccess += HandleMoveSuccess;
        _controller.OnMoveArrowFail += HandleMoveFail;

    }

    private void OnDisable()
    {
        _inputActions.UI.ClickAtPos.performed -= HandleInput;
        _controller.OnMoveArrowSuccess -= HandleMoveSuccess;
        _controller.OnMoveArrowFail -= HandleMoveFail;
        _inputActions.Disable();
    }

    private void HandleInput(InputAction.CallbackContext context)
    {
        var screenPos = context.ReadValue<Vector2>();
        _input.HandleInput(camera.ScreenToWorldPoint(screenPos));
    }

    //public void DrawBoardTest(List<Verticle> grid)
    //{
    //    _partsList = new List<GameObject>();
    //    for (int i = 0; i < grid.Count; i++)
    //    {
    //        Vector3 spawnPosition = new(grid[i].XVerticle, grid[i].YVerticle, 0);
    //        //Debug.Log($"{grid[i].XVerticle }/{ grid[i].YVerticle}");
    //        if (grid[i].Type == VerticleType.HEAD)
    //            _partsList.Add(Instantiate(headPrefab, spawnPosition, Quaternion.identity));
    //        else if (grid[i].Type == VerticleType.TAIL)
    //            _partsList.Add(Instantiate(tailPrefab, spawnPosition, Quaternion.identity));
    //        else if (grid[i].Type == VerticleType.BODY)
    //            _partsList.Add(Instantiate(bodyPrefab, spawnPosition, Quaternion.identity));
    //    }
    //}

    //private void MoveArrowAway(int boardIndex)
    //{
    //    var arrowIndices = _configData.Arrows[boardIndex].ArrowIndices;
    //    for (int i = 0; i < arrowIndices.Length; i++)
    //    {
    //        Destroy(_partsList[arrowIndices[i]]);
    //    }
    //}

    private void HandleMoveSuccess(int boardIndex)
    {
        var arrowRoot = _arrowRoots[boardIndex];
        var builder = _arrowBuilders[boardIndex];
        var path = _arrowPaths[boardIndex];

        var arrow = _configData.Arrows[boardIndex];
        var headPos = new Position(arrow.XArrowHead, arrow.YArrowHead);
        var direction = DirectionToVector(_controller.GetDirectionAtPosition(headPos));

        _arrowRoots.Remove(boardIndex);
        _arrowBuilders.Remove(boardIndex);
        _arrowPaths.Remove(boardIndex);

        StartCoroutine(AnimateMoveSuccess(arrowRoot, builder, path, direction));
    }

    private void HandleMoveFail(int boardIndex, int deltaIndex)
    {
        var arrowRoot = _arrowRoots[boardIndex];
        var builder = _arrowBuilders[boardIndex];
        var path = _arrowPaths[boardIndex];

        var arrow = _configData.Arrows[boardIndex];
        var headPos = new Position(arrow.XArrowHead, arrow.YArrowHead);
        var direction = DirectionToVector(_controller.GetDirectionAtPosition(headPos));

        StartCoroutine(AnimateMoveFail(arrowRoot, builder, path, direction, deltaIndex));
    }

    private Vector3 DirectionToVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.RIGHT:
                return Vector3.right;
            case Direction.LEFT:
                return Vector3.left;
            case Direction.UP:
                return Vector3.up;
            case Direction.DOWN:
                return Vector3.down;
            default:
                return Vector3.left;
        }
    }

    private IEnumerator AnimateMoveFail(GameObject arrowRoot, ArrowMeshBuilder builder, Vector3[] originalPath, Vector3 exitDir, int deltaIndex)
    {
        int n = originalPath.Length;
        float targetTravel = (deltaIndex - 1) * spacing;
        float travelled = 0f;
        var originPath = originalPath;
        var newPath = new Vector3[n];
        while (travelled < targetTravel)
        {
            travelled += speed * Time.deltaTime;
            for (int i = 0; i < n; i++)
            {
                float behind = i * spacing - travelled;
                newPath[i] = PositionBehindHead(originalPath, exitDir, behind);
            }
            builder.BuildArrow(newPath);
            yield return null;
        }
        //need fix

        //builder.BuildArrow(originPath);
        Array.Reverse(originPath);
        travelled = 0;
        while (travelled < targetTravel)
        {
            travelled += speed * Time.deltaTime;
            for (int i = 0; i < n; i++)
            {
                float behind = i * spacing - travelled;
                newPath[i] = PositionBehindHead(originPath, -exitDir, behind);
            }
            builder.BuildArrow(newPath);
            yield return null;
        }

    }

    private IEnumerator AnimateMoveSuccess(GameObject arrowRoot, ArrowMeshBuilder builder, Vector3[] originalPath, Vector3 exitDir)
    {
        float exitDistance = camera.orthographicSize * 2f * camera.aspect + exitPadding;

        int n = originalPath.Length;
        float totalLength = (n - 1) * spacing;
        float targetTravel = totalLength + exitDistance;
        float travelled = 0f;

        var newPath = new Vector3[n];
        while (travelled < targetTravel)
        {
            travelled += speed * Time.deltaTime;
            for (int i = 0;  i < n; i++)
            {
                float distanceFromInitHead = i * spacing - travelled; 
                newPath[i] = PositionBehindHead(originalPath, exitDir, distanceFromInitHead);
            }
            builder.BuildArrow(newPath);
            yield return null;
        }
        Destroy(arrowRoot);
    }

    private Vector3 PositionBehindHead(Vector3[] path, Vector3 exitDir, float distanceFromInitHead)
    {
        //out of gameplay
        if (distanceFromInitHead <= 0f)
            return path[0] - exitDir * distanceFromInitHead;

        int loIndex = Mathf.Min((int)(distanceFromInitHead / spacing), path.Length - 2);
        //Debug.Log(loIndex);
        float t = (distanceFromInitHead - loIndex * spacing) / spacing;
        return Vector3.Lerp(path[loIndex], path[loIndex + 1], t);
    }
}
