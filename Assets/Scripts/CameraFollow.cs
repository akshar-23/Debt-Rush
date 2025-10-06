using UnityEngine;

public class FixedRotationCameraFollow : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("The first player's transform.")]
    [SerializeField] private Transform player1;
    [Tooltip("The second player's transform.")]
    [SerializeField] private Transform player2;

    [Header("Framing & Position")]
    [SerializeField] private Vector3 offset = new Vector3(0, 15, -10);
    [SerializeField] private float padding = 2f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 0.25f;

    private Camera cam;
    private Vector3 velocity;

    private Quaternion initialRotation;

    void Start()
    {
        cam = GetComponent<Camera>();
        
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        Vector3 centerPoint;
        float requiredDistance;

        if (player1 != null && player2 != null)
        {
            centerPoint = (player1.position + player2.position) / 2f;
            requiredDistance = CalculateRequiredDistance();
        }
        else if (player1 != null)
        {
            centerPoint = player1.position;
            requiredDistance = offset.magnitude;
        }
        else if (player2 != null)
        {
            centerPoint = player2.position;
            requiredDistance = offset.magnitude;
        }
        else
        {
            return;
        }
        
        Vector3 desiredPosition = centerPoint + offset.normalized * requiredDistance;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        transform.rotation = initialRotation;
    }

    private float CalculateRequiredDistance()
    {
        float defaultDistance = offset.magnitude;

        Bounds bounds = new Bounds(player1.position, Vector3.zero);
        bounds.Encapsulate(player2.position);
        bounds.Expand(padding);

        float maxBoundsSize = Mathf.Max(bounds.size.x, bounds.size.y);

        float distanceToFit = (maxBoundsSize / 2f) / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

        return Mathf.Max(distanceToFit, defaultDistance);
    }
}