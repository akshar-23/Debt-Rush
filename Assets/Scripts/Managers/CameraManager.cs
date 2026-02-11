using UnityEngine;
using System; // Required for Action
using System.Collections; // Required for Coroutine (Fix 3)

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
    [SerializeField] public Camera player1Camera;
    [SerializeField] public Camera player2Camera;

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

    public CameraState currentState;
    private Vector3 mainCamVelocity;
    private Vector3 p1CamVelocity;
    private Vector3 p2CamVelocity;

    // --- FIX 3 VARS ---
    private float originalFOV; 
    private Coroutine fovCoroutine;

    private void OnEnable()
    {
        PlayerController.OnPlayerDied += HandlePlayerDeath;
        GameplaySceneManager.OnPlayerRespawn += HandlePlayerRespawn;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerDied -= HandlePlayerDeath;
        GameplaySceneManager.OnPlayerRespawn -= HandlePlayerRespawn;
    }

    void Start()
    {
        // --- FIX 3 SETUP ---
        // Store the original FOV so we can always return to it reliably
        if (mainCamera != null)
        {
            originalFOV = mainCamera.fieldOfView;
        }

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


    private void HandlePlayerDeath(int _id)
    {
        if (_id == 0) player1 = null;
        else if (_id == 1) player2 = null;

        if (player1 != null || player2 != null)
        {
            SwitchToSingleCamera();
        }
    }


    private void HandlePlayerRespawn(int _id, Transform playerTransform)
    {
        if (_id == 0)
        {
            player1 = playerTransform;
            Debug.Log("CameraManager re-acquired Player 1");
        }
        else if (_id == 1)
        {
            player2 = playerTransform;
            Debug.Log("CameraManager re-acquired Player 2");
        }
    }

    private void FollowSinglePlayer(Transform survivor)
    {
        if (currentState != CameraState.Single)
        {
            SwitchToSingleCamera();
        }

        Vector3 desiredPosition = survivor.position + singleCamOffset;
        mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, desiredPosition, ref mainCamVelocity, singleCamSmoothSpeed);
    }

    private void SwitchToSingleCamera()
    {
        currentState = CameraState.Single;

        // --- FIX 1 IMPLEMENTATION (Snap Position) ---
        // Before enabling the camera, put it exactly where it needs to be.
        if (player1 != null && player2 != null)
        {
            Vector3 centerPoint = (player1.position + player2.position) / 2f;
            
            // Reuse your existing logic to find the correct height/distance
            float requiredDistance = CalculateRequiredDistance();
            
            // Calculate the exact target position
            Vector3 targetPosition = centerPoint + singleCamOffset.normalized * requiredDistance;

            // SNAP the camera instantly
            mainCamera.transform.position = targetPosition;

            // CRITICAL: Reset velocity so SmoothDamp doesn't carry over old momentum
            mainCamVelocity = Vector3.zero;
        }

        mainCamera.enabled = true;
        player1Camera.enabled = false;
        player2Camera.enabled = false;

        // --- FIX 3 IMPLEMENTATION (FOV Ease-In) ---
        if (fovCoroutine != null) StopCoroutine(fovCoroutine);
        fovCoroutine = StartCoroutine(MergeEffect());
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

    // --- FIX 3 COROUTINE ---
    private IEnumerator MergeEffect()
    {
        float targetFOV = originalFOV;
        float startFOV = originalFOV * 1.2f; 
        
        mainCamera.fieldOfView = startFOV;

        float duration = 0.5f;
        float elapsed = 0f;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // Smoothly return to normal FOV
            float t = elapsed / duration;
            // Using SmoothStep for a nicer curve than linear Lerp
            t = t * t * (3f - 2f * t); 
            
            mainCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }
        
        mainCamera.fieldOfView = originalFOV;
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
        
        // Note: We use originalFOV here to ensure calculations stay stable even if FOV is animating
        float fovToUse = (originalFOV > 0) ? originalFOV : mainCamera.fieldOfView;

        float distanceToFit = (maxBoundsSize / 2f) / Mathf.Tan(fovToUse * 0.5f * Mathf.Deg2Rad);

        return Mathf.Max(distanceToFit, defaultDistance);
    }

    public void AssignTransformPosition(Transform position, int playerIndex)
    {
        if(playerIndex == 0)
        {
            player1 = position;            
        }
        if(playerIndex == 1)
        {
            player2 = position;
        }
    }
}