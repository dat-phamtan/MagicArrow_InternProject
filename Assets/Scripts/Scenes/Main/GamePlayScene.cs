using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.IO;
using Assets.Scripts.Sound;
using Assets.Scripts.UI;
using Assets.Scripts.Ultility;
using Assets.Scripts.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
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

    //border glow
    public Image glowHit;
    public float fadeInDuration = 0.05f; 
    public float waitDuration = 0.1f;     
    public float fadeOutDuration = 0.05f;

    //win
    public float barsAnimation = 0.1f;
    public int arrowDecPos = 200;
    public int arrowDecPosOut = 600;
    public int watchAdGoldReward = 100;
    public GameObject win1Panel;
    public GameObject dataLabel;
    public GameObject[] arrows;
    public TextMeshProUGUI win1Level;
    public TextMeshProUGUI win2Gold;

    public GameObject win2Panel;
    public GameObject[] stars;
    public GameObject[] rewards;
    public TextMeshProUGUI win2Level;
    public Button next;
    public Button watchAd;

    private List<Vector2> _arrowPos;
    private List<Vector2> _arrowPosOut;

    //lose
    private const int StarCost = 420;
    private const int HeartCost = 450;
    public GameObject lose1Popup;
    public Button starAdBtn1;
    public Button starAdBtn2;
    public Button starBuyBtn1;
    public Button starBuyBtn2;
    public GameObject lose2Popup;
    public Button heartAdBtn;
    public Button heartBuyBtn;
    public GameObject lose3Popup;
    public Button retryBtn;
    public GameObject lose4Popup;
    public TextMeshProUGUI live;
    public TextMeshProUGUI regenTime;

    public Button exit1;
    public Button exit2;
    public Button exit3;
    public Button exit4;
    
    //glowing border
    private bool _isGlowing = false;
    private Coroutine _glowing;

    public ArrowAssembler arrowAssembler;
    public CameraModifier cameraModifier;
    public PopUpManager popUpManager;

    private bool _isHolded = false;
    private Vector2 _currentPos;
    private int _boardWidth;
    private int _boardHeight;

    private IController _controller;
    private IGamePlayUI _uiManager;

    private InputSystem_Actions _inputActions;

    private BoardData _configData;
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
        _uiManager = Locator.Get<IGamePlayUI>();

        //_arrowAssember = new ArrowAssembler();
        _inputActions = new InputSystem_Actions();
        _arrowRoots = new Dictionary<int, GameObject>();
        _arrowBuilders = new Dictionary<int, ArrowMeshBuilder>();
        _arrowPaths = new Dictionary<int, Vector3[]>();
        _curvedPath = new Dictionary<int, Vector3[]>();
        _cumulativeLength = new Dictionary<int, float[]>();
        _arrowPos = new List<Vector2>();
        _arrowPosOut = new List<Vector2>();
    }

    void Start()
    {
        _controller.Init(this, popUpManager);
        _uiManager.Init(this);

        SpendHeartOnEnter();

        _configData = _controller.GetConfigData();
        _boardWidth = _configData.BoardWidth;
        _boardHeight = _configData.BoardHeight;
        BoardInit();
        arrowAssembler.Init(this);
        popUpManager.Init(_controller, this);
        cameraModifier.Init(this, _boardWidth, _boardHeight, spacing);
        cameraModifier.FitCamera();

        //sound
        Locator.Get<ISoundManager>().PlayMusic(MusicId.GamePlayTheme);

        //win
        ArrowDecorationInit();
        LoadLevelData();
    }

    private void OnEnable()
    {
        _inputActions.Enable();
        _inputActions.UI.Tap.performed += HandlePlayZoneClicked;
        _controller.OnMoveArrowSuccess += HandleMoveSuccess;
        _controller.OnMoveArrowFail += HandleMoveFail;
        _controller.OnEraseArrowAt += HandleEraseArrowAt;
        _controller.OnRerenderBoard += BoardInit;
        _controller.OnTurnPopupOn += TurnPopUp;
        _controller.OnLoseHeart += GlowHitAnimation;
        _controller.OnVictory += PlayWin1Animation;
        next.onClick.AddListener(HandleNextLevel);
        watchAd.onClick.AddListener(HandleWatchAd);

        starAdBtn1.onClick.AddListener(HandleAdStar1);
        starAdBtn2.onClick.AddListener(HandleAdStar2);
        starBuyBtn1.onClick.AddListener(HandleBuyStar1);
        starBuyBtn2.onClick.AddListener(HandleBuyStar2);
        heartAdBtn.onClick.AddListener(HandleAdHeart);
        heartBuyBtn.onClick.AddListener(HandleBuyHeart);
        retryBtn.onClick.AddListener(HandleRetry);
        exit1.onClick.AddListener(HandleMoveLoseConfirm);
        exit2.onClick.AddListener(HandleMoveLoseFail);
        exit3.onClick.AddListener(HandleBackHome);
        exit4.onClick.AddListener(HandleMoveBackLoseFail);
    }

    private void OnDisable()
    {
        _inputActions.UI.Tap.performed -= HandlePlayZoneClicked;
        _controller.OnMoveArrowSuccess -= HandleMoveSuccess;
        _controller.OnMoveArrowFail -= HandleMoveFail;
        _controller.OnEraseArrowAt -= HandleEraseArrowAt;
        _controller.OnRerenderBoard -= BoardInit;
        _controller.OnTurnPopupOn -= TurnPopUp;
        _controller.OnLoseHeart -= GlowHitAnimation;
        _controller.OnVictory -= PlayWin1Animation;
        next.onClick.RemoveListener(HandleNextLevel);
        watchAd.onClick.RemoveListener(HandleWatchAd);

        starAdBtn1.onClick.RemoveListener(HandleAdStar1);
        starAdBtn2.onClick.RemoveListener(HandleAdStar2);
        starBuyBtn1.onClick.RemoveListener(HandleBuyStar1);
        starBuyBtn2.onClick.RemoveListener(HandleBuyStar2);
        heartAdBtn.onClick.RemoveListener(HandleAdHeart);
        heartBuyBtn.onClick.RemoveListener(HandleBuyHeart);
        retryBtn.onClick.RemoveListener(HandleRetry);
        exit1.onClick.RemoveListener(HandleMoveLoseConfirm);
        exit2.onClick.RemoveListener(HandleMoveLoseFail);
        exit3.onClick.RemoveListener(HandleBackHome);
        exit4.onClick.RemoveListener(HandleMoveBackLoseFail);
        _inputActions.Disable();
    }

    private void HandleMoveLoseConfirm()
    {
        _uiManager.JumpOutAnimation(lose1Popup);
        _uiManager.JumpInAnimation(lose2Popup);
    }

    private void HandleMoveLoseFail()
    {
        _uiManager.JumpOutAnimation(lose2Popup);
        _uiManager.JumpInAnimation(lose3Popup);
    }

    private void HandleBackHome()
    {
        _uiManager.JumpOutAnimation(lose3Popup, () => GoToScene("Home"));
    }

    private void HandleMoveBackLoseFail()
    {
        _uiManager.JumpOutAnimation(lose4Popup);
    }

    private void HandleAdStar1() => ContinueAfterAd(lose1Popup);
    private void HandleAdStar2() => ContinueAfterAd(lose2Popup);

    private void ContinueAfterAd(GameObject popup)
    {
        _controller.RestoreHeart(1);
        _uiManager.JumpOutAnimation(popup);
    }

    private void HandleBuyStar1()
    {
        BuyContinue(lose1Popup);
    }

    private void HandleBuyStar2()
    {
        BuyContinue(lose2Popup);
    }

    private void BuyContinue(GameObject popup)
    {
        if (!TrySpendGold(StarCost))
            return;

        _controller.RestoreHeart(3);
        _uiManager.JumpOutAnimation(popup);
    }

    private void HandleAdHeart()
    {
        var data = GetPlayerData();
        data.Heart++;
        SavePlayerData();
    }

    private void HandleBuyHeart()
    {
        if (!TrySpendGold(HeartCost))
            return;

        var data = GetPlayerData();
        data.Heart++;
        SavePlayerData();
    }

    private void HandleRetry()
    {
        var data = GetPlayerData();
        if (data.Heart <= 0)
        {
            _uiManager.JumpInAnimation(lose4Popup);
            return; 
        }

        data.Heart--;
        SavePlayerData();
        _uiManager.JumpOutAnimation(lose3Popup, () => GoToScene("GamePlay"));
    }

    private void HandleNextLevel()
    {
        GoToScene("Home");
    }

    private void HandleWatchAd()
    {
        watchAd.interactable = false;
        GrantWatchAdReward();
    }

    private void GrantWatchAdReward()
    {
        var data = GetPlayerData();
        data.Gold += watchAdGoldReward;
        SavePlayerData();

        win2Gold.text = data.Gold.ToString(); 
        watchAd.interactable = true;
        GoToScene("Home");
    }

    private void Update()
    {
        if (!_controller.IsBgInteractionBlocked())
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
            //OnTurnPopupOn?.Invoke("VICTORY");
            PlayWin1Animation();
        }
        else
        {
            while (_controller.GetFailAnimationNum() > 0)
                yield return null;
            //OnTurnPopupOn?.Invoke("DEFEAT");
            ShowLoseNotifcation();
        }
    }

    private void TurnPopUp(bool isWin)
    {
        StartCoroutine(WaitForAllArrowCoroutine(isWin));
    }

    private void GlowHitAnimation()
    {
        if (_isGlowing)
        {
            StopCoroutine(_glowing);
        }
        _glowing = StartCoroutine(PlayGlowHitAnimation());
    }

    private IEnumerator PlayGlowHitAnimation()
    {
        _isGlowing = true;
        Color glowHitColor = glowHit.color;
        float timer = 0f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            //Debug.Log(timer);
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
            glowHitColor.a = Mathf.Lerp(1f, 0f, timer / fadeOutDuration);
            glowHit.color = glowHitColor;
            yield return null;
        }

        glowHitColor.a = 0f;
        glowHit.color = glowHitColor;
        _isGlowing = false;
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

    private void HandlePlayZoneClicked(InputAction.CallbackContext context)
    {
        if (_isHolded)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
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

    //WIN HANDLE
    private void ArrowDecorationInit()
    {
        _arrowPos.Add(new Vector2(0, -arrowDecPos));
        _arrowPos.Add(new Vector2(arrowDecPos, arrowDecPos));
        _arrowPos.Add(new Vector2(0, -arrowDecPos));
        _arrowPos.Add(new Vector2(-arrowDecPos, arrowDecPos));

        _arrowPosOut.Add(new Vector2(-arrowDecPosOut, -arrowDecPos - arrowDecPosOut));
        _arrowPosOut.Add(new Vector2(arrowDecPos - arrowDecPosOut, arrowDecPos - arrowDecPosOut));
        _arrowPosOut.Add(new Vector2(arrowDecPosOut, -arrowDecPos - arrowDecPosOut));
        _arrowPosOut.Add(new Vector2(-arrowDecPos + arrowDecPosOut, arrowDecPos - arrowDecPosOut));
    }

    private void LoadLevelData()
    {
        var levelData = _controller.GetCurrentLevelIndex();
        win1Level.text = "Level " + levelData.ToString();
    }

    private void PlayWin1Animation()
    {
        RestoreHeartOnWin();
        win1Panel.SetActive(true);
        StartCoroutine(PlayWin1Sequence());
    }

    private IEnumerator PlayWin1Sequence()
    {
        _uiManager.JumpInAnimation(dataLabel);
        for (int i = 0; i < arrows.Length; i++)
            _uiManager.MoveInAnimation(arrows[i], _arrowPos[i]);

        yield return new WaitForSeconds(barsAnimation); 

        for (int i = 0; i < arrows.Length; i++)
            _uiManager.MoveOutAnimation(arrows[i], _arrowPosOut[i]); 

        //yield return new WaitForSeconds(barsAnimation);

        _uiManager.JumpOutAnimation(dataLabel, () => PlayWin2Animation());
    }

    private void PlayWin2Animation()
    {
        var levelData = _controller.GetCurrentLevelIndex();
        win2Level.text = "Level " + levelData.ToString();
        win2Gold.text = _controller.GetPlayerData().Gold.ToString();
        win1Panel.SetActive(false);
        win2Panel.SetActive(true);
        StartCoroutine (PlayWin2Sequence());
    }

    private IEnumerator PlayWin2Sequence()
    {
        for (int i = 0; i < _controller.GetHeart(); i++)
            _uiManager.JumpInAnimation(stars[i]);
        yield return new WaitForSeconds(barsAnimation);
        for (int i = 0; i < rewards.Length; i++)
            _uiManager.JumpInAnimation(rewards[i]);
    }


    //LOSE HANDLE
    private void ShowLoseNotifcation()
    {
        _uiManager.JumpInAnimation(lose1Popup);
    }

    //HELPER
    private PlayerData GetPlayerData()
    {
        return _controller.GetPlayerData();
    }

    private void SavePlayerData()
    {
        Locator.Get<IStorage>().Save("PlayerData", GetPlayerData());
    }

    private bool TrySpendGold(int amount)
    {
        var data = GetPlayerData();
        if (data.Gold < amount)
            return false;

        data.Gold -= amount;
        SavePlayerData();
        return true;
    }

    private void GoToScene(string sceneName)
    {
        DG.Tweening.DOTween.KillAll();
        Locator.Get<ISoundManager>().StopMusic();
        TransitionScene.NextSceneOverride = sceneName;
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Transition");
    }

    private void SpendHeartOnEnter()
    {
        var data = GetPlayerData();
        data.Heart = Mathf.Max(data.Heart - 1, 0);
        SavePlayerData();
    }

    private void RestoreHeartOnWin()
    {
        var data = GetPlayerData();
        data.Heart++;
        SavePlayerData();
    }

}

