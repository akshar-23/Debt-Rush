using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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
    private Vector2 moveInput;

    public PlayerController player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //controller = GetComponent<CharacterController>();

        //horizontalInputAxis = "Horizontal_P" + playerNumber;
        //verticalInputAxis = "Vertical_P" + playerNumber;
        //interactButton = "Interact_P" + playerNumber;

        canCursorMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (canCursorMove)
        {
            Vector3 step = Vector3.zero;

            if (Input.GetKeyDown(KeyCode.UpArrow))
                step += Vector3.forward;

            if (Input.GetKeyDown(KeyCode.DownArrow))
                step += Vector3.back;

            if (Input.GetKeyDown(KeyCode.RightArrow))
                step += Vector3.right;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                step += Vector3.left;

            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                OnInteract();
            }

            transform.position += step * 1f;

            //float moveX = Input.GetAxisRaw(horizontalInputAxis);
            //float moveZ = Input.GetAxisRaw(verticalInputAxis);

            //moveDirection = new Vector3(moveX, 0f, moveZ).normalized;
            //controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            //Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
            //transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
        }

        //if (Input.GetKeyDown(interactButton))
        //{
        //   OnInteract();
        //}
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
