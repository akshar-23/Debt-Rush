using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    // Singleton instance
    public static MoneyManager Instance { get; private set; }

    public TextMeshProUGUI moneyText;

    [Header("SFX")]
    [SerializeField] private AudioClip pickupSound;

    [SerializeField]
    private int moneyAmount = 0;
    public int targetMoney = 1000;

    // Event that fires whenever money changes
    public static event System.Action OnMoneyChanged;

    private void Awake()
    {
        // Ensure only one instance exists
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
        moneyAmount = 0;
        targetMoney = 1000;
        OnMoneyChanged = null; // Clear all subscribers
    }

    private void Start()
    {
        UpdateMoneyText();
    }

    private void UpdateMoneyText()
    {
        if (moneyText != null)
        {
            moneyText.text = moneyAmount.ToString();
        }
    }

    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("The amount was negative!");
            return;
        }

        // Play sound (optional)
        if (pickupSound != null)
        {
            if(AudioManager.Instance != null) AudioManager.Instance.PlaySFX(pickupSound);
        }

        moneyAmount += amount;
        Debug.Log("Money added: " + amount + " - Total: " + moneyAmount);

        UpdateMoneyText();
        
        // Notify all subscribers that money changed
        OnMoneyChanged?.Invoke();
    }

    public void SubtractMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Tried to subtract a negative amount!");
            return;
        }

        if (moneyAmount - amount < 0)
        {
            Debug.LogWarning("Not enough money to subtract!");
            return;
        }

        moneyAmount -= amount;
        Debug.Log("Money subtracted: " + amount + " - Total: " + moneyAmount);

        UpdateMoneyText();
        
        // Notify all subscribers that money changed
        OnMoneyChanged?.Invoke();
    }

    public int GetMoneyAmount() { 
        return moneyAmount; 
    }

    public void ResetMoneyAmount()
    {
        moneyAmount = 0;
        Debug.Log("Money reseted - Total: " + moneyAmount);
        UpdateMoneyText();

        // Notify all subscribers that money changed
        OnMoneyChanged?.Invoke();
    }
}
