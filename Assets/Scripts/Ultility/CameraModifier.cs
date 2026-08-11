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

    public void TranslateCamera(Vector3 moveDir)
    {
        mainCamera.transform.Translate(mainCamera.orthographicSize * panSpeed * moveDir);
    }

    public void ZoomCamera(float zoomAmount)
    {
        //Debug.Log(ratioma
        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize + zoomAmount, minCameraSize, maxCameraSize);
    }

    public void FitCamera(int width, int height, float spacing)
    {
        float targetWidth = (width + 1) * spacing + padding;
        float targetHeight = (height + 1) * spacing + padding;

        float ratio = (float)Screen.width / Screen.height;
        float adjustedHeight = targetHeight / gridHeightRatio;

        float sizeY = adjustedHeight / 2f;
        float sizeX = (targetWidth / 2f) / ratio;
        //float sizeX = 0;
        mainCamera.orthographicSize = Mathf.Max(sizeY, sizeX);
    }
}
