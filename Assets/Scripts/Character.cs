using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Character : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("If true, character rotates to face movement direction")]
    [SerializeField] private bool faceMovementDirection = true;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private PlayerInput playerInput;
    private InputAction moveAction;

    [SerializeField] public Item itemEquipped;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        playerInput = GetComponent<PlayerInput>();

        // Get the action from this player's action map
        //playerInput.SwitchCurrentActionMap(playerInput.defaultActionMap);
        moveAction = playerInput.actions["Move"];

    }

    void Update()
    {
        // Get input (WASD / Arrow keys by default)
        //float horizontal = Input.GetAxisRaw("Horizontal");
        //float vertical = Input.GetAxisRaw("Vertical");

        movementInput = moveAction.ReadValue<Vector2>();

        UseItem();
        //movementInput = new Vector2(movementInput.x, movementInput.y).normalized;
    }

    void FixedUpdate()
    {
        // Move character
        rb.MovePosition(rb.position + movementInput * moveSpeed * Time.fixedDeltaTime);

        // Rotate to face movement direction
        if (faceMovementDirection && movementInput.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg;
            rb.rotation = angle - 90f; // adjust offset if sprite faces up/down by default
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }

    public void UseItem()
    {
        if(itemEquipped == null)
        {
            return;
        }
        itemEquipped.Use();
    }
}
