using UnityEngine;

public class CameraModifier : MonoBehaviour
{
    public Camera mainCamera;
    public float padding = 1.5f;
    public float gridHeightRatio = 0.7f;

    void Start()
    {
        
    }

    public void FitCamera(int width, int height, float spacing)
    {
        //float gridWorldWidth = (width - 1) * spacing;
        //float gridWorldHeight = (height - 1) * spacing;

        //mainCamera.transform.position = new Vector3(gridWorldWidth / 2f, gridWorldHeight / 2f, -10);

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
