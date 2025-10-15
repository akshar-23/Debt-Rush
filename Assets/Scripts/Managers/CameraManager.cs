using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public enum CameraState
    {
        Single,
        Split
    }

    [Header("Targets")]
    [SerializeField] private Transform player1;
    [SerializeField] private Transform player2;

    [Header("Cameras")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera player1Camera;
    [SerializeField] private Camera player2Camera;

    [Header("Distance Thresholds")]
    [SerializeField] private float splitDistance = 15f;
    [SerializeField] private float mergeDistance = 12f;

    [Header("Single Camera Settings")]
    [SerializeField] private Vector3 singleCamOffset = new Vector3(0, 25, 0);
    [SerializeField] private float singleCamPadding = 2f;
    [SerializeField] private float singleCamSmoothSpeed = 0.125f;

    [Header("Split Camera Settings")]
    [SerializeField] private Vector3 splitCamOffset = new Vector3(0, 25, 0);
    [SerializeField] private float splitCamSmoothSpeed = 0.125f;

    private CameraState currentState;
    private Vector3 mainCamVelocity;
    private Vector3 p1CamVelocity;
    private Vector3 p2CamVelocity;

    private void OnEnable()
    {
        PlayerController.OnPlayerDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerDied -= HandlePlayerDeath;
    }

    void Start()
    {
        SwitchToSingleCamera();
    }

    void LateUpdate()
    {
        if (player1 != null && player2 != null)
        {
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
        else if (player1 != null)
        {
            FollowSinglePlayer(player1);
        }
        else if (player2 != null)
        {
            FollowSinglePlayer(player2);
        }
    }

    private void HandlePlayerDeath(Transform deadPlayerTransform)
    {
        if (deadPlayerTransform == player1)
        {
            player1 = null;
        }
        else if (deadPlayerTransform == player2)
        {
            player2 = null;
        }

        if (player1 != null || player2 != null)
        {
            SwitchToSingleCamera();
        }
    }

    private void FollowSinglePlayer(Transform survivor)
    {
        Vector3 desiredPosition = survivor.position + singleCamOffset;
        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, desiredPosition, ref mainCamVelocity, singleCamSmoothSpeed);
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
        Vector3 p1DesiredPos = player1.position + splitCamOffset;
        player1Camera.transform.position = Vector3.SmoothDamp(player1Camera.transform.position, p1DesiredPos, ref p1CamVelocity, splitCamSmoothSpeed);

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