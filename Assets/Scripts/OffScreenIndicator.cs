using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class OffScreenIndicator : MonoBehaviour
{
    public Transform target;
    public Camera playerCamera;
    public CameraManager camManager;
    public float margin = 50f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void LateUpdate()
    {
        if (target == null || camManager == null || playerCamera == null)
        {
            SetVisibility(false);
            return;
        }

        if (camManager.currentState == CameraManager.CameraState.Single)
        {
            SetVisibility(false);
            return;
        }

        Vector3 screenPos = playerCamera.WorldToScreenPoint(target.position);

        if (screenPos.z < 0) screenPos *= -1;

        Rect vRect = playerCamera.rect;
        float minX = vRect.x * Screen.width + margin;
        float maxX = (vRect.x + vRect.width) * Screen.width - margin;
        float minY = vRect.y * Screen.height + margin;
        float maxY = (vRect.y + vRect.height) * Screen.height - margin;

        bool isOffScreen = screenPos.x <= minX || screenPos.x >= maxX ||
                           screenPos.y <= minY || screenPos.y >= maxY;

        if (isOffScreen)
        {
            SetVisibility(true);

            screenPos.x = Mathf.Clamp(screenPos.x, minX, maxX);
            screenPos.y = Mathf.Clamp(screenPos.y, minY, maxY);
            rectTransform.position = screenPos;

            Vector3 viewportCenter = new Vector3(
                (vRect.x + vRect.width / 2f) * Screen.width,
                (vRect.y + vRect.height / 2f) * Screen.height,
                0
            );

            Vector3 direction = screenPos - viewportCenter;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            rectTransform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            SetVisibility(false);
        }
    }

    private void SetVisibility(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
    }
}