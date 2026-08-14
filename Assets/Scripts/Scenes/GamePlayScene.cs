using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.UI;
using Assets.Scripts.Ultility;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class GamePlayScene : MonoBehaviour, IEventHandler
{
    public float spacing = 1f;
    public float speed = 10f;
    public float exitPadding = 10f;
    public int heart = 3;
    public float cornerRadius = 0.12f;
    public int segments = 10;
    public float panSpeed = 1.2f;
    public float previousPinchDistance = 0f;

    public new Camera camera;
    public GameObject headPrefab;
    public GameObject bodyPrefab;
    public GameObject tailPrefab;
    public GameObject dotPrefab;

    public Image glowHit;
    public float fadeInDuration = 1.5f; 
    public float waitDuration = 1f;     
    public float fadeOutDuration = 1.5f;

    public ArrowAssembler arrowAssembler;
    public CameraModifier cameraModifier;
    public PopUpManager popUpManager;

    private bool _isHolded = false;
    private Vector2 _currentPos;
    private int _boardWidth;
    private int _boardHeight;
    //private Tile _generatedCirleTile;

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
    public event Action<bool> OnAnimatedComplete;
    public event Action<string> OnTurnPopupOn;
    public event Action OnDisableCameraCenter;

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
        _controller.Init(this, popUpManager);
        _uiManager.Init(this);

        _configData = _controller.GetConfigData();
        _boardWidth = _configData.BoardWidth;
        _boardHeight = _configData.BoardHeight;
        BoardInit();
        arrowAssembler.Init(this);
        popUpManager.Init(_controller, this);
        cameraModifier.Init(this, _boardWidth, _boardHeight, spacing);
        cameraModifier.FitCamera();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.UI.Tap.performed += HandlePlayZoneClicked;
        //_inputActions.UI.Touch2Contact.performed += HandlePlayZoneClicked;

        _controller.OnMoveArrowSuccess += HandleMoveSuccess;
        _controller.OnMoveArrowFail += HandleMoveFail;
        _controller.OnEraseArrowAt += HandleEraseArrowAt;
        _controller.OnRerenderBoard += BoardInit;
        _controller.OnTurnPopupOn += TurnPopUp;
        _controller.OnLoseHeart += GlowHitAnimation;
    }

    private void OnDisable()
    {
        _inputActions.UI.Tap.performed -= HandlePlayZoneClicked;
        //_inputActions.UI.Touch2Contact.performed -= HandlePlayZoneClicked;

        _controller.OnMoveArrowSuccess -= HandleMoveSuccess;
        _controller.OnMoveArrowFail -= HandleMoveFail;
        _controller.OnEraseArrowAt -= HandleEraseArrowAt;
        _controller.OnRerenderBoard -= BoardInit;
        _controller.OnTurnPopupOn -= TurnPopUp;
        _controller.OnLoseHeart -= GlowHitAnimation;
        _inputActions.Disable();
    }

    private void Update()
    {
        if (!_controller.IsWinOrLose())
        {
            HandleZoom();
            if (!IsSecondTouched())
            {
                HandlePan();
            }
            if (!_inputActions.UI.Press.IsPressed())
            {
                OnDisableCameraCenter?.Invoke();
            }
                
        }
    }




    private bool IsSecondTouched()
    {
        return _inputActions.UI.Touch2Contact.IsPressed();
    }

    private void HandlePan()
    {
        if (_inputActions.UI.Press.IsPressed())
        {
            var delta = _inputActions.UI.DragPosition.ReadValue<Vector2>();
            var moveDirection = Time.deltaTime * new Vector3(-delta.x, -delta.y, 0);

            cameraModifier.TranslateCamera(moveDirection);
        }
    }

    private void HandleZoom()
    {
        if (_inputActions.UI.Touch2Contact.IsPressed() && _inputActions.UI.Tap.IsPressed())
        {
            var pos1 = _inputActions.UI.Touch1.ReadValue<Vector2>();
            var pos2 = _inputActions.UI.Touch2.ReadValue<Vector2>();

            var distance = Vector2.Distance(pos1, pos2);
            if (previousPinchDistance > 0)
            {
                var delta = previousPinchDistance - distance;
                cameraModifier.ZoomCamera(delta * 0.01f);
            }
            previousPinchDistance = distance;
        }
        else
        {
            previousPinchDistance = 0f;
        }
    }


    private IEnumerator WaitForAllArrowCoroutine(bool isWin)
    {
        if (isWin)
        {
            while (_controller.GetSuccessAnimationNum() > 0)
                yield return null;
            OnTurnPopupOn?.Invoke("VICTORY");
        }
        else
        {
            while (_controller.GetFailAnimationNum() > 0)
                yield return null;
            OnTurnPopupOn?.Invoke("DEFEAT");
        }
    }

    private void TurnPopUp(bool isWin)
    {
        StartCoroutine(WaitForAllArrowCoroutine(isWin));
    }

    private void GlowHitAnimation()
    {
        //Debug.Log("++++++");
        //glowHit = GetComponent<Image>();
        //StartCoroutine(PlayGlowHitAnimation());
        //Debug.Log("________");
    }

    private IEnumerator PlayGlowHitAnimation()
    {
        Color glowHitColor = glowHit.color;
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            glowHitColor.a = Mathf.Lerp(0f, 1f, timer / fadeInDuration);
            glowHit.color = glowHitColor;
            yield return null;
        }

        glowHitColor.a = 1f;
        glowHit.color = glowHitColor;

        yield return new WaitForSeconds(waitDuration);

        timer = 0f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            glowHitColor.a = Mathf.Lerp(1f, 0f, timer / fadeInDuration);
            glowHit.color = glowHitColor;
            yield return null;
        }

        glowHitColor.a = 0f;
        glowHit.color = glowHitColor;
    }

    private void BoardInit()
    {
        foreach (var root in _arrowRoots.Values)
        {
            if (root != null)
                Destroy(root);
        }
        _arrowRoots.Clear();
        _arrowBuilders.Clear();
        _arrowPaths.Clear();
        _curvedPath.Clear();
        _cumulativeLength.Clear();

        for (int i = 0; i < _configData.Arrows.Length; i++)
        {
            var gridPath = arrowAssembler.BuildPathPoints(_configData.Arrows[i].ArrowIndices, _boardWidth, _boardHeight, spacing);
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
            if (_currentPos == null)
            {
                _currentPos = context.ReadValue<Vector2>();
                return;
            }
            var movedPosition = (context.ReadValue<Vector2>() - _currentPos).normalized;
            //cameraModifier.transform.position = movedPosition * 4f;
            Debug.Log($"{movedPosition}");
        }
    }

    private void HandlePlayZoneClicked(InputAction.CallbackContext context)
    {
        if (_isHolded)
            return;
        var screenPos = _inputActions.UI.Position.ReadValue<Vector2>();
        //Debug.Log($"{screenPos.x}/{screenPos.y}");
        OnInteractAt?.Invoke(camera.ScreenToWorldPoint(screenPos));
    }

    private void HandleMoveSuccess(int configIndex)
    {
        var arrowRoot = _arrowRoots[configIndex];
        var builder = _arrowBuilders[configIndex];

        _arrowRoots.Remove(configIndex);
        _arrowBuilders.Remove(configIndex);
        _arrowPaths.Remove(configIndex);

        StartCoroutine(AnimateMoveSuccess(arrowRoot, builder, _curvedPath[configIndex], _cumulativeLength[configIndex], configIndex));
    }

    private void HandleMoveFail(int interactedConfigIndex, int collidedConfigIndex, int deltaIndex)
    {
        var interactedArrowRoot = _arrowRoots[interactedConfigIndex];
        var collidedArrowRoot = _arrowRoots[collidedConfigIndex];
        var builder = _arrowBuilders[interactedConfigIndex];

        StartCoroutine(AnimateMoveFail(interactedArrowRoot, collidedArrowRoot, builder, _curvedPath[interactedConfigIndex], _cumulativeLength[interactedConfigIndex], deltaIndex, interactedConfigIndex));
    }

    private void HandleEraseArrowAt(int configIndex)
    {
        var arrowRoot = _arrowRoots[configIndex];
        _arrowRoots.Remove(configIndex);
        _arrowBuilders.Remove(configIndex);
        _arrowPaths.Remove(configIndex);
        Destroy(arrowRoot);
    }

    private IEnumerator AnimateMoveFail(GameObject interactedArrowRoot, GameObject collidedArrowRoot, ArrowMeshBuilder builder, Vector3[] originalPath, float[] cumulativeLength, int deltaIndex, int interactedConfigIndex)
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
            newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, headDist));

            for (int i = 0; i < n; i++)
            {
                float nodeDist = cumulativeLength[i];
                if (nodeDist > headDist && nodeDist < tailDist)
                    newPathList.Add(originalPath[i]);
            }
            newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, tailDist));
            builder.BuildArrow(newPathList.ToArray(), cumulativeLength, spacing);
            yield return null;
        }

        HandleFirstFailAnimation(interactedConfigIndex, interactedArrowRoot, collidedArrowRoot);

        while (travelled > 0)
        {
            travelled = Mathf.Max(travelled - speed * Time.deltaTime, 0f);
            float headDist = -travelled;
            float tailDist = totalLength - travelled;

            var newPathList = new List<Vector3>();
            newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, headDist));

            for (int i = 0; i < n; i++)
            {
                float nodeDist = cumulativeLength[i];
                if (nodeDist > headDist && nodeDist < tailDist)
                {
                    newPathList.Add(originalPath[i]);
                }
            }
            newPathList.Add(PositionAtDistance(originalPath, cumulativeLength, tailDist));
            builder.BuildArrow(newPathList.ToArray(), cumulativeLength, spacing);
            yield return null;
        }

        builder.BuildArrow(originalPath, cumulativeLength, spacing);
        OnUnblockInteractWidthArrow?.Invoke(interactedConfigIndex);
        OnAnimatedComplete?.Invoke(false);
    }

    private IEnumerator AnimateMoveSuccess(GameObject arrowRoot, ArrowMeshBuilder builder, Vector3[] originalPath, float[] cumulativeLength, int configIndex)
    {
        float exitDistance = camera.orthographicSize * 2f * camera.aspect + exitPadding;
        int n = originalPath.Length;
        //float totalLength = (n - 1) * spacing;
        //for (int i = 0; i < cumulativeLength.Length; i++)
        //{
        //    Debug.Log(cumulativeLength[i]);
        //}

        float totalLength = cumulativeLength[^1];
        float targetTravel = totalLength + exitDistance;
        float travelled = 0f;

        while (travelled < targetTravel)
        {
            travelled += speed * Time.deltaTime;
            float headDist = -travelled; //at goal
            float tailDist = totalLength - travelled; //goal - travelled

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
            //Debug.Log(newPathList.Count);
            builder.BuildArrow(newPathList.ToArray(), cumulativeLength, spacing);
            yield return null;
        }
        Destroy(arrowRoot);
        OnAnimatedComplete?.Invoke(true);
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

    private void HandleFirstFailAnimation(int configIndex, GameObject interactedArrowRoot, GameObject collidedArrowRoot)
    {
        if (_controller.IsFirstMoveFail(configIndex))
        {
            arrowAssembler.ChangeArrowColor(1, interactedArrowRoot.GetComponent<ArrowMeshBuilder>());
        }
        OnCollidedAnimation?.Invoke(collidedArrowRoot);
    }


}



















    //FUNC THAT NO MORE USED
    //private Vector3 PositionBehindHead(Vector3[] path, Vector3 exitDir, float distanceFromInitHead)
    //{
    //    //out of gameplay
    //    if (distanceFromInitHead <= 0f)
    //        return path[0] - exitDir * distanceFromInitHead;

    //    int loIndex = Mathf.Min((int)(distanceFromInitHead / spacing), path.Length - 2);
    //    //Debug.Log(loIndex);
    //    float t = (distanceFromInitHead - loIndex * spacing) / spacing;
    //    return Vector3.Lerp(path[loIndex], path[loIndex + 1], t);
    //}

    //private Vector3 DirectionToVector(Direction dir)
    //{
    //    switch (dir)
    //    {
    //        case Direction.RIGHT:
    //            return Vector3.right;
    //        case Direction.LEFT:
    //            return Vector3.left;
    //        case Direction.UP:
    //            return Vector3.up;
    //        case Direction.DOWN:
    //            return Vector3.down;
    //        default:
    //            return Vector3.left;
    //    }
    //}
