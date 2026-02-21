using UnityEngine;

public class FloatingIcon : MonoBehaviour
{
    [Header("Floating Settings")]
    [SerializeField] private float floatAmount = 10f;   // How far it moves (in pixels)
    [SerializeField] private float floatSpeed = 2f;     // How fast it moves

    private RectTransform rectTransform;
    private Vector2 startPos;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        float newY = Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        rectTransform.anchoredPosition = startPos + new Vector2(0f, newY);
    }
}