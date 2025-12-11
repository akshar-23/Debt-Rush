using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;

public class Cursor : MonoBehaviour
{
    public Sprite sprite;
    public GameObject BombPrefab;

    //Hardcoded for player 2 for now
    public int playerNumber = 2;

    // setting these automatically in code
    private string horizontalInputAxis;
    private string verticalInputAxis;
    private string interactButton;

    public float moveSpeed = 10f;
    public float height = 10f;

    private CharacterController controller;
    public bool canCursorMove;
    private Vector3 moveDirection;

    public PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        controller = GetComponent<CharacterController>();

        horizontalInputAxis = "Horizontal_P" + playerNumber;
        verticalInputAxis = "Vertical_P" + playerNumber;
        interactButton = "Interact_P" + playerNumber;

        canCursorMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (canCursorMove)
        {
            float moveX = Input.GetAxisRaw(horizontalInputAxis);
            float moveZ = Input.GetAxisRaw(verticalInputAxis);

            moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

        }

        if (Input.GetButtonUp(interactButton))
        {
            OnInteract();
        }
    }

    private void OnInteract()
    {
        canCursorMove = false;
        if (BombPrefab != null)
        {
            GameObject bomb = Instantiate(BombPrefab, new Vector3(transform.position.x, transform.position.y + height, transform.position.z), transform.rotation);
            bomb.GetComponent<Bomb>().playerId = player.playerNumber;
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (player != null)
        {
            player.OnSpawnedObjectDestroyed();
        }
    }
}
