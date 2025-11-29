using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using static GameManager;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : Character
{
    [Header("Player Settings")]
    [Tooltip("The unique number for this player (1, 2, 3, etc.).")]
    public int playerNumber = 1;

    [Header("Movement Settings")]
    [Tooltip("The speed at which the player moves.")]
    public float moveSpeed = 7f;
    [Tooltip("The speed at which the player rotates to face the movement direction.")]
    public float rotationSpeed = 10f;

    [Header("Inventory")]
    [SerializeField] private List<ShopItem> inventory = new();

    // setting these automatically in code
    private string horizontalInputAxis;
    private string verticalInputAxis;
    private string interactButton;

    public bool isAtDestination = false;

    private CharacterController controller;
    private Vector3 moveDirection;
    [SerializeField] private bool canPlayerMove;

    [Header("Items")]
    [SerializeField]
    public GameObject itemEquipped;
    public GameObject itemAuxPrefab;


    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<CharacterController>();

        horizontalInputAxis = "Horizontal_P" + playerNumber;
        verticalInputAxis = "Vertical_P" + playerNumber;
        interactButton = "Interact_P" + playerNumber;

        canPlayerMove = true;
    }

    private void Start()
    {
        Transform childTransform = this.transform.Find("Shield");
        if (childTransform != null)
        {
            GameObject childGameObject = childTransform.gameObject;
            childGameObject.SetActive(false);
        }

        foreach (var item in inventory)
        {
            if (item.Name.Equals("Shield"))
            {
                if (childTransform != null)
                {
                    GameObject childGameObject = childTransform.gameObject;
                    childGameObject.SetActive(true);
                    Debug.Log("Found child: " + childGameObject.name);
                }
            }
        }

    }

    void Update()
    {
        if (canPlayerMove)
        {
            float moveX = Input.GetAxisRaw(horizontalInputAxis);
            float moveZ = Input.GetAxisRaw(verticalInputAxis);

            moveDirection = new Vector3(moveX, 0f, moveZ).normalized;

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            if (Input.GetButtonDown(interactButton))
            {
                OnInteract();
            }
        }
    }

    private void OnInteract()
    {
        if (itemEquipped == null)
        {
            return;
        }
        Vector3 spawnPos = transform.position + transform.forward * 1f;

        if (itemEquipped.CompareTag("Target"))
        {
            canPlayerMove = false;
            //itemEquipped.GetComponent<CellphoneService>();

            GameObject cursor = Instantiate(itemEquipped, spawnPos, new Quaternion(90, 0, 0, 90));

            cursor.GetComponent<Cursor>().BombPrefab = itemAuxPrefab;
            cursor.GetComponent<Cursor>().player = this;
        }

        //Gun Logic
        MoneyManager.Instance.SubtractMoney(itemEquipped.GetComponent<Consumable>().cost);
        GameObject proj = Instantiate(itemEquipped, spawnPos, transform.rotation);
        Projectile p = proj.GetComponent<Projectile>();
        if (p != null)
            p.Init(transform.forward);
    }

    public void SetCanPlayerMove(bool _canPlayerMove)
    {
        canPlayerMove = _canPlayerMove;
    }

    public void OnSpawnedObjectDestroyed()
    {
        canPlayerMove = true;
        Debug.Log("My spawned object died.");
    }

    public void CopyInventory(List<ShopItem> itemsList)
    {
        inventory = itemsList;
    }
}