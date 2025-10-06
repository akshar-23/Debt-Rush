using UnityEngine;

/// <summary>
/// A 3D Perspective camera that follows two targets with a FIXED rotation.
/// The camera's rotation must be set manually in the editor.
/// This script only controls the camera's position and distance (zoom).
/// </summary>
public class FixedRotationCameraFollow : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("The first player's transform.")]
    [SerializeField] private Transform player1;
    [Tooltip("The second player's transform.")]
    [SerializeField] private Transform player2;

    [Header("Framing & Position")]
    [Tooltip("The camera's position relative to the players' midpoint. The rotation is NOT affected by this.")]
    [SerializeField] private Vector3 offset = new Vector3(0, 15, -10);
    [Tooltip("The amount of space to leave around the players.")]
    [SerializeField] private float padding = 2f;

    [Header("Smoothing")]
    [Tooltip("How quickly the camera moves to its target position. Lower is slower.")]
    [SerializeField] private float smoothSpeed = 0.25f;

    // Private variables
    private Camera cam;
    private Vector3 velocity; // Used for SmoothDamp

    // --- FOR GUARANTEED FIXED ROTATION ---
    private Quaternion initialRotation;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        // Store the original rotation that you set in the editor
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (player1 == null || player2 == null) return;

        // 1. Calculate the center point of the players
        Vector3 centerPoint = (player1.position + player2.position) / 2f;

        // 2. Calculate the required distance to frame the players
        float requiredDistance = CalculateRequiredDistance();

        // 3. Determine the final desired position
        // This position is based on the center point and the offset, scaled by our zoom.
        Vector3 desiredPosition = centerPoint + offset.normalized * requiredDistance;

        // 4. Smoothly move the camera towards the desired position
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        // 5. ENFORCE THE FIXED ROTATION
        // This guarantees that no other script or physics can change the camera's rotation.
        transform.rotation = initialRotation;
    }

    private float CalculateRequiredDistance()
    {
        // Get the default distance from the offset vector's length
        float defaultDistance = offset.magnitude;

        // Create a bounds that encapsulates both players
        Bounds bounds = new Bounds(player1.position, Vector3.zero);
        bounds.Encapsulate(player2.position);
        bounds.Expand(padding);

        float maxBoundsSize = Mathf.Max(bounds.size.x, bounds.size.y);

        // Calculate the distance needed to frame the players perfectly
        float distanceToFit = (maxBoundsSize / 2f) / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        // Return the larger of the two distances:
        // Either the distance needed to fit the players, or our default distance.
        // This prevents the camera from zooming in too close.
        return Mathf.Max(distanceToFit, defaultDistance);
    }
}