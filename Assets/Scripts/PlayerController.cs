using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : Character
{
    [Header("Movement Settings")]
    [Tooltip("The speed at which the player moves.")]
    public float moveSpeed = 7f;
    [Tooltip("The speed at which the player rotates to face the movement direction.")]
    public float rotationSpeed = 10f;

    [Header("Input Axis Names")]
    [Tooltip("The name of the horizontal input axis from the Input Manager.")]
    public string horizontalInputAxis = "Horizontal";
    [Tooltip("The name of the vertical input axis from the Input Manager.")]
    public string verticalInputAxis = "Vertical";

    private CharacterController controller;
    private Vector3 moveDirection;

    [Header("Items")]

    [SerializeField]
    public Item itemEquipped;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw(horizontalInputAxis);
        float moveZ = Input.GetAxisRaw(verticalInputAxis);

        //Debug.Log($"Player: {gameObject.name}, Input: ({moveX}, {moveZ})");

        moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.E))
        {
            OnInteract();
        }
    }

    private void OnInteract()
    {
        if (itemEquipped == null)
        {
            return;
        }
        itemEquipped.Use();
    }
}