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

    [System.Serializable]
    public struct ShopItem
    {
        public int Id;
        public string Name;
        public int Price;
        [TextArea] public string Description;
        public int Limit;                
        [SerializeField, HideInInspector] public int Current;  
    }

    [Header("Shop Data (edit in Inspector)")]
    [SerializeField] private List<ShopItem> shopItemsP1 = new();
    [SerializeField] private List<ShopItem> shopItemsP2 = new();

   
    [SerializeField, HideInInspector] private List<ShopItem> inventoryP1 = new();
    [SerializeField, HideInInspector] private List<ShopItem> inventoryP2 = new();

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
        if (gameOverUI != null) gameOverUI.gameObject.SetActive(false);
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
        int i = list.FindIndex(x => x.Name == name);
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
    public void SetTimer(float time)
    {
        timer = time;
        isTimerRunning = true;
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
