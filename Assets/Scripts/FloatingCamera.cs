using UnityEngine;

public class FloatingCamera : MonoBehaviour
{
    [Header("Floating Settings")]
    public float floatAmountX = 0.2f;
    public float floatAmountZ = 0.2f;
    public float floatSpeed = 1f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float offsetX = Mathf.Sin(Time.time * floatSpeed) * floatAmountX;
        float offsetZ = Mathf.Cos(Time.time * floatSpeed) * floatAmountZ;

        transform.position = new Vector3(
            startPosition.x + offsetX,
            startPosition.y,
            startPosition.z + offsetZ
        );
    }
}
