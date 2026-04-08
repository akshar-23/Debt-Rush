using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
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
    [Tooltip("How strong the gravity is.")]
    public float gravity = -9.81f;

    private float verticalVelocity;

    public Transform weaponHolder;

    [Header("Inventory")]
    [SerializeField] private List<ShopItem> inventory = new List<ShopItem>();
    [SerializeField] private int inventoryPos = -1;


    [Header("Player Input")]
    public PlayerInput input;
    private Vector2 moveInput;
    private Vector2 directionInput;

    [Header("Winning Condition")]
    // We should move this to the destinationPoint object.
    public bool isAtDestination = false;

    private CharacterController controller;
    public Vector3 moveDirection;
    [SerializeField] private bool canPlayerMove;
    [SerializeField] private bool canPlayerAct;
    private bool interactPressed = false;
    private bool submitPressed = false;

    [Header("Items")]
    [SerializeField]
    public ShopItem itemEquipped;

    [Header("Objective Bools")]
    [SerializeField]
    public int killCount = 0;
    public int lastMultiKillCount = 0;
    public bool reachedDestinationFirst = false;
    public int chestCount = 0;
    public int deadCount = 0;
    public bool hasOnly5Kills = false;
    public bool hasNeverOpenedChest = false;
    public bool hasOnly3Deaths = false;

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
        if (GameManager.Instance != null)
        {
            hiddenStash = GameManager.Instance.playerStash[playerNumber - 1];
        }
        else
        {
            hiddenStash = 0;
        }

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
            Vector3 velocity = movement * moveSpeed;
            
            if (!controller.isGrounded)
            {
                verticalVelocity = -0.5f;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }

            velocity.y = verticalVelocity;

            controller.Move(velocity * Time.deltaTime);

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
            interactPressed = true;
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
        else if (ctx.canceled)
        {
            interactPressed = false;
        }
    }

    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (ctx.started)
        {
            submitPressed = true;
            //OnSubmit();
        }
        else if (ctx.canceled)
        {
            submitPressed = false;
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

    public bool GetInteractPressed()
    {
        bool result = interactPressed;
        interactPressed = false;
        return result;
    }

    public bool GetSubmitPressed()
    {
        bool result = submitPressed;
        submitPressed = false;
        return result;
    }

    public void RegisterInteractPressed()
    {
        interactPressed = false;
    }

    public void RegisterSubmitPressed()
    {
        submitPressed = false;
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
            if (!inventory[oldInventoryPos].isPassiveItem)
            {
                hudref.SetStateToIndex(playerNumber, oldInventoryPos, InventoryButtonStates.Normal);
            }
            hudref.SetStateToIndex(playerNumber, inventoryPos, InventoryButtonStates.Selected);
            itemEquipped.gameObject.SetActive(true);
            itemEquipped.isActiveItem = true;

            return;
        }
    }

    public void UpdateHiddenStash(int amount)
    {
        hiddenStash += amount;
        
        if (hiddenStash < 0)
        {
            hiddenStash = 0;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.playerStash[playerNumber - 1] = hiddenStash;
        }
        
        Debug.Log($"Player {playerNumber} stash updated by {amount}. New total: {hiddenStash}");
    }

    public void CheckFinalObjectives()
    {
        if (killCount == 5)
        {
            hasOnly5Kills = true;
        }

        if (chestCount == 0)
        {
            hasNeverOpenedChest = true;
        }

        if (deadCount == 3)
        {
            hasOnly3Deaths = true;
        }
    }

    public void MoveLeft()
    {
        if (inventory.Count == 0) return;
        int oldInventoryPos = inventoryPos;

        inventory[inventoryPos].gameObject.SetActive(false);
        inventory[inventoryPos].isActiveItem = false;
        
        for (int i = 0; i < inventory.Count; i++)
        {
            inventoryPos = (inventoryPos - 1 + inventory.Count) % inventory.Count;

            // Skip passive items
            if (inventory[inventoryPos].isPassiveItem)
                continue;

            // Found a non-passive item
            itemEquipped = inventory[inventoryPos];
            if (!inventory[oldInventoryPos].isPassiveItem)
            {
                hudref.SetStateToIndex(playerNumber, oldInventoryPos, InventoryButtonStates.Normal);
            }
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

    public void BindUI(MultiplayerEventSystem eventSystem, Canvas canvas, GameObject firstButton)
    {
        var binder = GetComponent<PlayerUIBinder>();
        if (binder != null)
        {
            binder.eventSystem = eventSystem;
            binder.playerCanvas = canvas;
            binder.firstSelected = firstButton;

            binder.Bind();
        }
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

    public void AddInventoryItem(ShopItem _item)
    {
        if (_item.isPassiveItem)
        {
            int idx = inventory.FindLastIndex(i => i.isPassiveItem) + 1;
            inventory.Insert(idx, _item);
        }
        else
        {
            inventory.Add(_item);
        }
        GameManager.Instance.AddToInventory(playerNumber, _item);
        hudref.BuildUI();
        if (itemEquipped != null)
        {
            hudref.SetStateToIndex(playerNumber, inventoryPos, InventoryButtonStates.Selected);
        }

        int totalPassiveItems = 0;

        for(int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].isPassiveItem) { totalPassiveItems++; }
        }

        if (inventory.Count - 1 == totalPassiveItems)
        {
            SelectFirstItem();
        }
    }

    public void DeleteInventoryItem(ShopItem _item)
    {
        Destroy(_item.gameObject);
        inventory.Remove(_item);
        GameManager.Instance.TryRemoveFromInventory(playerNumber, _item, out ShopItem removed);
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

        if (itemEquipped == null) { inventoryPos = 0; }
    }

    public int GetPlayerNumber()
    {
        return playerNumber;
    }

    public int GetInventoryPos()
    {
        return inventoryPos;
    }
}
