using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    public GameOverScreen gameOverUI;
    public static GameManager Instance { get; private set; }

    public Character[] players;
    public Character[] enemies;
    [SerializeField] List<string> inventoryP1 = new();
    [SerializeField] List<string> inventoryP2 = new();

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
        if (gameOverUI != null)
        {
            gameOverUI.gameObject.SetActive(false);
        }
        Time.timeScale = 1f;
    }


    /// Read-only view 
    public IReadOnlyList<string> GetInventory(int playerIndex)
        => (playerIndex == 1) ? (IReadOnlyList<string>)inventoryP1 : inventoryP2;

    /// Add one item by name to a player�s inventory
    public void AddToInventory(int playerIndex, string itemName)
    {
        (playerIndex == 1 ? inventoryP1 : inventoryP2).Add(itemName);
    }

    /// Remove one item by name. Returns true if removed.
    public bool RemoveFromInventory(int playerIndex, string itemName)
    {
        return (playerIndex == 1 ? inventoryP1 : inventoryP2).Remove(itemName);
    }


    /// Wipe inventories 
    public void ClearInventories()
    {
        inventoryP1.Clear();
        inventoryP2.Clear();
    }

    public void ShowGameOverScreen(string finaltext)
    {
        if (gameOverUI != null)
        {
            gameOverUI.gameObject.SetActive(true);
            gameOverUI.gameOverText.text = finaltext;
        }
    }

    public void CheckWinLossConditions()
    {
        if (players != null && players.Length == 0)
        {
            gameOverUI.gameObject.SetActive(true);
            gameOverUI.gameOverText.text = "GAME OVER";
        }
        else if (enemies != null && enemies.Length == 0)
        {
            gameOverUI.gameObject.SetActive(true);
            gameOverUI.gameOverText.text = "YOU WON";
        }
    }
}
