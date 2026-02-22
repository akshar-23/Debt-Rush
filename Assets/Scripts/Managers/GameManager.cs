using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject gameOverUI;
    private bool isGameOver = false;
    public bool checkConditions = false;

    public Character[] players = new Character[2];
    public Character[] enemies = new Character[40];

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

    public void ResetState()
    {
        isGameOver = false;
        checkConditions = false;
        if (gameOverUI != null) gameOverUI.gameObject.SetActive(false);
        ClearInventories();
        players = new Character[2];
        enemies = new Character[40];
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
        if (playerIndex == 1)
        {
            if (inventoryP1.Count >= 5)
            {
                return;
            }
        }
        else if (playerIndex == 2)
        {
            if (inventoryP2.Count >= 5)
            {
                return;
            }
        }

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
        int totalActivePlayers = 0;
        int deadPlayers = 0;
        int playersAtDestination = 0;

        foreach (var player in players)
        {
            if (player == null) continue;

            totalActivePlayers++;

            if (player.isDead)
            {
                deadPlayers++;
            }
            else
            {
                var controller = player.GetComponent<PlayerController>();
                if (controller != null && controller.isAtDestination)
                {
                    playersAtDestination++;
                }
            }
        }

        if (totalActivePlayers == 0) return;

        if (deadPlayers >= totalActivePlayers)
        {
            ShowGameOverScreen("GAME OVER");
            return;
        }

        int aliveCount = totalActivePlayers - deadPlayers;

        if (playersAtDestination >= aliveCount && aliveCount > 0)
        {
            var mm = MoneyManager.Instance;
            if (mm.GetMoneyAmount() >= mm.targetMoney)
            {
                mm.SubtractMoney(mm.targetMoney);
                ShowGameOverScreen("YOU WON");
            }
        }
    }
}
