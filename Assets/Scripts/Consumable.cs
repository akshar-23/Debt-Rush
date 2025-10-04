using UnityEngine;
using System.Collections;

public class Consumable : Item
{
    public int uses;

    public Consumable(string name, string description, int value, int cost, Sprite icon = null)
        : base(name, description, cost, icon)
    {
        
    }

    public override void Use()
    {
        //base.Use();
        //Debug.Log("You pay " + cost + " money!");
        //moneyManager.SubtractMoney(cost);
        StartCoroutine(ExecuteWithDelay(10f));
        uses--;
    }

    private IEnumerator ExecuteWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Using item: " + itemName);
        moneyManager.SubtractMoney(cost);
        Debug.Log("You pay " + cost + " money!");
    }
}
