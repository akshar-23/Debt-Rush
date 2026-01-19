using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using static GameManager;

[RequireComponent(typeof(CharacterController))]
public class PlayerMoveController : Character
{
    [Header("Player Settings")]
    [Tooltip("The unique number for this player (1, 2, 3, etc.).")]
    public int playerNumber = 1;

    [Header("Movement Settings")]
    [Tooltip("The speed at which the player moves.")]
    public float moveSpeed = 7f;
    [Tooltip("The speed at which the player rotates to face the movement direction.")]
    public float rotationSpeed = 10f;

    public Transform weaponHolder;

    [Header("Inventory")]
    [SerializeField] private List<ShopItem> inventory = new List<ShopItem>();
    private int inventoryPos = -1;

    public PlayerInput input;
    private Vector2 moveInput;
    private Vector2 directionInput;

    public bool isAtDestination = false;

    private CharacterController controller;
    public Vector3 moveDirection;
    [SerializeField] private bool canPlayerMove;
    [SerializeField] private bool canPlayerAct;

    [Header("Items")]
    [SerializeField]
    public ShopItem itemEquipped;

    [Header("Objective Bools")]
    [SerializeField]
    public int killCount = 0;
    public int lastMultiKillCount = 0;

    [Space]
    [SerializeField]
    public int hiddenStash = 0;

    [Space]
    [Header("I don't like this, not proud of it!")]
    public HUD hudref;


    protected override void Awake()
    {
        base.Awake();
        //controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();

        canPlayerMove = true;
        canPlayerAct = true;

        InventoryInit();
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
            if (item.itemName.Equals("Shield"))
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
        if (Gamepad.current != null)
        {
            Debug.Log("Player " + playerNumber + " " + Gamepad.current.leftStick.ReadValue());
            Debug.Log("Gamepads detected: " + Gamepad.all.Count);
        }
        else
        {
            //Debug.Log("No gamepad detected.");
        }

        if (canPlayerMove)
        {
            Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
            transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

            Vector3 direction = new Vector3(directionInput.x, 0, directionInput.y);
            moveDirection = direction.normalized;

            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        Debug.Log("Move! Player: " + playerNumber + " " + ctx.phase);
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        Debug.Log("Rotate! Player: " + playerNumber + " " + ctx.phase);
        directionInput = ctx.ReadValue<Vector2>();
    }
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        Debug.Log("Interact! Player: " + playerNumber + " " +  ctx.phase);
        OnInteract();
    }

    public void OnChangeInventory_L(InputAction.CallbackContext ctx)
    {
        ChangeInventoryPosition(true);
    }

    public void OnChangeInventory_R(InputAction.CallbackContext ctx)
    {
        ChangeInventoryPosition(false);
    }

    private void ChangeInventoryPosition(bool isLeft)
    {
        if (inventory.Count == 0)
        {
            return;
        }
        else if (inventoryPos == 0)
        {
            inventoryPos = inventory.Count;
            itemEquipped = inventory[inventoryPos];//. FininventoryPos];
        }
    }

    private void OnInteract()
    {
        if (itemEquipped == null || !canPlayerAct)
        {
            return;
        }

        if (itemEquipped.CompareTag("Target"))
        {
            canPlayerMove = false;
            //itemEquipped.GetComponent<CellphoneService>();

            //GameObject cursor = Instantiate(itemEquipped, spawnPos, new Quaternion(90, 0, 0, 90));

            //cursor.GetComponent<Cursor>().BombPrefab = itemAuxPrefab;
            //cursor.GetComponent<Cursor>().player = this;
        }

        itemEquipped.Execute();



    }

    public void SetCanPlayerMove(bool _canPlayerMove)
    {
        canPlayerMove = _canPlayerMove;
    }

    public void SetCanPlayerAct(bool _canPlayerAct)
    {
        canPlayerAct = _canPlayerAct;
    }

    public void OnSpawnedObjectDestroyed()
    {
        canPlayerMove = true;
        canPlayerAct = true;
        Debug.Log("My spawned object died.");
    }

    public void CopyInventory(List<ShopItem> itemsList)
    {
        foreach (var item in itemsList)
        {
            GameObject itemObj = Instantiate(item.gameObject, weaponHolder);
            ShopItem newItem = itemObj.GetComponent<ShopItem>();
            newItem.isActiveItem = false;
            inventory.Add(newItem);
            newItem.gameObject.SetActive(false);
        }
    }

    private void InventoryInit()
    {
        if (inventory.Count != 0)
        {
            inventoryPos = 0;
            itemEquipped = inventory[inventoryPos];
            itemEquipped.gameObject.SetActive(true);
            itemEquipped.isActiveItem = true;
        }
    }

    public void DeleteInventoryItem(ShopItem _item)
    {
        Destroy(_item.gameObject);
        inventory.Remove(_item);
        GameManager.Instance.TryRemoveFromInventory(playerNumber, _item.itemName, out ShopItem removed);
        hudref.BuildUI();
        
        if (inventory.Count == 0)
        {
            inventoryPos = -1;
            itemEquipped = null;
        }
        else
        {
            inventoryPos = 0;
            itemEquipped = inventory[inventoryPos];
            itemEquipped.gameObject.SetActive(true);
            itemEquipped.isActiveItem = true;
        }
    }
}