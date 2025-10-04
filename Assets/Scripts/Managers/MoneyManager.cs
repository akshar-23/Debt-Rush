using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    // Singleton instance
    public static MoneyManager Instance { get; private set; }

    [SerializeField]
    private int moneyAmount = 0;


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

    public void AddMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("The amount was negative!");
            return;
        }

        moneyAmount += amount;
        Debug.Log("Money added: " + amount + " - Total: " + moneyAmount);
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
    }
}
