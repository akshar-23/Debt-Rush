using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject gameOverUI;
    private bool isGameOver;
    public bool checkConditions = false;

    public Character[] players;
    public Character[] enemies;

    [Header("Shop Data (edit in Inspector)")]
    [SerializeField] private List<ShopItem> shopItemsP1 = new();
    [SerializeField] private List<ShopItem> shopItemsP2 = new();

    [SerializeField, HideInInspector] private List<ShopItem> inventoryP1 = new();
    [SerializeField, HideInInspector] private List<ShopItem> inventoryP2 = new();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (gameOverUI != null) gameOverUI.gameObject.SetActive(false);
        Time.timeScale = 1f;
        isGameOver = false;
    }

    void Update()
    {
        if (isGameOver) return;

        if (checkConditions)
        {
            CheckWinLossConditions();
        }
    }

    /// Read-only view    
    public IReadOnlyList<ShopItem> GetShop(int playerIndex)
        => (playerIndex == 1) ? (IReadOnlyList<ShopItem>)shopItemsP1 : shopItemsP2;

    public IReadOnlyList<ShopItem> GetInventory(int playerIndex)
        => (playerIndex == 1) ? (IReadOnlyList<ShopItem>)inventoryP1 : inventoryP2;

    public void AddToInventory(int playerIndex, ShopItem item)
    {
        (playerIndex == 1 ? inventoryP1 : inventoryP2).Add(item);
    }

    public bool TryRemoveFromInventory(int playerIndex, string name, out ShopItem removed)
    {
        var list = (playerIndex == 1) ? inventoryP1 : inventoryP2;
        int i = list.FindIndex(x => x.itemName == name);
        if (i >= 0)
        {
            removed = list[i];
            list.RemoveAt(i);
            return true;
        }
        removed = default;
        return false;
    }

    public void ClearInventories()
    {
        inventoryP1.Clear();
        inventoryP2.Clear();
    }


    public void ShowGameOverScreen(string finaltext)
    {
        isGameOver = true;
        if (gameOverUI != null)
        {
            gameOverUI.gameObject.SetActive(true);
            gameOverUI.GetComponent<GameOverScreen>().SetGameOverText(finaltext);
        }
    }

    public void CheckWinLossConditions()
    {
        //If Players are both null (dead), or timer reach 0
        if (players[0].isDead && players[1].isDead)
        {
            ShowGameOverScreen("GAME OVER");
        }
        else if (
            !players[0].isDead && players[0].GetComponent<PlayerController>().isAtDestination &&
            !players[1].isDead && players[1].GetComponent<PlayerController>().isAtDestination &&
            MoneyManager.Instance.GetMoneyAmount() >= MoneyManager.Instance.targetMoney
        )
        {
            MoneyManager.Instance.SubtractMoney(MoneyManager.Instance.targetMoney);
            ShowGameOverScreen("YOU WON");
        }
    }
}
