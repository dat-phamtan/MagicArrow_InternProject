using Assets.Scripts.Boosters;
using Assets.Scripts.UI;
using Assets.Scripts.Ultility;
using Assets.Scripts.Utility;
using System;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraModifier : MonoBehaviour
{
    public Camera mainCamera;
    public float padding = 1.5f;
    public float gridHeightRatio = 0.7f;
    public float minCameraSize = 5f;
    public float maxCameraSize = 20f;
    public float panSpeed = 0.1f;

    private IEventHandler _eventHandler;
    private IBoostersManager _boosterManager;
    private int _width;
    private int _height;
    private float _spacing;
    private float _xSafe;
    private float _ySafe;
    private bool _isPanning = false;


    private void Update()
    {
        if (!_isPanning)
            CenterTheCamera();
    }

    public void Init(IEventHandler eventHandler, int width, int height, float spacing)
    {
        _width = width;
        _height = height;
        _spacing = spacing;
        _eventHandler = eventHandler;
        _boosterManager = Locator.Get<IBoostersManager>();
        //_boosterManager.OnBoosterBusyChanged +=
        eventHandler.OnDisableCameraCenter += HandleDisable;
        CalaculateSafeZone();
    }

    private void HandleDisable()
    {
        _isPanning = false;
    }

    private void CenterTheCamera()
    {
        var camPos = mainCamera.transform.position;
        if (IsOutsideXZone())
        {    
            var delta = _xSafe - camPos.x;
            var moveDirection = Time.deltaTime * new Vector3(delta, 0f) * 10f;
            mainCamera.transform.Translate(moveDirection);
        }

        if (IsOutsideYZone())
        {
            var delta = _ySafe - camPos.y;
            var moveDirection = Time.deltaTime * new Vector3(0f, delta) * 10f;
            mainCamera.transform.Translate(moveDirection);
        }
    }

    private void ClampCameraToSafeZone()
    {
        var camPos = mainCamera.transform.position;
        camPos.x = Mathf.Clamp(camPos.x, -_xSafe, _xSafe);
        camPos.y = Mathf.Clamp(camPos.y, -_ySafe, _ySafe);
        mainCamera.transform.position = camPos;
    }


    private void CalaculateSafeZone()
    {
        _xSafe = (_width - 1) * _spacing / 2f;
        _ySafe = (_height - 1) * _spacing/ 2f;
    }

    private bool IsOutsideXZone()
    {
        var camPos = mainCamera.transform.position;
        if (camPos.x < -_xSafe || camPos.x > _xSafe)
            return true;
        return false;
    }

    private bool IsOutsideYZone()
    {
        var camPos = mainCamera.transform.position;
        if (camPos.y < -_ySafe || camPos.y > _ySafe)
            return true;
        return false;
    }

    public void TranslateCamera(Vector3 moveDir)
    {
        _isPanning = true;
        mainCamera.transform.Translate(mainCamera.orthographicSize * panSpeed * moveDir);
    }

    public void ZoomCamera(float zoomAmount)
    {
        //Debug.Log(ratioma
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize + zoomAmount, minCameraSize, maxCameraSize);
    }

    public void FitCamera()
    {
        float targetWidth = (_width + 1) * _spacing + padding;
        float targetHeight = (_height + 1) * _spacing + padding;

        float ratio = (float)Screen.width / Screen.height;
        float adjustedHeight = targetHeight / gridHeightRatio;

        float sizeY = adjustedHeight / 2f;
        float sizeX = (targetWidth / 2f) / ratio;
        //float sizeX = 0;
        mainCamera.orthographicSize = Mathf.Max(sizeY, sizeX);
    }
}
