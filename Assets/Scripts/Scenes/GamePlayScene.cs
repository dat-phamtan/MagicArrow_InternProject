using Assets.Scripts.Boosters;
using Assets.Scripts.Config;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using Assets.Scripts.IO;
using Assets.Scripts.UI;
using Assets.Scripts.Utility;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = Assets.Scripts.Input.PlayerInput;

public class GamePlayScene : MonoBehaviour, IEventHandler
{
    public new Camera camera;
    public GameObject headPrefab;
    public GameObject bodyPrefab;
    public GameObject tailPrefab;
    public ArrowAssembler arrowAssembler;
    public CameraModifier cameraModifier;
    public float spacing = 1f;
    public float speed = 10f;
    public float exitPadding = 10f;
    public int heart = 3;

    private bool _isHolded = false;
    private Vector2 currentPos;

    private IController _controller;
    private IUIManager _uiManager;
    private InputSystem_Actions _inputActions;
    private ConfigData _configData;
    private Dictionary<int, GameObject> _arrowRoots;
    private Dictionary<int, ArrowMeshBuilder> _arrowBuilders;
    private Dictionary<int, Vector3[]> _arrowPaths;

    public event Action<Vector3> OnInteractAt;
    public event Action<int> OnUnblockInteractWidthArrow;
    


    private void Awake()
    {
        _controller = Locator.Get<IController>();
        _uiManager = Locator.Get<IUIManager>();

        _inputActions = new InputSystem_Actions();
        _arrowRoots = new Dictionary<int, GameObject>();
        _arrowBuilders = new Dictionary<int, ArrowMeshBuilder>();
        _arrowPaths = new Dictionary<int, Vector3[]>();
    }

    void Start()
    {
        _controller.Init(this);
        _uiManager.Init(this);
        _configData = _controller.GetConfigData();
        for (int i = 0; i < _configData.Arrows.Length; i++)
        {
            var root = arrowAssembler.Build(_controller, _configData.Arrows[i], _configData.BoardWidth, _configData.BoardHeight, spacing, out var points, out var builder);
            _arrowRoots[i] = root;
            _arrowBuilders[i] = builder;
            _arrowPaths[i] = points;  
        }
        cameraModifier.FitCamera(_configData.BoardWidth, _configData.BoardHeight, spacing);
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.UI.Clicked.started += HandlePressedStart;
        _inputActions.UI.Clicked.canceled += HandlePressedEnd;
        _inputActions.UI.InteractAtPos.performed += HandleSufInput;
        _inputActions.UI.ClickAtPos.canceled += HandlePlayZoneClicked;
        

        _controller.OnMoveArrowSuccess += HandleMoveSuccess;
        _controller.OnMoveArrowFail += HandleMoveFail;
        _controller.OnEraseArrowAt += HandleEraseArrowAt;

    }

    private void HandlePressedEnd(InputAction.CallbackContext context)
    {
            
        _isHolded = false;  
    }

    private void HandlePressedStart(InputAction.CallbackContext context)
    {
        _isHolded = true;
    }

    private void HandleSufInput(InputAction.CallbackContext context)
    {
        if (_isHolded)
        {
            if (currentPos == null)
            {
                currentPos = context.ReadValue<Vector2>();
                return;
            }
            var movedPosition = (context.ReadValue<Vector2>() - currentPos).normalized;
            //cameraModifier.transform.position = movedPosition * 4f;
            Debug.Log($"{movedPosition}");
        }
    }

    private void OnDisable()
    {
        _inputActions.UI.Clicked.started -= HandlePressedStart;
        _inputActions.UI.Clicked.canceled -= HandlePressedEnd;
        _inputActions.UI.InteractAtPos.performed -= HandleSufInput;
        _inputActions.UI.ClickAtPos.canceled -= HandlePlayZoneClicked;

        _controller.OnMoveArrowSuccess -= HandleMoveSuccess;
        _controller.OnMoveArrowFail -= HandleMoveFail;
        _controller.OnEraseArrowAt -= HandleEraseArrowAt;
        _inputActions.Disable();
    }

    private void HandlePlayZoneClicked(InputAction.CallbackContext context)
    {
        if (_isHolded)
            return;
        var screenPos = context.ReadValue<Vector2>();
        OnInteractAt?.Invoke(camera.ScreenToWorldPoint(screenPos));
    }

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

        StartCoroutine(AnimateMoveSuccess(arrowRoot, builder, path, direction, boardIndex));
    }

    private void HandleMoveFail(int boardIndex, int deltaIndex)
    {
        var arrowRoot = _arrowRoots[boardIndex];
        var builder = _arrowBuilders[boardIndex];
        var path = _arrowPaths[boardIndex];

        var arrow = _configData.Arrows[boardIndex];
        var headPos = new Position(arrow.XArrowHead, arrow.YArrowHead);
        var direction = DirectionToVector(_controller.GetDirectionAtPosition(headPos));

        StartCoroutine(AnimateMoveFail(arrowRoot, builder, path, direction, deltaIndex, boardIndex));
        
    }

    private void HandleEraseArrowAt(int boardIndex)
    {
        var arrowRoot = _arrowRoots[boardIndex];
        _arrowRoots.Remove(boardIndex);
        _arrowBuilders.Remove(boardIndex);
        _arrowPaths.Remove(boardIndex);
        Destroy(arrowRoot);
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

    private IEnumerator AnimateMoveFail(GameObject arrowRoot, ArrowMeshBuilder builder, Vector3[] originalPath, Vector3 exitDir, int deltaIndex, int boardIndex)
    {
        int n = originalPath.Length;
        float targetTravel = (deltaIndex - 1) * spacing;
        float travelled = 0f;
        //var originPath = originalPath;
        var newPath = new Vector3[n];
        while (travelled < targetTravel)
        {
            travelled += speed * Time.deltaTime;
            for (int i = 0; i < n; i++)
            {
                float behind = i * spacing - travelled;
                newPath[i] = PositionBehindHead(originalPath, exitDir, behind);
            }
            builder.BuildArrow(_controller, _configData.Arrows[boardIndex].ArrowIndices, newPath, spacing);
            yield return null;
        }

        while (travelled > 0)
        {
            travelled -= speed * Time.deltaTime;
            for (int i = 0; i < n; i++)
            {
                float behind = i * spacing - travelled;
                newPath[i] = PositionBehindHead(originalPath, exitDir, behind);
            }
            builder.BuildArrow(_controller, _configData.Arrows[boardIndex].ArrowIndices, newPath, spacing);
            yield return null;
        }
        builder.BuildArrow(_controller, _configData.Arrows[boardIndex].ArrowIndices, originalPath, spacing);
        OnUnblockInteractWidthArrow?.Invoke(boardIndex);
    }

    private IEnumerator AnimateMoveSuccess(GameObject arrowRoot, ArrowMeshBuilder builder, Vector3[] originalPath, Vector3 exitDir, int boardIndex)
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
            builder.BuildArrow(_controller, _configData.Arrows[boardIndex].ArrowIndices, newPath, spacing);
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
