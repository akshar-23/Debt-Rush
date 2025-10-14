using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;


public class GameManager : MonoBehaviour
{
    public GameObject gameOverUI;
    private bool isGameOver;
    public static GameManager Instance { get; private set; }

    public Character[] players;
    public Character[] enemies;
    [SerializeField] List<string> inventoryP1 = new();
    [SerializeField] List<string> inventoryP2 = new();

    [Header("Timer Settings")]
    [SerializeField]  private float timer = 0f;
    [SerializeField]  private bool isTimerRunning = true;
    public TextMeshProUGUI timerText;

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
        isGameOver = false;

        if (isTimerRunning)
        {
            UpdateTimerDisplay();
        }
        else
        {
            timerText.text = "";
        }
        
    }

    void Update()
    {
        if (isGameOver) return;

        if (isTimerRunning) {            
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            else
            {
                isTimerRunning = false;
                timer = 0;                
                Debug.Log("Time’s up!");
            }

            UpdateTimerDisplay();
        }



        CheckWinLossConditions();

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
        if ((players[0] == null && players[1] == null) || timer == 0)
        {
            ShowGameOverScreen("GAME OVER");
        }
        else if(enemies == null || enemies.Length == 0 || enemies.All(e => e == null))
        {
            ShowGameOverScreen("YOU WON");
        }
    }

    void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetTimer()
    {
        timer = 0f;
        isTimerRunning = true;
    }
}
