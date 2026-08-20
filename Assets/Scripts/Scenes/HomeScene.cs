using Assets.Scripts.CoreLogic;
using Assets.Scripts.Data;
using Assets.Scripts.Utility;
using TMPro;
using UnityEngine;

public class HomeScene : MonoBehaviour
{
    public TextMeshProUGUI coinValue;
    public TextMeshProUGUI heartValue;
    public TextMeshProUGUI starValue;
    public TextMeshProUGUI heartRegenTime;
    public GameObject currentLevel;

    public Transform contentTransform;
    public GameObject levelItemPrefab;


    public float levelSpacing = 50f;

    private int _numLevel;
    private InputSystem_Actions _inputActions;
    private IController _controller;
    private PlayerData _playerData;


    public void Awake()
    {
        _inputActions = new InputSystem_Actions();
    }

    public void OnEnable()
    {
        _inputActions.Enable();
    }

    public void OnDisable()
    {
        _inputActions.Disable();
    }

    public void Start()
    {
        _controller = Locator.Get<IController>();
        _playerData = _controller.GetPlayerData();
        _numLevel = _playerData.CurrentLevelsData.Length;

        coinValue.text = _playerData.Gold.ToString();
        heartValue.text = _playerData.Heart.ToString();
        starValue.text = _playerData.Star.ToString();
        heartRegenTime.text = _playerData.RegenHour.ToString() + ":" + _playerData.RegenMinute.ToString();

        GenerateLevelList();

    }


    public void Update()
    {

    }

    private void HandlePan()
    {

    }

    void GenerateLevelList()
    {
        for (int i = 0; i < _numLevel; i++)
        {
            GameObject newLevelItem = Instantiate(levelItemPrefab, contentTransform);
            newLevelItem.name = "Level_" + (i + 1);
        }
    }
}
