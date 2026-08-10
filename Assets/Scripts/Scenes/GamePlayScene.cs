using Assets.Scripts.Boosters;
using Assets.Scripts.Config;
using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Input;
using Assets.Scripts.IO;
using Assets.Scripts.UI;
using Assets.Scripts.Ultility;
using Assets.Scripts.Utility;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    public GameObject dotPrefab;

    public ArrowAssembler arrowAssembler;
    public CameraModifier cameraModifier;
    public float spacing = 1f;
    public float speed = 10f;
    public float exitPadding = 10f;
    public int heart = 3;
    public float cornerRadius = 0.12f;
    public int segments = 10;

    private bool _isHolded = false;

    private Vector2 currentPos;

    private IController _controller;
    private IUIManager _uiManager;
    //private IArrowAssember _arrowAssember;
    private InputSystem_Actions _inputActions;
    private ConfigData _configData;
    private Dictionary<int, GameObject> _arrowRoots;
    private Dictionary<int, ArrowMeshBuilder> _arrowBuilders;
    private Dictionary<int, Vector3[]> _arrowPaths;
    private Dictionary<int, Vector3[]> _curvedPath;
    private Dictionary<int, float[]> _cumulativeLength;

    public event Action<Vector3> OnInteractAt;
    public event Action<int> OnUnblockInteractWidthArrow;
    public event Action<GameObject> OnCollidedAnimation;

    private void Awake()
    {
        _controller = Locator.Get<IController>();
        _uiManager = Locator.Get<IUIManager>();

        //_arrowAssember = new ArrowAssembler();
        _inputActions = new InputSystem_Actions();
        _arrowRoots = new Dictionary<int, GameObject>();
        _arrowBuilders = new Dictionary<int, ArrowMeshBuilder>();
        _arrowPaths = new Dictionary<int, Vector3[]>();
        _curvedPath = new Dictionary<int, Vector3[]>();
        _cumulativeLength = new Dictionary<int, float[]>();
    }

    void Start()
    {
        _controller.Init(this);
        _uiManager.Init(this);
        _configData = _controller.GetConfigData();
        BoardInit();
        arrowAssembler.Init(this);
        cameraModifier.FitCamera(_configData.BoardWidth, _configData.BoardHeight, spacing);
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.UI.Clicked.started += HandlePressedStart;
        _inputActions.UI.Clicked.canceled += HandlePressedEnd;
        _inputActions.UI.InteractAtPos.performed += HandleSufInput;
        _inputActions.UI.ClickAtPos.performed += HandlePlayZoneClicked;
        

        _controller.OnMoveArrowSuccess += HandleMoveSuccess;
        _controller.OnMoveArrowFail += HandleMoveFail;
        _controller.OnEraseArrowAt += HandleEraseArrowAt;

    }

    private void OnDisable()
    {
        _inputActions.UI.Clicked.started -= HandlePressedStart;
        _inputActions.UI.Clicked.canceled -= HandlePressedEnd;
        _inputActions.UI.InteractAtPos.performed -= HandleSufInput;
        _inputActions.UI.ClickAtPos.performed -= HandlePlayZoneClicked;

        _controller.OnMoveArrowSuccess -= HandleMoveSuccess;
        _controller.OnMoveArrowFail -= HandleMoveFail;
        _controller.OnEraseArrowAt -= HandleEraseArrowAt;
        _inputActions.Disable();
    }

    private void BoardInit()
    {
        int width = _configData.BoardWidth;
        int height = _configData.BoardHeight;
        for (int i = 0; i < _configData.Arrows.Length; i++)
        {
            var gridPath = arrowAssembler.BuildPathPoints(_configData.Arrows[i].ArrowIndices, width, height, spacing);
            CurvePathUtils.BuildCurved(gridPath, cornerRadius, segments, out var curvedPath, out var cumulativeLength);
            var root = arrowAssembler.Build(_configData.Arrows[i], curvedPath, cumulativeLength, spacing, out var builder);
            
            _arrowRoots[i] = root;
            _arrowBuilders[i] = builder;
            _arrowPaths[i] = gridPath;
            _curvedPath[i] = curvedPath;
            _cumulativeLength[i] = cumulativeLength;
        }
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

    private void HandlePlayZoneClicked(InputAction.CallbackContext context)
    {
        if (_isHolded)
            return;
        var screenPos = context.ReadValue<Vector2>();
        OnInteractAt?.Invoke(camera.ScreenToWorldPoint(screenPos));
    }

    private void HandleMoveSuccess(int configIndex)
    {
        var arrowRoot = _arrowRoots[configIndex];
        var builder = _arrowBuilders[configIndex];
        var path = _arrowPaths[configIndex];

        var arrow = _configData.Arrows[configIndex];
        var headPos = new Position(arrow.XArrowHead, arrow.YArrowHead);
        var direction = DirectionToVector(_controller.GetDirectionAtPosition(headPos));

        _arrowRoots.Remove(configIndex);
        _arrowBuilders.Remove(configIndex);
        _arrowPaths.Remove(configIndex);

        StartCoroutine(AnimateMoveSuccess(arrowRoot, builder, _curvedPath[configIndex], _cumulativeLength[configIndex], direction, configIndex));
    }

    private void HandleMoveFail(int interactedConfigIndex, int collidedConfigIndex, int deltaIndex)
    {
        var interactedArrowRoot = _arrowRoots[interactedConfigIndex];
        var collidedArrowRoot = _arrowRoots[collidedConfigIndex];
        var builder = _arrowBuilders[interactedConfigIndex];
        var path = _arrowPaths[interactedConfigIndex];

        var arrow = _configData.Arrows[interactedConfigIndex];
        var headPos = new Position(arrow.XArrowHead, arrow.YArrowHead);
        var direction = DirectionToVector(_controller.GetDirectionAtPosition(headPos));

        StartCoroutine(AnimateMoveFail(interactedArrowRoot, collidedArrowRoot, builder, _curvedPath[interactedConfigIndex], _cumulativeLength[interactedConfigIndex], direction, deltaIndex, interactedConfigIndex));
    }

    private void HandleEraseArrowAt(int configIndex)
    {
        var arrowRoot = _arrowRoots[configIndex];
        _arrowRoots.Remove(configIndex);
        _arrowBuilders.Remove(configIndex);
        _arrowPaths.Remove(configIndex);
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

    private IEnumerator AnimateMoveFail(GameObject interactedArrowRoot, GameObject collidedArrowRoot, ArrowMeshBuilder builder, Vector3[] originalPath, float[] cumulativeLength, Vector3 exitDir, int deltaIndex, int interactedConfigIndex)
    { 
        float exitDistance = (deltaIndex == 1) ? 0.5f * spacing : (deltaIndex - 1) * spacing;
        int n = originalPath.Length;
        //float totalLength = (n - 1) * spacing;
        float totalLength = cumulativeLength[^1];
        float travelled = 0f;

        while (travelled < exitDistance)
        {
            travelled = Mathf.Min(travelled + speed * Time.deltaTime, exitDistance);
            float headDist = -travelled;
            float tailDist = totalLength - travelled;

            var newPathList = new List<Vector3>();
            //newPathList.Add(PositionBehindHead(originalPath, exitDir, headDist));
            newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, headDist));

            for (int i = 0; i < n; i++)
            {
                float nodeDist = cumulativeLength[i];
                if (nodeDist > headDist && nodeDist < tailDist)
                    newPathList.Add(originalPath[i]);
            }
            //newPathList.Add(PositionBehindHead(originalPath, exitDir, tailDist));
            newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, tailDist));
            builder.BuildArrow(newPathList.ToArray(), cumulativeLength, spacing);
            yield return null;
        }

        HandleArrowFirstFail(interactedConfigIndex, interactedArrowRoot, collidedArrowRoot);

        while (travelled > 0)
        {
            travelled = Mathf.Max(travelled - speed * Time.deltaTime, 0f);
            float headDist = -travelled;
            float tailDist = totalLength - travelled;

            var newPathList = new List<Vector3>();
            //newPathList.Add(PositionBehindHead(originalPath, exitDir, headDist));
            newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, headDist));

            for (int i = 0; i < n; i++)
            {
                float nodeDist = cumulativeLength[i];
                if (nodeDist > headDist && nodeDist < tailDist)
                {
                    newPathList.Add(originalPath[i]);
                }
            }
            //newPathList.Add(PositionBehindHead(originalPath, exitDir, tailDist));
            newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, tailDist));
            builder.BuildArrow(newPathList.ToArray(), cumulativeLength, spacing);
            yield return null;
        }

        builder.BuildArrow(originalPath, cumulativeLength, spacing);
        OnUnblockInteractWidthArrow?.Invoke(interactedConfigIndex);
    }

    private IEnumerator AnimateMoveSuccess(GameObject arrowRoot, ArrowMeshBuilder builder, Vector3[] originalPath, float[] cumulativeLength, Vector3 exitDir, int configIndex)
    {
        float exitDistance = camera.orthographicSize * 2f * camera.aspect + exitPadding;
        int n = originalPath.Length;
        //float totalLength = (n - 1) * spacing;
        float totalLength = cumulativeLength[^1];
        float targetTravel = totalLength + exitDistance;
        float travelled = 0f;

        while (travelled < targetTravel)
        {
            travelled += speed * Time.deltaTime;
            float headDist = -travelled;
            float tailDist = totalLength - travelled;

            var newPathList = new List<Vector3>();
            //newPathList.Add(PositionBehindHead(originalPath, exitDir, headDist));
            newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, headDist));

            for (int i = 0; i < n; i++)
            {
                //float nodeDist = i * spacing;
                float nodeDist = cumulativeLength[i];
                if (nodeDist > headDist && nodeDist < tailDist)
                {
                    newPathList.Add(originalPath[i]);
                }
            }
            //newPathList.Add(PositionBehindHead(originalPath, exitDir, tailDist));
            newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, tailDist));
            builder.BuildArrow(newPathList.ToArray(), cumulativeLength, spacing);
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

    private Vector3 PositionAtDistance(Vector3[] curvedPath, float[] cumLen, float distance)
    {
        if (distance <= 0f)
        {
            var dir = (curvedPath[1] - curvedPath[0]).normalized;
            return curvedPath[0] + dir * distance;
        }

        int lastPos = cumLen.Length - 1;
        if (distance >= cumLen[lastPos])
        {
            var dir = (curvedPath[lastPos] - curvedPath[lastPos - 1]).normalized;
            return curvedPath[lastPos] + dir * (distance - cumLen[lastPos]);
        }

        int lo = 0;
        while (cumLen[lo + 1] < distance)
            lo++;
        float t = (distance - cumLen[lo]) / (cumLen[lo + 1] - cumLen[lo]);
        return Vector3.Lerp(curvedPath[lo], curvedPath[lo + 1], t);
    }

    private void HandleArrowFirstFail(int configIndex, GameObject interactedArrowRoot, GameObject collidedArrowRoot)
    {
        if (_controller.IsFirstMoveFail(configIndex))
        {
            arrowAssembler.ChangeArrowColor(1, interactedArrowRoot.GetComponent<ArrowMeshBuilder>());
        }
        OnCollidedAnimation?.Invoke(collidedArrowRoot);
    }
}
