using Assets.Scripts.CoreLogic;
using Assets.Scripts.HeartManager;
using Assets.Scripts.Utility;
using UnityEngine;

public class HeartTicker : MonoBehaviour
{
    public float tickInterval = 1f;

    private IHeartManager _heartRegenManager;
    private float _timer;

    private void Awake()
    {
        _heartRegenManager = Locator.Get<IHeartManager>();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < tickInterval)
            return;

        _timer = 0f;
        _heartRegenManager.Tick();
    }
}