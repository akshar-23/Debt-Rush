using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static HUD;

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

    public Transform weaponHolder;

    [Header("Inventory")]
    private List<ShopItem> inventory = new List<ShopItem>();
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
    [Header("References")]
    public HUD hudref;

    // Cursor reference for input forwarding
    public Cursor activeCursor;


    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInput>();

        canPlayerMove = true;
        canPlayerAct = true;

        InventoryInit();
    }

    private void Start()
    {
    }

    void Update()
    {
        if (canPlayerMove)
        {
            Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
            controller.Move(movement * moveSpeed * Time.deltaTime);

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
        moveInput = ctx.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        // Forward to cursor if active, otherwise use for player rotation
        if (activeCursor != null)
        {
            activeCursor.OnLook(ctx);
        }
        else
        {
            directionInput = ctx.ReadValue<Vector2>();
        }
    }
    
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            // Forward to cursor if active
            if (activeCursor != null)
            {
                activeCursor.OnInteract(ctx);
            }
            else
            {
                OnInteract();
            }
        }
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (ctx.started && activeCursor != null)
        {
            activeCursor.OnCancel(ctx);
        }
    }

    public void OnChangeInventory_L(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            MoveLeft();
        }
    }

    public void OnChangeInventory_R(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            MoveRight();
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
        }

        itemEquipped.Execute();
    }

    public void MoveRight()
    {
        if (inventory.Count == 0) return;
        int oldInventoryPos = inventoryPos;

        inventory[inventoryPos].gameObject.SetActive(false);
        inventory[inventoryPos].isActiveItem = false;

        for (int i = inventoryPos; i < inventory.Count; i++)
        {
            inventoryPos = (inventoryPos + 1) % inventory.Count;

            if(inventoryPos == 0)
            {
                i = inventoryPos;
            }
            // Skip passive items
            if (inventory[inventoryPos].isPassiveItem)
                continue;

            // Found a non-passive item
            itemEquipped = inventory[inventoryPos];
            hudref.SetStateToIndex(playerNumber, oldInventoryPos, InventoryButtonStates.Normal);
            hudref.SetStateToIndex(playerNumber, inventoryPos, InventoryButtonStates.Selected);
            itemEquipped.gameObject.SetActive(true);
            itemEquipped.isActiveItem = true;

            return;
        }
    }

    public void MoveLeft()
    {
        if (inventory.Count == 0) return;
        int oldInventoryPos = inventoryPos;

        inventory[inventoryPos].gameObject.SetActive(false);
        inventory[inventoryPos].isActiveItem = false;
        
        for (int i = inventoryPos; i < inventory.Count; i--)
        {
            inventoryPos = (inventoryPos - 1 + inventory.Count) % inventory.Count;

            if (inventoryPos == inventory.Count)
            {
                i = inventoryPos;
            }
            // Skip passive items
            if (inventory[inventoryPos].isPassiveItem)
                continue;

            // Found a non-passive item
            itemEquipped = inventory[inventoryPos];
            hudref.SetStateToIndex(playerNumber, oldInventoryPos, InventoryButtonStates.Normal);
            hudref.SetStateToIndex(playerNumber, inventoryPos, InventoryButtonStates.Selected);
            itemEquipped.gameObject.SetActive(true);
            itemEquipped.isActiveItem = true;

            return;
        }
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
        activeCursor = null;
    }

    public void CopyInventory(List<ShopItem> itemsList)
    {
        inventory.Clear();

        foreach (var item in itemsList)
        {
            if (item == null) continue;
            
            GameObject itemObj = Instantiate(item.gameObject, weaponHolder);
            ShopItem newItem = itemObj.GetComponent<ShopItem>();
            
            if (newItem.maxCount > 0)
            {
                newItem.currentCount = newItem.maxCount;
            }
            
            newItem.isActiveItem = false;
            inventory.Add(newItem);
            newItem.gameObject.SetActive(false);

            // Passive Item should be executed only once.
            if(newItem.isPassiveItem)
            {
                newItem.Init(playerNumber);
                newItem.Execute();
                newItem.gameObject.SetActive(true);
            }
        }
        
        InventoryInit();
    }

    public void EquipItemInTheModel(GameObject objectPrefab, Vector3 addPosition)
    {
        objectPrefab.transform.SetParent(transform);

        objectPrefab.transform.localPosition = Vector3.zero + addPosition;
        objectPrefab.transform.localRotation = Quaternion.identity;
        objectPrefab.transform.localScale = Vector3.one;
    }

    private void InventoryInit()
    {
        if (inventory.Count != 0)
        {
            SelectFirstItem();
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
            SelectFirstItem();
        }
    }

    public void SelectFirstItem()
    {
        for(int i = 0; i < inventory.Count; i++)
        {
            ShopItem item = inventory[i];

            if (item.isPassiveItem) { continue; }

            itemEquipped = item;
            hudref.SetStateToIndex(playerNumber, i, InventoryButtonStates.Selected);
            inventoryPos = i;
            itemEquipped = inventory[inventoryPos];
            itemEquipped.gameObject.SetActive(true);
            itemEquipped.isActiveItem = true;
            break;
        }
    }
}
