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

    private int _width;
    private int _height;
    private float _spacing;
    private float _xSafe;
    private float _ySafe;


    private void Update()
    {
        CenterTheCamera();
    }

    public void Init(int width, int height, float spacing)
    {
        _width = width;
        _height = height;
        _spacing = spacing;
    }

    private void CenterTheCamera()
    {
        if (IsOutsideXZone())
        {
            var camPos = mainCamera.transform.position;
            var delta = camPos - new Vector3(_xSafe, _ySafe, 0f);
            var moveDirection = Time.deltaTime * delta;
            mainCamera.transform.Translate(-moveDirection);
        }
    }
    
    private void CalaculateSafeZone()
    {
        _xSafe = (_width - 1) * _spacing * 1.5f;
        _ySafe = (_height - 1) * _spacing * 1.5f;
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
