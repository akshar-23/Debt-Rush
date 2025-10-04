using UnityEngine;
using System.Collections;

public class CellphoneService : Item
{
    // Time for the service to appear in seconds
    [SerializeField] public int delay = 10;

    public CellphoneService(string name, string description, int value, int cost, Sprite icon = null)
        : base(name, description, cost, icon)
    {

    }

    public override void Use()
    {
        //base.Use();
        //moneyManager.SubtractMoney(cost);
        StartCoroutine(ExecuteWithDelay(delay));
    }

    private IEnumerator ExecuteWithDelay(int delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Using item: " + itemName);
        MoneyManager.Instance.SubtractMoney(cost);
        Debug.Log("You pay " + cost + " money!");
    }
}
