using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public enum CameraState
    {
        Single,
        Split
    }

    [Header("Targets")]
    [Tooltip("The first player's transform.")]
    [SerializeField] private Transform player1;
    [Tooltip("The second player's transform.")]
    [SerializeField] private Transform player2;

    [Header("Cameras")]
    [Tooltip("The main camera used for the single, merged view.")]
    [SerializeField] private Camera mainCamera;
    [Tooltip("The camera that will follow Player 1 in split-screen.")]
    [SerializeField] private Camera player1Camera;
    [Tooltip("The camera that will follow Player 2 in split-screen.")]
    [SerializeField] private Camera player2Camera;

    [Header("Distance Thresholds")]
    [Tooltip("The distance at which the camera will split.")]
    [SerializeField] private float splitDistance = 15f;
    [Tooltip("The distance at which the camera will merge back together. Should be less than splitDistance.")]
    [SerializeField] private float mergeDistance = 12f;

    [Header("Single Camera Settings")]
    [SerializeField] private Vector3 singleCamOffset = new Vector3(0, 25, 0);
    [SerializeField] private float singleCamPadding = 2f;
    [SerializeField] private float singleCamSmoothSpeed = 0.125f;

    [Header("Split Camera Settings")]
    [Tooltip("The top-down offset for the individual player cameras.")]
    [SerializeField] private Vector3 splitCamOffset = new Vector3(0, 20, 0);
    [SerializeField] private float splitCamSmoothSpeed = 0.125f;

    private CameraState currentState;
    private Vector3 mainCamVelocity;
    private Vector3 p1CamVelocity;
    private Vector3 p2CamVelocity;

    void Start()
    {
        SwitchToSingleCamera();
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        float distance = Vector3.Distance(player1.position, player2.position);

        if (currentState == CameraState.Single && distance > splitDistance)
        {
            SwitchToSplitScreen();
        }
        else if (currentState == CameraState.Split && distance < mergeDistance)
        {
            SwitchToSingleCamera();
        }

        if (currentState == CameraState.Single)
        {
            HandleSingleCamera();
        }
        else
        {
            HandleSplitScreenCameras();
        }
    }

    private void SwitchToSingleCamera()
    {
        currentState = CameraState.Single;
        mainCamera.enabled = true;
        player1Camera.enabled = false;
        player2Camera.enabled = false;
    }

    private void SwitchToSplitScreen()
    {
        currentState = CameraState.Split;
        mainCamera.enabled = false;
        player1Camera.enabled = true;
        player2Camera.enabled = true;

        player1Camera.rect = new Rect(0, 0, 0.5f, 1);
        player2Camera.rect = new Rect(0.5f, 0, 0.5f, 1);
    }

    private void HandleSingleCamera()
    {
        Vector3 centerPoint = (player1.position + player2.position) / 2f;
        float requiredDistance = CalculateRequiredDistance();
        Vector3 desiredPosition = centerPoint + singleCamOffset.normalized * requiredDistance;

        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, desiredPosition, ref mainCamVelocity, singleCamSmoothSpeed);
    }

    private void HandleSplitScreenCameras()
    {
        // Player 1 Camera
        Vector3 p1DesiredPos = player1.position + splitCamOffset;
        player1Camera.transform.position = Vector3.SmoothDamp(player1Camera.transform.position, p1DesiredPos, ref p1CamVelocity, splitCamSmoothSpeed);

        // Player 2 Camera
        Vector3 p2DesiredPos = player2.position + splitCamOffset;
        player2Camera.transform.position = Vector3.SmoothDamp(player2Camera.transform.position, p2DesiredPos, ref p2CamVelocity, splitCamSmoothSpeed);
    }

    private float CalculateRequiredDistance()
    {
        float defaultDistance = singleCamOffset.magnitude;
        Bounds bounds = new Bounds(player1.position, Vector3.zero);
        bounds.Encapsulate(player2.position);
        bounds.Expand(singleCamPadding);

        float maxBoundsSize = Mathf.Max(bounds.size.x, bounds.size.z);
        float distanceToFit = (maxBoundsSize / 2f) / Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);

        return Mathf.Max(distanceToFit, defaultDistance);
    }
}