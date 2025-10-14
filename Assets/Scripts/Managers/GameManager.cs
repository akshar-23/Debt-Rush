using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameOverScreen gameOverUI;
    public static GameManager Instance { get; private set; }

    public Character[] players;
    public Character[] enemies;

    [System.Serializable]
    public struct ShopItem
    {
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
    }

   
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
            ShowGameOverScreen("GAME OVER");
        }
        else if (enemies != null && enemies.Length == 0)
        {
            ShowGameOverScreen("YOU WON");
        }
    }
}
