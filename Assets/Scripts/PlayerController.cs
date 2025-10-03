using UnityEngine;

// This line ensures that any GameObject with this script also has a CharacterController component.
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The speed at which the player moves.")]
    public float moveSpeed = 7f;

    [Header("Input Axis Names")]
    [Tooltip("The name of the horizontal input axis from the Input Manager.")]
    public string horizontalInputAxis = "Horizontal";
    [Tooltip("The name of the vertical input axis from the Input Manager.")]
    public string verticalInputAxis = "Vertical";

    private CharacterController controller;
    private Vector3 moveDirection;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw(horizontalInputAxis);
        float moveZ = Input.GetAxisRaw(verticalInputAxis);

        Debug.Log($"Player: {gameObject.name}, Input: ({moveX}, {moveZ})");

        moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);
    }
}
